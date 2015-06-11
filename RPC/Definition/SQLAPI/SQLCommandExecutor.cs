using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Common;
using System.ComponentModel;

namespace RPC.SQLAPI
{
    /// <summary>
    /// An RPC method executor to execute SQLAPI methods.
    /// </summary>
    public class SQLCommandExecutor : IExecutor
    {
        private readonly ValidSQLAPIMethod validMethod;

        public SQLAPIAttribute SQLAttr { get { return validMethod.ValidInterface.APIAttr; } }
        public RPCAttribute RPCAttr { get { return validMethod.ValidInterface.APIAttr; } }
        public Type InterfaceType { get { return validMethod.ValidInterface.InterfaceType; } }
        public MethodInfo Method { get { return validMethod.Method; } }
        public HttpMethodAttribute HttpMethodAttr { get { return validMethod.CommandAttr; } }
        public SQLCommandAttribute CommandAttr { get { return validMethod.CommandAttr; } }

        public IDictionary<string, RPCMethodParameter> Parameters { get { return validMethod.Parameters; } }

        /// <summary>
        /// Determines if the command to be executed could produce side-effects (storing data).
        /// </summary>
        public bool CommandHasSideEffects { get { return !(CommandAttr is SQLQueryAttribute); } }

        internal SQLCommandExecutor(ValidSQLAPIMethod validMethod)
        {
            Debug.Assert(validMethod != null);

            this.validMethod = validMethod;
        }

        public Task<object> Execute(IDictionary<string, object> parameterValues)
        {
            // Choose based on method's result container kind:
            switch (validMethod.ResultContainerKind)
            {
                case ResultContainerKind.List:
                    return ExecuteListNonGeneric(parameterValues);
                case ResultContainerKind.Single:
                    return ExecuteSingleNonGeneric(parameterValues);
                case ResultContainerKind.NoResult:
                    return ExecuteNoResultNonGeneric(parameterValues);
                default:
                    throw new InvalidOperationException("Could not determine kind of method result container");
            }
        }

        static readonly string[] lineSeparators = new string[1] { "\r\n" };

        public Task<object> ExecuteMeta(string metaCommand, IDictionary<string, object> metaParameterValues)
        {
            switch (metaCommand.ToLower())
            {
                // Reports the final processed command text:
                case "command":
                    {
                        bool splitLines = false;
                        object lines;
                        if (metaParameterValues.TryGetValue("lines", out lines))
                        {
                            splitLines = (bool)lines;
                        }

                        if (splitLines)
                        {
                            return Task.FromResult((object)new
                            {
                                command = (object)validMethod.CommandText.Split(lineSeparators, StringSplitOptions.None)
                            });
                        }
                        else
                        {
                            return Task.FromResult((object)new
                            {
                                command = (object)validMethod.CommandText
                            });
                        }
                    }

                // Reports the unprocessed command text pulled from the SQLCommandAttribute or the local override file:
                case "rawcommand":
                    {
                        bool splitLines = false;
                        object lines;
                        if (metaParameterValues.TryGetValue("lines", out lines))
                        {
                            splitLines = (bool)lines;
                        }

                        if (splitLines)
                        {
                            return Task.FromResult((object)new
                            {
                                rawcommand = (object)validMethod.RawCommandText.Split(lineSeparators, StringSplitOptions.None)
                            });
                        }
                        else
                        {
                            return Task.FromResult((object)new
                            {
                                rawcommand = (object)validMethod.RawCommandText
                            });
                        }
                    }

#if false
                // For debugging only!
                case "basedirectory":
                    return Task.FromResult((object)new { baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory });
#endif

                default: throw new InvalidOperationException("Unrecognized meta command '{0}'".F(metaCommand));
            }
        }

        public async Task<object> ExecuteSingleNonGeneric(IDictionary<string, object> parameterValues)
        {
            // TResult = ResultType

            // Create an instance of the result converter to use:
            object converter = Activator.CreateInstance(validMethod.ConverterType);
            var converterMethod = validMethod.ConverterType.GetMethod("Convert");
            var mbType = typeof(Maybe<>).MakeGenericType(validMethod.ResultType);

            // Make sure the parameter value dictionary is non-null:
            parameterValues = parameterValues ?? new Dictionary<string, object>(0);

            using (var db = new SqlConnection(validMethod.AsyncConnStr))
            using (var cmd = buildCommand(db, parameterValues))
            {
                // Open the connection:
                await db.OpenAsync();

                // Wait for the data to come back:
                using (var dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SingleRow | CommandBehavior.SingleResult))
                {
                    if (!await dr.ReadAsync())
                        return Activator.CreateInstance(mbType); // Maybe<TResult>.Nothing;

                    var rowreader = new RowReader(dr);
                    var item = converterMethod.Invoke(converter, new object[1] { rowreader }); // converter.Convert(rowreader);
                    return Activator.CreateInstance(mbType, new object[1] { item }); //new Maybe<TResult>(item);
                }
            }
        }

        public async Task<object> ExecuteListNonGeneric(IDictionary<string, object> parameterValues)
        {
            // Create an instance of the result converter to use:
            //var converter = (SQLResultConverter<TResult>)Activator.CreateInstance(ConverterType);
            object converter = Activator.CreateInstance(validMethod.ConverterType);
            var converterMethod = validMethod.ConverterType.GetMethod("Convert");
            var listType = typeof(List<>).MakeGenericType(validMethod.ResultType);

            // Make sure the parameter value dictionary is non-null:
            parameterValues = parameterValues ?? new Dictionary<string, object>(0);

            using (var db = new SqlConnection(validMethod.AsyncConnStr))
            using (var cmd = buildCommand(db, parameterValues))
            {
                // Open the connection:
                await db.OpenAsync();

                // Wait for the data to come back:
                using (var dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SingleResult))
                {
                    // If nothing available, don't waste memory allocating anything yet:
                    if (!await dr.ReadAsync())
                        return Activator.CreateInstance(listType, new object[1] { 0 }); //new List<TResult>(0);

                    // Now allocate a list and a RowReader and start reading/converting:
                    var rowreader = new RowReader(dr);
                    var rrArgs = new object[1] { rowreader };
                    var list = (System.Collections.IList)Activator.CreateInstance(listType); // new List<TResult>();
                    do
                    {
                        var item = converterMethod.Invoke(converter, rrArgs); // converter.Convert(rowreader);
                        list.Add(item);
                    } while (await dr.ReadAsync());

                    // Return the list:
                    return list;
                }
            }
        }

        /// <summary>
        /// Executes the query asynchronously and returns a list of results.
        /// </summary>
        /// <returns></returns>
        public async Task<CommandResult> ExecuteNoResult(IDictionary<string, object> parameterValues = null)
        {
            // Verify the method's return type is `Task<CommandResult>`:
            var resultType = typeof(Task<CommandResult>);
            if (this.Method.ReturnType != resultType)
                throw new ArgumentException("Method '{0}' on interface '{1}' return type `{2}` is not `{3}`".F(this.Method.Name, this.InterfaceType.FullName, this.Method.ReturnType.GetCSharpDisplayName(), resultType.GetCSharpDisplayName()), "method");

            // Make sure the parameter value dictionary is non-null:
            parameterValues = parameterValues ?? new Dictionary<string, object>(0);

            using (var db = new SqlConnection(validMethod.AsyncConnStr))
            using (var cmd = buildCommand(db, parameterValues))
            {
                // Open the connection:
                await db.OpenAsync();

                // Execute the command:
                var result = await cmd.ExecuteNonQueryAsync();

                // Return the number of rows affected:
                return new CommandResult { RecordsAffected = result };
            }
        }

        public async Task<object> ExecuteNoResultNonGeneric(IDictionary<string, object> parameterValues)
        {
            return (object)await ExecuteNoResult(parameterValues);
        }

        protected SqlCommand buildCommand(SqlConnection db, IDictionary<string, object> parameterValues)
        {
            Debug.Assert(db != null);
            Debug.Assert(parameterValues != null);

            var cmd = db.CreateCommand();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = validMethod.CommandText;

            // Bind all parameters:
            foreach (var param in validMethod.Parameters.Values)
            {
                var sqlParam = ((SQLMethodParameter)param).Descriptor;

                // Create the SqlParameter and mutate it with the attribute values:
                var cmdprm = cmd.Parameters.Add(param.Name, sqlParam.Type);
                // This applies attribute info to the SqlParameter, including overwriting the Name:
                ((SQLMethodParameter)param).Descriptor.ApplyTo(cmdprm);

                // Add the value from the dictionary:
                object value;
                if (parameterValues.TryGetValue(param.Name, out value))
                    cmdprm.Value = value ?? DBNull.Value;
                else
                    cmdprm.Value = DBNull.Value;
            }

            return cmd;
        }
    }

    /// <summary>
    /// Executes SQL queries from declarative interfaces.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class SQLCommandExecutor<TResult> : SQLCommandExecutor
    {
        internal SQLCommandExecutor(ValidSQLAPIMethod validMethod)
            : base(validMethod)
        {
        }

        public Task<List<TResult>> ExecuteList(IDictionary<string, object> parameterValues = null)
        {
            return ExecuteListNonGeneric(parameterValues).ContinueWith((t) => (List<TResult>)t.Result);
        }

        public Task<Maybe<TResult>> ExecuteSingle(IDictionary<string, object> parameterValues = null)
        {
            return ExecuteSingleNonGeneric(parameterValues).ContinueWith((t) => (Maybe<TResult>)t.Result);
        }
    }
}

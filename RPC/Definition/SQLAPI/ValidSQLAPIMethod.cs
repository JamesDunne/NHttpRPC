using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    internal class ValidSQLAPIInterface
    {
        public readonly Type InterfaceType;
        public readonly SQLAPIAttribute APIAttr;
        public readonly ConcurrentDictionary<string, ValidSQLAPIMethod> Methods;

        internal ValidSQLAPIInterface(Type interfaceType, SQLAPIAttribute apiAttr)
        {
            Debug.Assert(interfaceType != null);
            Debug.Assert(apiAttr != null);

            this.InterfaceType = interfaceType;
            this.APIAttr = apiAttr;
            this.Methods = new ConcurrentDictionary<string, ValidSQLAPIMethod>(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal class ValidSQLAPIMethod
    {
        public readonly ValidSQLAPIInterface ValidInterface;
        public readonly MethodInfo Method;
        public readonly SQLCommandAttribute CommandAttr;

        public readonly ReadOnlyDictionary<string, RPCMethodParameter> Parameters;

        public readonly Type ResultType;
        public readonly Type ResultContainerType;
        public readonly ResultContainerKind ResultContainerKind;
        public readonly Type ConverterType;

        public readonly string CommandText;
        public readonly string RawCommandText;

        public readonly SqlAsyncConnectionString AsyncConnStr;

        // Caches:
        static readonly ConcurrentDictionary<string, SqlAsyncConnectionString> _connStringCache = new ConcurrentDictionary<string, SqlAsyncConnectionString>();

        internal ValidSQLAPIMethod(ValidSQLAPIInterface intf, MethodInfo method, SQLCommandAttribute cmdAttr)
        {
            Debug.Assert(intf != null);
            Debug.Assert(method != null);
            Debug.Assert(cmdAttr != null);

            this.ValidInterface = intf;
            this.Method = method;
            this.CommandAttr = cmdAttr;

            // Discover the basic result type used to represent data rows based on the method's return type:
            var resultTypes = discoverResultTypes(method);
            this.ResultType = resultTypes.ResultType;
            this.ResultContainerType = resultTypes.ContainerType;

            if (ResultType == typeof(CommandResult))
                ResultContainerKind = ResultContainerKind.NoResult;
            else
            {
                var gen = ResultContainerType.GetGenericTypeDefinition();
                if (gen == typeof(List<>))
                    ResultContainerKind = ResultContainerKind.List;
                else if (gen == typeof(Maybe<>))
                    ResultContainerKind = ResultContainerKind.Single;
                else
                    throw new Exception("Unrecognized method return type");

                // Discover the nested `Converter` type:
                this.ConverterType = discoverConverterType(ResultType);
            }

            // Turn the method's `ParameterInfo[]` into a dictionary keyed by parameter name:
            var methodParams = method.GetParameters();
            var parameters = new Dictionary<string, RPCMethodParameter>(methodParams.Length);
            foreach (var parameterInfo in methodParams)
            {
                SQLParamDescriptor descriptor;
                SQLParamAttribute attr;

                // Check if the parameter has a SQLParamAttribute declared:
                if ((attr = parameterInfo.GetCustomAttribute<SQLParamAttribute>()) != null)
                {
                    descriptor = attr.Descriptor;
                }
                // Check if the parameter's type declares a SQLParamAttribute:
                else if ((attr = TypeDescriptor.GetAttributes(parameterInfo.ParameterType).OfType<SQLParamAttribute>().SingleOrDefault()) != null)
                {
                    descriptor = attr.Descriptor;
                }
                else
                {
                    throw new ArgumentException("Method '{0}' on interface '{1}' parameter '{2}' has no SQLParamAttribute declared".F(method.Name, intf.InterfaceType.FullName, parameterInfo.Name), "method");
                }

                // NOTE(jsd): This dictionary insert should never cause key conflicts because parameter names must be unique.
                parameters.Add(parameterInfo.Name, new SQLMethodParameter(parameterInfo, descriptor));
            }
            this.Parameters = new ReadOnlyDictionary<string, RPCMethodParameter>(parameters);

            // Make sure we have an asynchronous-enabled connection string:
            string connName = cmdAttr.ConnectionName ?? intf.APIAttr.ConnectionName;
            if (connName.IsNullOrEmpty())
                throw new Exception("No connection name specified in either the method's SQLCommandAttribute or the SQLAPIAttribute!");

            this.AsyncConnStr = _connStringCache.GetOrAdd(
                connName,
                (cn) => new SqlAsyncConnectionString(global::Common.Configuration.EnvironmentSettings.ConnectionStrings[cn])
            );

            // NOTE(jsd): Caching of this computed command text is not necessary since any file changes under the bin/ folder
            // will cause an app pool restart anyway.

            // Compute the command text:
            string text = CommandAttr.CommandText;

            // Override the command text with contents from a file?
            try
            {
                // Search for the command text file:
                string relSQLPath = Path.Combine(
                    "SQLAPI",
                    SanitizePath(ValidInterface.APIAttr.Category),
                    SanitizePath(ValidInterface.APIAttr.Version),
                    SanitizePath(ValidInterface.APIAttr.Name),
                    SanitizeFileName(Method.Name) + ".sql"
                );
                string absSQLPath = FindLocalFile(relSQLPath);

                // If we have it, load its contents:
                if (absSQLPath != null)
                {
                    Trace.WriteLine("Overriding method text with contents of '{0}'".F(absSQLPath), "SQLAPI");
                    text = File.ReadAllText(absSQLPath);
                }
            }
            catch
            {
                // Don't care.
            }

            RawCommandText = text;

            // Process the command text:
            var textProcessorType = CommandAttr.TextProcessorType ?? ValidInterface.APIAttr.TextProcessorType;
            if (textProcessorType == null)
            {
                // No text processor:
                CommandText = text;
            }
            else
            {
                // Let exceptions be thrown.
                var textProcessor = (SQLTextProcessor)Activator.CreateInstance(textProcessorType);

                CommandText = textProcessor.ProcessText(text);
            }
        }

        private static string SanitizePath(string path)
        {
            return path.RemoveAllCharacters(Path.GetInvalidPathChars());
        }

        private static string SanitizeFileName(string fileName)
        {
            return fileName.RemoveAllCharacters(Path.GetInvalidFileNameChars());
        }

        private static string FindLocalFile(string relativePath)
        {
            try
            {
                var baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

                string path = System.IO.Path.Combine(baseDirectory, relativePath);
                if (System.IO.File.Exists(path))
                    return path;

                // BaseDirectory points to the root folder of a website we're hosted in, so try its bin/ subfolder next:
                var baseBinDirectory = System.IO.Path.Combine(baseDirectory, "bin");
                path = System.IO.Path.Combine(baseBinDirectory, relativePath);
                if (System.IO.File.Exists(path))
                    return path;
            }
            catch
            {
                // Don't care.
            }

            // Could not locate the file:
            return null;
        }

        protected internal struct ResultTypes
        {
            public readonly Type ResultType;
            public readonly Type ContainerType;

            public ResultTypes(Type resultType, Type containerType)
            {
                ResultType = resultType;
                ContainerType = containerType;
            }
        }

        protected internal static ResultTypes discoverResultTypes(MethodInfo method)
        {
            if (!method.ReturnType.IsGenericType)
                throw new ArgumentException("Method return type must be one of: `Task<CommandResult>`, `Task<Maybe<U>>`, `Task<List<U>>`");
            if (method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
                throw new ArgumentException("Method return type must be one of: `Task<CommandResult>`, `Task<Maybe<U>>`, `Task<List<U>>`");

            // Get the `T` from `Task<T>`:
            var resultType = method.ReturnType.GetGenericArguments()[0];

            // `CommandResult` is for commands that don't return a result set:
            if (resultType == typeof(CommandResult))
                return new ResultTypes(resultType, resultType);

            // `T` should be a generic type at this point:
            if (!resultType.IsGenericType)
                throw new ArgumentException("Method return type must be one of: `Task<CommandResult>`, `Task<Maybe<U>>`, `Task<List<U>>`");
            var genericTypeDef = resultType.GetGenericTypeDefinition();

            // Pull the `U` from `List<U>` or `Maybe<U>`:
            if (genericTypeDef == typeof(List<>))
                return new ResultTypes(resultType.GetGenericArguments()[0], resultType);
            else if (genericTypeDef == typeof(Maybe<>))
                return new ResultTypes(resultType.GetGenericArguments()[0], resultType);
            else
                throw new ArgumentException("Method return type must be one of: `Task<CommandResult>`, `Task<Maybe<U>>`, `Task<List<U>>`");
        }

        protected static Type discoverConverterType(Type resultType)
        {
            if (resultType == null)
                return null;
            if (resultType == typeof(CommandResult))
                return null;

            // Handle basic types:
            if (BasicResultConverter<object>.CanConvert(resultType))
                return typeof(BasicResultConverter<>).MakeGenericType(resultType);

            var converterType = resultType.GetNestedType("Converter");
            if (converterType == null)
                throw new ArgumentException("Could not find nested `Converter` type within result type `{0}`".F(resultType.FullName));

            var expectedResultConverterType = typeof(SQLResultConverter<>).MakeGenericType(resultType);
            if (!converterType.GetInterfaces().Any(i => i.IsGenericType && i == expectedResultConverterType))
                throw new ArgumentException(
                    "Nested Converter class '{0}' must implement the `{1}` interface".F(converterType.FullName,
                        expectedResultConverterType.GetCSharpDisplayName()), "resultType");

            return converterType;
        }
    }
}

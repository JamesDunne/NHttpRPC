﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Data
{
    public delegate T ReadResultDelegate<T>(SqlParameterCollection prms, IEnumerable<AutoDataRecord> rows);

    /// <summary>
    /// A simple wrapper around an <see cref="IDataRecord"/> that auto-increments the column ordinal on each successive function call to get a column value.
    /// </summary>
    /// <example>
    /// from row in (IEnumerable&lt;AutoDataRecord&gt;)rows
    /// select new {
    ///   a = row.Int32(),
    ///   b = row.Int32(),
    ///   c = row.String()
    /// }
    /// </example>
    public struct AutoDataRecord
    {
        private readonly IDataRecord _row;
        private int _ord;

        public AutoDataRecord(IDataRecord row)
        {
            _row = row;
            _ord = 0;
        }

        public int Ordinal { get { return _ord; } }
        public int FieldCount { get { return _row.FieldCount; } }

        public string Name(int i) { return _row.GetName(i); }
        public Type FieldType(int i) { return _row.GetFieldType(i); }
        public string DataTypeName(int i) { return _row.GetDataTypeName(i); }

        public object Value() { int i = _ord++; return _row.GetValue(i); }
        public Byte Byte() { int i = _ord++; return _row.GetByte(i); }
        public Byte? NullByte() { int i = _ord++; return _row.IsDBNull(i) ? (Byte?)null : _row.GetByte(i); }
        public Int16 Int16() { int i = _ord++; return _row.GetInt16(i); }
        public Int16? NullInt16() { int i = _ord++; return _row.IsDBNull(i) ? (Int16?)null : _row.GetInt16(i); }
        public Int32 Int32() { int i = _ord++; return _row.GetInt32(i); }
        public Int32? NullInt32() { int i = _ord++; return _row.IsDBNull(i) ? (Int32?)null : _row.GetInt32(i); }
        public Int64 Int64() { int i = _ord++; return _row.GetInt64(i); }
        public Int64? NullInt64() { int i = _ord++; return _row.IsDBNull(i) ? (Int64?)null : _row.GetInt64(i); }
        public Decimal Decimal() { int i = _ord++; return _row.GetDecimal(i); }
        public Decimal? NullDecimal() { int i = _ord++; return _row.IsDBNull(i) ? (Decimal?)null : _row.GetDecimal(i); }
        public String String() { int i = _ord++; return _row.GetString(i); }
        public String NullString() { int i = _ord++; return _row.IsDBNull(i) ? (String)null : _row.GetString(i); }
        public String StringTrim() { int i = _ord++; return _row.GetString(i).Trim(); }
        public String NullStringTrim() { int i = _ord++; return _row.IsDBNull(i) ? (String)null : _row.GetString(i).Trim(); }
        public Boolean Boolean() { int i = _ord++; return _row.GetBoolean(i); }
        public Boolean? NullBoolean() { int i = _ord++; return _row.IsDBNull(i) ? (Boolean?)null : _row.GetBoolean(i); }
        public DateTime DateTime() { int i = _ord++; return _row.GetDateTime(i); }
        public DateTime? NullDateTime() { int i = _ord++; return _row.IsDBNull(i) ? (DateTime?)null : _row.GetDateTime(i); }
        public DateTimeOffset DateTimeOffset() { int i = _ord++; return (_row as SqlDataReader).GetDateTimeOffset(i); }
        public DateTimeOffset? NullDateTimeOffset() { int i = _ord++; return _row.IsDBNull(i) ? (DateTimeOffset?)null : (_row as SqlDataReader).GetDateTimeOffset(i); }

        // TODO(jsd): More types!
    }

    public struct AutoMetadataRecord
    {
        private readonly IDataRecord _row;

        public AutoMetadataRecord(IDataRecord row)
        {
            _row = row;
        }

        public int FieldCount { get { return _row.FieldCount; } }

        public string Name(int i) { return _row.GetName(i); }
        public Type FieldType(int i) { return _row.GetFieldType(i); }
        public string DataTypeName(int i) { return _row.GetDataTypeName(i); }
    }

    /// <summary>
    /// Represents a connection string that is guaranteed to have been altered to enable asynchronous processing.
    /// </summary>
    public struct SqlAsyncConnectionString
    {
        private readonly string _connectionString;

        /// <summary>
        /// Creates a connection string prepared for enabling asynchronous processing with an optional connection timeout.
        /// </summary>
        /// <param name="connectionString">The initial connection string to modify.</param>
        /// <param name="connectTimeout">Time to wait for a connection to open (in seconds). Default is 5.</param>
        public SqlAsyncConnectionString(string connectionString, [Optional] int? connectTimeout)
        {
            // Make sure asynchronous processing is enabled for the connection string:
            var csb = new SqlConnectionStringBuilder(connectionString);
            csb.AsynchronousProcessing = true;
            csb.ConnectTimeout = connectTimeout ?? global::Common.Configuration.EnvironmentSettings.Get("DatabaseTimeout.Connect.Seconds", Int32.Parse, 5);

            _connectionString = csb.ConnectionString;
        }

        /// <summary>
        /// Creates a connection string prepared for enabling asynchronous processing with a default connection timeout of 5 seconds.
        /// </summary>
        /// <param name="connectionString">The initial connection string to modify.</param>
        public SqlAsyncConnectionString(string connectionString)
            : this(connectionString, null)
        {
        }

        public static implicit operator string(SqlAsyncConnectionString cstr)
        {
            return cstr._connectionString;
        }
    }

    /// <summary>
    /// Represents a SQL command that can be easily executed asynchronously.
    /// </summary>
    public sealed class SqlAsyncCommand
    {
        #region Private fields

        private readonly SqlConnection _db;
        private readonly SqlCommand _cmd;

        /// <summary>
        /// Default exception handler. Override with SetExceptionHandler.
        /// </summary>
        private static readonly Action<SqlAsyncCommand, Exception> _errDefault = (cmd, ex) =>
        {
            Trace.WriteLine(ex.ToString(), "SqlAsyncCommand");
        };

        #endregion

        #region Constructors

        private SqlAsyncCommand(CommandType commandType, string commandText, SqlAsyncConnectionString asyncConnectionString, int? commandTimeout)
        {
            this._db = new SqlConnection(asyncConnectionString);

            // Create a command to execute the query with:
            this._cmd = _db.CreateCommand();
            this._cmd.CommandTimeout = commandTimeout ?? global::Common.Configuration.EnvironmentSettings.Get("DatabaseTimeout.Command.Seconds", Int32.Parse, 60);
            this._cmd.CommandText = commandText;
            this._cmd.CommandType = commandType;
        }

        #endregion

        #region Static creator methods

        /// <summary>
        /// Creates an asynchronous command to execute an inline SQL query.
        /// </summary>
        /// <param name="queryText">The inline SQL query text to execute</param>
        /// <param name="asyncConnectionString">The asynchronous connection string</param>
        /// <returns></returns>
        public static SqlAsyncCommand Query(string queryText, SqlAsyncConnectionString asyncConnectionString)
        {
            return Query(queryText, asyncConnectionString, null);
        }

        /// <summary>
        /// Creates an asynchronous command to execute an inline SQL query with an optional execution timeout.
        /// </summary>
        /// <param name="queryText">The inline SQL query text to execute</param>
        /// <param name="asyncConnectionString">The asynchronous connection string</param>
        /// <param name="commandTimeout">An optional command execution timeout value (in seconds). Default is 60.</param>
        /// <returns></returns>
        public static SqlAsyncCommand Query(string queryText, SqlAsyncConnectionString asyncConnectionString, [Optional] int? commandTimeout)
        {
            return new SqlAsyncCommand(CommandType.Text, queryText, asyncConnectionString, commandTimeout);
        }

        /// <summary>
        /// Creates an asynchronous command to execute a SQL stored procedure.
        /// </summary>
        /// <param name="procedureName">The SQL stored procedure name to execute</param>
        /// <param name="asyncConnectionString">The asynchronous connection string</param>
        /// <returns></returns>
        public static SqlAsyncCommand StoredProcedure(string procedureName, SqlAsyncConnectionString asyncConnectionString)
        {
            return StoredProcedure(procedureName, asyncConnectionString, null);
        }

        /// <summary>
        /// Creates an asynchronous command to execute a SQL stored procedure with an optional execution timeout.
        /// </summary>
        /// <param name="procedureName">The SQL stored procedure name to execute</param>
        /// <param name="asyncConnectionString">The asynchronous connection string</param>
        /// <param name="commandTimeout">An optional command execution timeout value (in seconds). Default is 60.</param>
        /// <returns></returns>
        public static SqlAsyncCommand StoredProcedure(string procedureName, SqlAsyncConnectionString asyncConnectionString, [Optional] int? commandTimeout)
        {
            return new SqlAsyncCommand(CommandType.StoredProcedure, procedureName, asyncConnectionString, commandTimeout);
        }

        #endregion

        #region Query parameters

        /// <summary>
        /// Adds a command parameter with a name and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithValue(string paramName, object value)
        {
            var prm = this._cmd.Parameters.AddWithValue(paramName, value);
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name and type.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithType(string paramName, SqlDbType type)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, and direction.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="direction">Parameter direction</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeDirection(string paramName, SqlDbType type, ParameterDirection direction)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            prm.Direction = direction;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeValue(string paramName, SqlDbType type, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            prm.Value = value;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, direction, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeDirectionValue(string paramName, SqlDbType type, ParameterDirection direction, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            prm.Direction = direction;
            prm.Value = value;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, and size.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSize(string paramName, SqlDbType type, int size)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, size, and direction.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="direction">Parameter direction</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSizeDirection(string paramName, SqlDbType type, int size, ParameterDirection direction)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            prm.Direction = direction;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, size, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSizeValue(string paramName, SqlDbType type, int size, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            prm.Value = value;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, size, direction, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSizeDirectionValue(string paramName, SqlDbType type, int size, ParameterDirection direction, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            prm.Direction = direction;
            prm.Value = value;
            return prm;
        }

        #endregion

        #region Command properties

        /// <summary>
        /// Gets the command parameter collection.
        /// </summary>
        public SqlParameterCollection Parameters { get { return _cmd.Parameters; } }

        /// <summary>
        /// Gets the command text.
        /// </summary>
        public string CommandText { get { return _cmd.CommandText; } }

        /// <summary>
        /// Gets the command type.
        /// </summary>
        public CommandType CommandType { get { return _cmd.CommandType; } }

        #endregion

        #region Asynchronous execution

        /// <summary>
        /// Enumerates a SqlDataReader, wrapping each row in an <see cref="AutoDataRecord"/> struct.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static IEnumerable<AutoDataRecord> _enumerate(SqlDataReader dr)
        {
            try
            {
                while (dr.Read()) yield return new AutoDataRecord(dr);
            }
            finally
            {
                dr.Close();
            }
        }

        #region ExecuteReader

        /// <summary>
        /// State object to pass along to the AsyncCallback delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        sealed class ExecuteReaderState<T, Tstate>
        {
            public readonly SqlAsyncCommand query;
            public readonly ReadResultDelegate<T> readResult;
            public readonly Action<T, Tstate> useResult;
            public readonly Action<SqlAsyncCommand, Exception> err;
            public readonly Tstate state;

            public ExecuteReaderState(SqlAsyncCommand query, ReadResultDelegate<T> readResult, Action<T, Tstate> useResult, Action<SqlAsyncCommand, Exception> err, Tstate state)
            {
                this.query = query;
                this.readResult = readResult;
                this.useResult = useResult;
                this.err = err;
                this.state = state;
            }
        }

        /// <summary>
        /// The asynchronous completion handler for ExecuteReader.
        /// </summary>
        /// <remarks>Executed on an IOCP thread.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="iar"></param>
        private static void _completeReader<T, Tstate>(IAsyncResult iar)
        {
            var rst = (ExecuteReaderState<T, Tstate>)iar.AsyncState;
            try
            {
                T result;

                // Get the SqlDataReader from the asynchronous result:
                using (SqlDataReader dr = rst.query._cmd.EndExecuteReader(iar))
                    result = rst.readResult(rst.query._cmd.Parameters, _enumerate(dr));

                // We no longer need the connection open:
                rst.query._cmd.Dispose();
                rst.query._db.Close();

                // Hand the computed custom result from reading the SqlDataReader to the custom action:
                rst.useResult(result, rst.state);
            }
            catch (Exception ex)
            {
                rst.query._cmd.Dispose();
                rst.query._db.Close();

                // TODO(jsd): If this throws an exception ... well ... let's just say I hate exceptions in general.
                rst.err(rst.query, ex);
            }

            rst = null;
        }

        /// <summary>
        /// Asynchronously executes the current command. When the asynchronous operation completes, <paramref name="readResult"/>
        /// will be called to generate a custom <typeparamref name="T"/>-typed result which will then be handed to <paramref name="useResult"/>
        /// after the reader and connection are closed.
        /// </summary>
        /// <typeparam name="T">Custom type used to represent query results with.</typeparam>
        /// <param name="readResult">Function called to generate a custom <typeparamref name="T"/>-typed result which will then be handed to <paramref name="useResult"/>.</param>
        /// <param name="useResult">Action called to perform some work with the custom result.</param>
        public void ExecuteReader<T, Tstate>(Action<SqlAsyncCommand, Exception> err, ReadResultDelegate<T> readResult, Action<T, Tstate> useResult, Tstate state)
        {
            var errorHandler = (err ?? _errDefault);
            try
            {
                // Blocking call, unfortunately:
                this._db.Open();

                // Wrap up all our state for the completion handler:
                var ast = new ExecuteReaderState<T, Tstate>(this, readResult, useResult, errorHandler, state);

                // Fire off the query:
                this._cmd.BeginExecuteReader(_completeReader<T, Tstate>, (object)ast, CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                // TODO(jsd): If this throws an exception ... well ... let's just say I hate exceptions in general.
                errorHandler(this, ex);
            }
        }

        #endregion

        #region ExecuteNonQuery<T>

        /// <summary>
        /// State object to pass along to the AsyncCallback delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        sealed class ExecuteNonQueryState<T, Tstate>
        {
            public readonly SqlAsyncCommand query;
            public readonly Func<SqlParameterCollection, int, T> processResult;
            public readonly Action<T, Tstate> useResult;
            public readonly Action<SqlAsyncCommand, Exception> err;
            public readonly Tstate state;

            public ExecuteNonQueryState(SqlAsyncCommand query, Func<SqlParameterCollection, int, T> processResult, Action<T, Tstate> useResult, Action<SqlAsyncCommand, Exception> err, Tstate state)
            {
                this.query = query;
                this.processResult = processResult;
                this.useResult = useResult;
                this.err = err;
                this.state = state;
            }
        }

        /// <summary>
        /// The asynchronous completion handler for ExecuteNonQuery.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ar"></param>
        private static void _completeNonQuery<T, Tstate>(IAsyncResult ar)
        {
            // NOTE(jsd): It is best to reduce the number of memory allocations, so we make use of the state object field available to us:
            var st = (ExecuteNonQueryState<T, Tstate>)ar.AsyncState;

            try
            {
                // Complete the asynchronous request and get the number of rows affected:
                int nr = st.query._cmd.EndExecuteNonQuery(ar);

                T result;
                result = st.processResult(st.query._cmd.Parameters, nr);

                // We no longer need the connection open:
                st.query._cmd.Dispose();
                st.query._db.Close();

                st.useResult(result, st.state);
            }
            catch (Exception ex)
            {
                st.query._cmd.Dispose();
                st.query._db.Close();

                // TODO(jsd): If this throws an exception ... well ... let's just say I hate exceptions in general.
                st.err(st.query, ex);
            }
        }

        /// <summary>
        /// Asynchronously executes the current command. When the asynchronous operation completes, <paramref name="processResult"/>
        /// will be called to generate a custom <typeparamref name="T"/>-typed result which will then be handed to <paramref name="useResult"/>
        /// after the connection is closed.
        /// </summary>
        /// <typeparam name="T">Custom type used to represent query results with.</typeparam>
        /// <param name="processResult">Function called to generate a custom <typeparamref name="T"/>-typed result which will then be handed to <paramref name="useResult"/>.</param>
        /// <param name="useResult">Action called to perform some work with the custom result.</param>
        public void ExecuteNonQuery<T, Tstate>(Action<SqlAsyncCommand, Exception> err, Func<SqlParameterCollection, int, T> processResult, Action<T, Tstate> useResult, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                // Blocking call, unfortunately:
                this._db.Open();

                var ast = new ExecuteNonQueryState<T, Tstate>(this, processResult, useResult, errorHandler, state);

                this._cmd.BeginExecuteNonQuery(_completeNonQuery<T, Tstate>, (object)ast);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                // TODO(jsd): If this throws an exception ... well ... let's just say I hate exceptions in general.
                errorHandler(this, ex);
            }
        }

        #endregion

        #region ExecuteNonQuery

        private static object _processResultDummy(SqlParameterCollection pms, int nr)
        {
            return null;
        }

        /// <summary>
        /// Asynchronously executes the current command. When the asynchronous operation successfully completes, <paramref name="done"/>
        /// will be called after the connection is closed.
        /// </summary>
        /// <param name="done">Action called to perform some work after the command successfully completes.</param>
        public void ExecuteNonQuery<Tstate>(Action<SqlAsyncCommand, Exception> err, Action<Tstate> done, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                // Blocking call, unfortunately:
                this._db.Open();

                var st = new ExecuteNonQueryState<object, Tstate>(this, _processResultDummy, (dumb_, state_) => done(state_), errorHandler, state);

                this._cmd.BeginExecuteNonQuery(_completeNonQuery<object, Tstate>, (object)st);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                // TODO(jsd): If this throws an exception ... well ... let's just say I hate exceptions in general.
                errorHandler(this, ex);
            }
        }

        #endregion

        #endregion

        #region Synchronous execution

        #region ExecuteReaderSync

        public void ExecuteReaderSync<T, Tstate>(Action<SqlAsyncCommand, Exception> err, Func<SqlParameterCollection, IEnumerable<AutoDataRecord>, T> readResult, Action<T, Tstate> useResult, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                this._db.Open();

                T result;
                using (var dr = this._cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess))
                    result = readResult(this._cmd.Parameters, _enumerate(dr));

                this._cmd.Dispose();
                this._db.Close();

                useResult(result, state);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                errorHandler(this, ex);
            }
        }

        #endregion

        #region ExecuteNonQuerySync<T>

        public void ExecuteNonQuerySync<T, Tstate>(Action<SqlAsyncCommand, Exception> err, Func<SqlParameterCollection, int, T> processResult, Action<T, Tstate> useResult, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                this._db.Open();

                int nr = this._cmd.ExecuteNonQuery();
                T result;
                result = processResult(this._cmd.Parameters, nr);

                this._cmd.Dispose();
                this._db.Close();

                useResult(result, state);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                errorHandler(this, ex);
            }
        }

        #endregion

        #region ExecuteNonQuerySync

        public void ExecuteNonQuerySync<Tstate>(Action<SqlAsyncCommand, Exception> err, Action<Tstate> done, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                this._db.Open();

                this._cmd.ExecuteNonQuery();

                this._cmd.Dispose();
                this._db.Close();

                done(state);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                errorHandler(this, ex);
            }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Represents a SQL command that can be easily executed asynchronously.
    /// </summary>
    public sealed class SqlSyncCommand
    {
        #region Private fields

        private readonly SqlConnection _db;
        private readonly SqlCommand _cmd;

        /// <summary>
        /// Default exception handler. Override with SetExceptionHandler.
        /// </summary>
        private static readonly Action<SqlSyncCommand, Exception> _errDefault = (cmd, ex) =>
        {
            Trace.WriteLine(ex.ToString(), "SqlSyncCommand");
        };

        #endregion

        #region Constructors

        private SqlSyncCommand(CommandType commandType, string commandText, string connectionString, int? commandTimeout)
        {
            this._db = new SqlConnection(connectionString);

            // Create a command to execute the query with:
            this._cmd = _db.CreateCommand();
            this._cmd.CommandTimeout = commandTimeout ?? global::Common.Configuration.EnvironmentSettings.Get("DatabaseTimeout.Command.Seconds", Int32.Parse, 60);
            this._cmd.CommandText = commandText;
            this._cmd.CommandType = commandType;
        }

        #endregion

        #region Static creator methods

        /// <summary>
        /// Creates an asynchronous command to execute an inline SQL query.
        /// </summary>
        /// <param name="queryText">The inline SQL query text to execute</param>
        /// <param name="connectionString">The connection string</param>
        /// <returns></returns>
        public static SqlSyncCommand Query(string queryText, string connectionString)
        {
            return Query(queryText, connectionString, null);
        }

        /// <summary>
        /// Creates an asynchronous command to execute an inline SQL query with an optional execution timeout.
        /// </summary>
        /// <param name="queryText">The inline SQL query text to execute</param>
        /// <param name="connectionString">The connection string</param>
        /// <param name="commandTimeout">An optional command execution timeout value (in seconds). Default is 60.</param>
        /// <returns></returns>
        public static SqlSyncCommand Query(string queryText, string connectionString, [Optional] int? commandTimeout)
        {
            return new SqlSyncCommand(CommandType.Text, queryText, connectionString, commandTimeout);
        }

        /// <summary>
        /// Creates an asynchronous command to execute a SQL stored procedure.
        /// </summary>
        /// <param name="procedureName">The SQL stored procedure name to execute</param>
        /// <param name="connectionString">The connection string</param>
        /// <returns></returns>
        public static SqlSyncCommand StoredProcedure(string procedureName, string connectionString)
        {
            return StoredProcedure(procedureName, connectionString, null);
        }

        /// <summary>
        /// Creates an asynchronous command to execute a SQL stored procedure with an optional execution timeout.
        /// </summary>
        /// <param name="procedureName">The SQL stored procedure name to execute</param>
        /// <param name="connectionString">The connection string</param>
        /// <param name="commandTimeout">An optional command execution timeout value (in seconds). Default is 60.</param>
        /// <returns></returns>
        public static SqlSyncCommand StoredProcedure(string procedureName, string connectionString, [Optional] int? commandTimeout)
        {
            return new SqlSyncCommand(CommandType.StoredProcedure, procedureName, connectionString, commandTimeout);
        }

        #endregion

        #region Query parameters

        /// <summary>
        /// Adds a command parameter with a name and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithValue(string paramName, object value)
        {
            var prm = this._cmd.Parameters.AddWithValue(paramName, value);
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name and type.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithType(string paramName, SqlDbType type)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, and direction.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="direction">Parameter direction</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeDirection(string paramName, SqlDbType type, ParameterDirection direction)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            prm.Direction = direction;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeValue(string paramName, SqlDbType type, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            prm.Value = value;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, direction, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeDirectionValue(string paramName, SqlDbType type, ParameterDirection direction, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type);
            prm.Direction = direction;
            prm.Value = value;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, and size.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSize(string paramName, SqlDbType type, int size)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, size, and direction.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="direction">Parameter direction</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSizeDirection(string paramName, SqlDbType type, int size, ParameterDirection direction)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            prm.Direction = direction;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, size, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSizeValue(string paramName, SqlDbType type, int size, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            prm.Value = value;
            return prm;
        }

        /// <summary>
        /// Adds a command parameter with a name, type, size, direction, and value.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="type">Parameter type</param>
        /// <param name="size">Parameter size</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="value">Parameter value</param>
        /// <returns></returns>
        public SqlParameter AddParameterWithTypeSizeDirectionValue(string paramName, SqlDbType type, int size, ParameterDirection direction, object value)
        {
            var prm = this._cmd.Parameters.Add(paramName, type, size);
            prm.Direction = direction;
            prm.Value = value;
            return prm;
        }

        #endregion

        #region Private

        /// <summary>
        /// Enumerates a SqlDataReader, wrapping each row in an <see cref="AutoDataRecord"/> struct.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static IEnumerable<AutoDataRecord> _enumerate(SqlDataReader dr)
        {
            try
            {
                while (dr.Read()) yield return new AutoDataRecord(dr);
            }
            finally
            {
                dr.Close();
            }
        }

        #endregion

        #region Command properties

        /// <summary>
        /// Gets the command parameter collection.
        /// </summary>
        public SqlParameterCollection Parameters { get { return _cmd.Parameters; } }

        /// <summary>
        /// Gets the command text.
        /// </summary>
        public string CommandText { get { return _cmd.CommandText; } }

        /// <summary>
        /// Gets the command type.
        /// </summary>
        public CommandType CommandType { get { return _cmd.CommandType; } }

        #endregion

        #region Synchronous execution

        #region ExecuteReader

        public void ExecuteReader<T, Tstate>(Action<SqlSyncCommand, Exception> err, Func<SqlParameterCollection, IEnumerable<AutoDataRecord>, T> readResult, Action<T, Tstate> useResult, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                this._db.Open();

                T result;
                using (var dr = this._cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess))
                    result = readResult(this._cmd.Parameters, _enumerate(dr));

                this._cmd.Dispose();
                this._db.Close();

                useResult(result, state);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                errorHandler(this, ex);
            }
        }

        public T ExecuteReaderWithMeta<TMeta, T>(Func<SqlParameterCollection, AutoMetadataRecord, TMeta> getMeta, Func<TMeta, IEnumerable<AutoDataRecord>, T> readResult)
        {
            try
            {
                this._db.Open();

                T result;
                using (var dr = this._cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess))
                {
                    var meta = getMeta(this._cmd.Parameters, new AutoMetadataRecord(dr));
                    result = readResult(meta, _enumerate(dr));
                }

                this._cmd.Dispose();
                this._db.Close();

                return result;
            }
            catch (Exception)
            {
                this._cmd.Dispose();
                this._db.Close();

                throw;
            }
        }

        public T ExecuteReader<T>(Func<SqlParameterCollection, IEnumerable<AutoDataRecord>, T> readResult)
        {
            try
            {
                this._db.Open();

                T result;
                using (var dr = this._cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess))
                    result = readResult(this._cmd.Parameters, _enumerate(dr));

                this._cmd.Dispose();
                this._db.Close();

                return result;
            }
            catch (Exception)
            {
                this._cmd.Dispose();
                this._db.Close();

                throw;
            }
        }

        #endregion

        #region ExecuteNonQuery<T>

        public void ExecuteNonQuery<T, Tstate>(Action<SqlSyncCommand, Exception> err, Func<SqlParameterCollection, int, T> processResult, Action<T, Tstate> useResult, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                this._db.Open();

                int nr = this._cmd.ExecuteNonQuery();
                T result;
                result = processResult(this._cmd.Parameters, nr);

                this._cmd.Dispose();
                this._db.Close();

                useResult(result, state);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                errorHandler(this, ex);
            }
        }

        #endregion

        #region ExecuteNonQuery

        public void ExecuteNonQuery<Tstate>(Action<SqlSyncCommand, Exception> err, Action<Tstate> done, Tstate state)
        {
            var errorHandler = err ?? _errDefault;
            try
            {
                this._db.Open();

                this._cmd.ExecuteNonQuery();

                this._cmd.Dispose();
                this._db.Close();

                done(state);
            }
            catch (Exception ex)
            {
                this._cmd.Dispose();
                this._db.Close();

                errorHandler(this, ex);
            }
        }

        #endregion

        #endregion
    }
}

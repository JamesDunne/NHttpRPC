using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC
{
    /// <summary>
    /// A RPC method executor
    /// </summary>
    public interface IExecutor
    {
        RPCAttribute RPCAttr { get; }
        Type InterfaceType { get; }
        MethodInfo Method { get; }
        HttpMethodAttribute HttpMethodAttr { get; }

        IDictionary<string, RPCMethodParameter> Parameters { get; }

        /// <summary>
        /// Execute the method as normal and return its results.
        /// </summary>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        Task<object> Execute(IDictionary<string, object> parameterValues);

        /// <summary>
        /// Execute a meta request about the method and return the metadata.
        /// </summary>
        /// <param name="metaCommand">The implementation-specific command to execute, supplied as the "$meta" query string parameter</param>
        /// <param name="metaParameterValues">Meta command parameters</param>
        /// <returns></returns>
        Task<object> ExecuteMeta(string metaCommand, IDictionary<string, object> metaParameterValues);
    }
}

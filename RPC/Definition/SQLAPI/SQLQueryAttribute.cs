using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SQLQueryAttribute : SQLCommandAttribute
    {
        public SQLQueryAttribute(string query, string connectionName = null, Type queryTextProcessorType = null)
            : base("GET", query, connectionName, queryTextProcessorType)
        {
        }
    }
}

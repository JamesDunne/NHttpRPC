using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SQLUpdateAttribute : SQLCommandAttribute
    {
        public SQLUpdateAttribute(string commandText, string connectionName = null, Type queryTextProcessorType = null)
            : base("POST", commandText, connectionName, queryTextProcessorType)
        {
        }
    }
}

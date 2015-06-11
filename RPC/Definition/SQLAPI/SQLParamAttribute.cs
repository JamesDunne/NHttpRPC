using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class SQLParamAttribute : Attribute
    {
        public readonly SQLParamDescriptor Descriptor;

        public SQLParamAttribute(SqlDbType type, string name = null, bool isNullable = false, int size = 0, int scale = 0, int precision = 0)
        {
            this.Descriptor = new SQLParamDescriptor(type, name, isNullable, size, scale, precision);
        }
    }
}

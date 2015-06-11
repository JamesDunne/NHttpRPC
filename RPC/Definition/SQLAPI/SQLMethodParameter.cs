using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    public sealed class SQLMethodParameter : RPCMethodParameter
    {
        public readonly ParameterInfo ParameterInfo;
        public readonly SQLParamDescriptor Descriptor;

        public SQLMethodParameter(ParameterInfo parameterInfo, SQLParamDescriptor descriptor)
            : base(parameterInfo.Name, CLRType(descriptor), parameterInfo.IsOptional)
        {
            ParameterInfo = parameterInfo;
            Descriptor = descriptor;
        }

        public override string TypeName()
        {
            return this.Descriptor.TypeName();
        }

        static Type CLRType(SQLAPI.SQLParamDescriptor descriptor)
        {
            var baseType = CLRTypeForDbType(descriptor.Type);

            // Wrap nullable value types in Nullable<T>:
            if (descriptor.IsNullable && baseType.IsValueType)
                return typeof(Nullable<>).MakeGenericType(baseType);

            return baseType;
        }

        static Type CLRTypeForDbType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.Int: return typeof(int);
                case SqlDbType.BigInt: return typeof(long);
                case SqlDbType.SmallInt: return typeof(short);
                case SqlDbType.TinyInt: return typeof(byte);
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar: return typeof(string);
                case SqlDbType.DateTime2:
                case SqlDbType.DateTime: return typeof(DateTime);

                case SqlDbType.Bit: return typeof(bool);
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                case SqlDbType.Decimal: return typeof(decimal);
                case SqlDbType.Float: return typeof(double);
                case SqlDbType.Real: return typeof(float);

                case SqlDbType.UniqueIdentifier: return typeof(Guid);
                case SqlDbType.Binary:
                case SqlDbType.VarBinary: return typeof(byte[]);

                case SqlDbType.Timestamp: return typeof(byte[]);
                // TODO(jsd): I dunno.
                default: return typeof(string);
            }
        }
    }
}

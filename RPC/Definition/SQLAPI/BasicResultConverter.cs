using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    public sealed class BasicResultConverter<T> : SQLResultConverter<T>
    {
        public static bool CanConvert(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return CanConvert(type.GetGenericArguments()[0]);

            if (type == typeof(string))
                return true;
            if (type == typeof(int))
                return true;
            if (type == typeof(bool))
                return true;
            if (type == typeof(DateTime))
                return true;
            if (type == typeof(DateTimeOffset))
                return true;
            if (type == typeof(Guid))
                return true;
            if (type == typeof(short))
                return true;
            if (type == typeof(long))
                return true;
            if (type == typeof(byte))
                return true;
            // TODO: more types?
            return false;
        }

        public T Convert(RowReader rr)
        {
            var type = typeof(T);

            bool isNullableType = (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
            if (type.IsClass || isNullableType)
            {
                // Check for null:
                if (rr._dr.IsDBNull(0))
                    return (T)(object)null;

                // Handle the inner non-nullable type:
                if (isNullableType)
                    type = type.GetGenericArguments()[0];
            }

            if (type == typeof(string))
                return (T)(object)rr._dr.GetString(0);
            if (type == typeof(int))
                return (T)(object)rr._dr.GetInt32(0);
            if (type == typeof(bool))
                return (T)(object)rr._dr.GetBoolean(0);
            if (type == typeof(DateTime))
                return (T)(object)rr._dr.GetDateTime(0);
            if (type == typeof(DateTimeOffset))
                return (T)(object)rr._dr.GetDateTimeOffset(0);
            if (type == typeof(Guid))
                return (T)(object)rr._dr.GetGuid(0);
            if (type == typeof(short))
                return (T)(object)rr._dr.GetInt16(0);
            if (type == typeof(long))
                return (T)(object)rr._dr.GetInt64(0);
            if (type == typeof(byte))
                return (T)(object)rr._dr.GetByte(0);

            throw new Exception("Unsupported CLR type for SQL result: {0}".F(type.FullName));
        }
    }
}

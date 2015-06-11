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
    public sealed class SQLParamDescriptor
    {
        public readonly SqlDbType Type;
        public readonly string Name;
        public readonly bool IsNullable;
        public readonly int? Size;
        public readonly int? Scale;
        public readonly int? Precision;

        public SQLParamDescriptor(SqlDbType type, string name = null, bool isNullable = false, int size = 0, int scale = 0, int precision = 0)
        {
            this.Type = type;
            this.Name = name;
            this.IsNullable = isNullable;
            this.Size = size == 0 ? (int?)null : size;
            this.Scale = scale == 0 ? (int?)null : scale;
            this.Precision = precision == 0 ? (int?)null : precision;
        }

        /// <summary>
        /// Mutates the passed-in <paramref name="param"/> using only the non-null values from this attribute.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public SqlParameter ApplyTo(SqlParameter param)
        {
            if (param.SqlDbType != Type) param.SqlDbType = Type;
            param.IsNullable = IsNullable;

            if (Name != null) param.ParameterName = Name;
            if (Size.HasValue) param.Size = Size.Value;
            if (Scale.HasValue) param.Scale = (byte)Scale.Value;
            if (Precision.HasValue) param.Precision = (byte)Precision.Value;

            return param;
        }

        /// <summary>
        /// Gets a T-SQL formatted name for the type constraint.
        /// </summary>
        /// <returns></returns>
        public string TypeName()
        {
            switch (Type)
            {
                case SqlDbType.Int: return "int";
                case SqlDbType.TinyInt: return "tinyint";
                case SqlDbType.SmallInt: return "smallint";
                case SqlDbType.BigInt: return "bigint";
                case SqlDbType.Bit: return "bit";
                case SqlDbType.Float: return "float";
                case SqlDbType.Real: return "real";
                case SqlDbType.DateTime: return "datetime";
                case SqlDbType.VarChar: return "varchar{0}".F(Size.NullFormat(v => v < 0 ? "(max)" : "({0})".F(v)));
                case SqlDbType.NVarChar: return "nvarchar{0}".F(Size.NullFormat(v => v < 0 ? "(max)" : "({0})".F(v)));
                case SqlDbType.Char: return "char{0}".F(Size.NullFormat(v => v < 0 ? "(max)" : "({0})".F(v)));
                case SqlDbType.NChar: return "nchar{0}".F(Size.NullFormat(v => v < 0 ? "(max)" : "({0})".F(v)));
                case SqlDbType.VarBinary: return "varbinary{0}".F(Size.NullFormat(v => v < 0 ? "(max)" : "({0})".F(v)));
                case SqlDbType.Binary: return "binary{0}".F(Size.NullFormat(v => v < 0 ? "(max)" : "({0})".F(v)));
                // TODO(jsd): More formatting support!
                default: return Type.ToString().ToLowerInvariant();
            }
        }
    }
}

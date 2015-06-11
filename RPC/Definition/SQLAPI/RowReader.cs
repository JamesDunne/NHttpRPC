using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    public sealed class RowReader
    {
        internal readonly SqlDataReader _dr;
        readonly Dictionary<string, int> _ordinals;

        internal RowReader(SqlDataReader dr)
        {
            _dr = dr;

            // Create a dictionary of names to ordinals:
            // TODO(jsd): Not sure about OrdinalIgnoreCase here wrt uniqueness of column names.
            _ordinals = new Dictionary<string, int>(_dr.FieldCount, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _dr.FieldCount; ++i)
                _ordinals[_dr.GetName(i)] = i;
        }

        public int Ordinal(string name)
        {
            int ord;
            if (!_ordinals.TryGetValue(name, out ord)) throw new InvalidOperationException("Column '{0}' does not exist".F(name));
            return ord;
        }

        public bool IsNull(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord); }
        public bool IsNullOrNonExistent(string name)
        {
            int ord;
            if (!_ordinals.TryGetValue(name, out ord)) return true;
            return _dr.IsDBNull(ord);
        }

        #region Non-nullable data readers

        public string String(string name) { int ord = Ordinal(name); return _dr.GetString(ord); }
        public Int32 Int32(string name) { int ord = Ordinal(name); return _dr.GetInt32(ord); }
        public bool Bool(string name) { int ord = Ordinal(name); return _dr.GetBoolean(ord); }
        public DateTime DateTime(string name) { int ord = Ordinal(name); return _dr.GetDateTime(ord); }
        public DateTimeOffset DateTimeOffset(string name) { int ord = Ordinal(name); return _dr.GetDateTimeOffset(ord); }
        public decimal Decimal(string name) { int ord = Ordinal(name); return _dr.GetDecimal(ord); }
        // TODO: add more.

        #endregion

        #region Nullable data readers

        public string NullString(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord) ? (string)null : _dr.GetString(ord); }
        public Int32? NullInt32(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord) ? (Int32?)null : _dr.GetInt32(ord); }
        public bool? NullBool(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord) ? (bool?)null : _dr.GetBoolean(ord); }
        public DateTime? NullDateTime(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord) ? (DateTime?)null : _dr.GetDateTime(ord); }
        public DateTimeOffset? NullDateTimeOffset(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord) ? (DateTimeOffset?)null : _dr.GetDateTimeOffset(ord); }
        public decimal? NullDecimal(string name) { int ord = Ordinal(name); return _dr.IsDBNull(ord) ? (decimal?)null : _dr.GetDecimal(ord); }
        // TODO: add more.

        #endregion
    }
}

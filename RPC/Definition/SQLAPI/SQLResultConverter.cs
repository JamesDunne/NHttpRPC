using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    /// <summary>
    /// An interface to represent converters from SQL results represented by a <see cref="RowReader"/> to concrete model types.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface SQLResultConverter<out TResult>
    {
        /// <summary>
        /// Converts column data from a SQL resultset into a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <param name="rr"></param>
        /// <returns></returns>
        TResult Convert(RowReader rr);
    }
}

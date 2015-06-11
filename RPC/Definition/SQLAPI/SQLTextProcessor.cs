using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPC.SQLAPI
{
    public interface SQLTextProcessor
    {
        /// <summary>
        /// Processes the incoming possibly-marked-up <paramref name="sql"/> text and produces valid SQL.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        string ProcessText(string sql);
    }
}

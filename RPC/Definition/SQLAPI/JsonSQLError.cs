using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    public class JsonSQLError : Common.Web.JsonError
    {
        public int number;
        public int line;
        public string procedure;
        public string server;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.JsonRPC
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HttpPostAttribute : HttpMethodAttribute
    {
        public HttpPostAttribute() : base("POST")
        {
        }
    }
}

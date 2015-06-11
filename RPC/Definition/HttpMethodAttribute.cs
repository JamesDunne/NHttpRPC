using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC
{
    /// <summary>
    /// The base of attribute types applied to RPC methods which describes which HTTP method to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class HttpMethodAttribute : Attribute
    {
        public readonly string HttpMethod;

        protected HttpMethodAttribute(string httpMethod)
        {
            HttpMethod = httpMethod.ToUpperInvariant();
        }

        public bool IsGET()
        {
            return HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsPOST()
        {
            return HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }
    }
}

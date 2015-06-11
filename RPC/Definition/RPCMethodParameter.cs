using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC
{
    /// <summary>
    /// A descriptor for an RPC method's parameter.
    /// </summary>
    public class RPCMethodParameter
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly bool IsOptional;

        public RPCMethodParameter(string name, Type type, bool isOptional = false)
        {
            Name = name;
            Type = type;
            IsOptional = isOptional;
        }

        public virtual string TypeName()
        {
            return this.Type.FullName;
        }
    }
}

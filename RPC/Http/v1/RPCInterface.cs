using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.v1
{
    public struct RPCInterface
    {
        public readonly Type ty;
        public readonly RPCAttribute api;

        public RPCInterface(Type type, RPCAttribute attr)
        {
            ty = type;
            api = attr;
        }
    }
}

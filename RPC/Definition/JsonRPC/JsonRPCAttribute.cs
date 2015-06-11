using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC.JsonRPC
{
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class JsonRPCAttribute : RPCAttribute
    {
        public JsonRPCAttribute(string category, string version, string name)
            : base(category, version, name)
        {
        }

        public override IExecutor GetExecutor(Type interfaceType, string methodName, Func<Type> getImplementationType = null)
        {
            Type implType = null;
            if (getImplementationType != null)
                implType = getImplementationType();

            return (IExecutor)Executor.ExecutorFor(interfaceType, implType, methodName);
        }
    }
}

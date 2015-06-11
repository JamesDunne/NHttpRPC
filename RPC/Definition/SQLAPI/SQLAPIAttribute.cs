using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    /// <summary>
    /// Applies to an interface to signify that its methods are implemented as SQLAPI methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class SQLAPIAttribute : RPCAttribute
    {
        public readonly string ConnectionName;
        public readonly Type TextProcessorType;

        public SQLAPIAttribute(string category, string version, string name, string connectionName, Type textProcessorType = null)
            : base(category, version, name)
        {
            if (category == null) throw new ArgumentNullException("category");
            if (name == null) throw new ArgumentNullException("name");
            if (connectionName == null) throw new ArgumentNullException("connectionName");

            if (textProcessorType != null)
            {
                var expectedTextProcessorType = typeof(SQLTextProcessor);
                if (textProcessorType.GetInterfaces().Any(i => i.IsGenericType && i == expectedTextProcessorType))
                    throw new ArgumentException("textProcessorType '{0}' must implement the `{1}` interface".F(textProcessorType.FullName, expectedTextProcessorType.GetCSharpDisplayName()), "textProcessorType");

                this.TextProcessorType = textProcessorType;
            }

            this.ConnectionName = connectionName;
        }

        public override IExecutor GetExecutor(Type interfaceType, string methodName, Func<Type> getImplementationType = null)
        {
            return (IExecutor)Executor.ExecutorFor(interfaceType, methodName);
        }
    }
}

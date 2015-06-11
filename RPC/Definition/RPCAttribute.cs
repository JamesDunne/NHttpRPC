using System;

namespace RPC
{
    /// <summary>
    /// An attribute derived from this applies to interfaces which define RPC methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public abstract class RPCAttribute : Attribute
    {
        public readonly string Category;
        public readonly string Version;
        public readonly string Name;

        public RPCAttribute(string category, string version, string name)
        {
            if (category == null) throw new ArgumentNullException("category");
            if (version == null) throw new ArgumentNullException("version");
            if (name == null) throw new ArgumentNullException("name");

            this.Category = category;
            this.Version = version;
            this.Name = name;
        }

        /// <summary>
        /// Gets an executor that can execute methods of this kind of RPC.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="methodName"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public abstract IExecutor GetExecutor(Type interfaceType, string methodName, Func<Type> getImplementationType = null);
    }
}

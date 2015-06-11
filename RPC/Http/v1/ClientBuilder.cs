using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RPC.v1
{
    public class ClientBuilder
    {
        private System.Reflection.Emit.AssemblyBuilder ab;
        private Dictionary<Type, Type> clientImplementations;

        private ClientBuilder(System.Reflection.Emit.AssemblyBuilder ab)
        {
            this.ab = ab;
            this.clientImplementations = new Dictionary<Type, Type>();
        }

        public static ClientBuilder Start(string assemblyName)
        {
            // Create a dynamic assembly to compile client classes into:
            return new ClientBuilder(AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run));
        }

        private Type register<TClient>()
        {
            Type impl;
            if (this.clientImplementations.TryGetValue(typeof(TClient), out impl))
                return impl;

            // Compile a concrete class implementation into the assembly builder:
            impl = HttpClientBase.CompileClientFor<TClient>(this.ab);

            this.clientImplementations.Add(typeof(TClient), impl);

            return impl;
        }

        public void Register<TClient>()
        {
            register<TClient>();
        }

        public TClient GetClient<TClient>(Uri uriBase)
        {
            Type impl;
            if (!this.clientImplementations.TryGetValue(typeof(TClient), out impl))
                impl = register<TClient>();

            return (TClient)Activator.CreateInstance(impl, uriBase);
        }
    }
}

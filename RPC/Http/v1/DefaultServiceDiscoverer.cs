using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC.v1
{
    public sealed class DefaultServiceDiscoverer : IServiceDiscoverer
    {
        readonly RPCInterface[] rpcInterfaces;
        readonly Type[] rpcImplementations;

        public DefaultServiceDiscoverer(Assembly interfaceAssembly, Assembly implementationAssembly)
        {
            this.rpcInterfaces = (
                from ty in interfaceAssembly.GetExportedTypes()
                where ty.IsInterface
                let api = ty.GetCustomAttribute<RPCAttribute>()
                where api != null
                select new RPCInterface(ty, api)
            ).ToArray();

            this.rpcImplementations = implementationAssembly.GetExportedTypes();
        }

        public RPCInterface[] DiscoverInterfaces()
        {
            return rpcInterfaces;
        }

        public Type DiscoverImplementationFor(Type type)
        {
            // TODO(jsd): Find a better way to find an implementation class...
            var impls = (
                from ty in rpcImplementations
                where ty.IsClass && !ty.IsAbstract
                where type.IsAssignableFrom(ty)
                select ty
            ).Take(2).ToList(2);

            // We only need one implementing class:
            if (!impls.Any())
                throw new InvalidRequestException("No implementation class found for interface '{0}'".F(type.FullName));

            // TODO(jsd): Consider requiring an attribute to signify the main RPC implementation if multiple implementing classes are found.
            if (impls.Count > 1)
                throw new InvalidRequestException("Multiple implementation classes found for interface '{0}'".F(type.FullName));

            return impls.Single();
        }
    }
}

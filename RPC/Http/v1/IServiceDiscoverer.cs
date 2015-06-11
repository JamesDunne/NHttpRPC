using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPC.v1
{
    public interface IServiceDiscoverer
    {
        /// <summary>
        /// Returns an array of all known interface types that have an RPCAttribute defined on them.
        /// </summary>
        /// <returns></returns>
        RPCInterface[] DiscoverInterfaces();

        /// <summary>
        /// Given a specific interface type, find its concrete implementation class to be invoked service-side.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Type DiscoverImplementationFor(Type type);
    }
}

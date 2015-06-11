using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC
{
    public static class MethodFinder
    {
        public static IDictionary<string, MethodInfo> GetMethods(Type intf)
        {
            var dict = new Dictionary<string, MethodInfo>();

            var ifstk = new Stack<Type>();
            ifstk.Push(intf);
            while (ifstk.Count > 0)
            {
                var i = ifstk.Pop();
                foreach (var s in i.GetInterfaces())
                    ifstk.Push(s);

                foreach (var m in i.GetMethods())
                {
                    // Conflict?
                    if (dict.ContainsKey(m.Name))
                    {
                        // Gross; wish there was a cleaner way to find overloads:
                        try
                        {
                            i.GetMethod(m.Name);

                            // Skip adding this method because it's been overridden in a higher level interface:
                            continue;
                        }
                        catch (AmbiguousMatchException)
                        {
                            throw new Exception("Overloaded methods are not allowed in RPC interfaces");
                        }
                    }

                    dict.Add(m.Name, m);
                }
            }

            return dict;
        }

        public static MethodInfo GetMethod(Type intf, string name)
        {
            return GetMethods(intf)[name];
        }
    }
}

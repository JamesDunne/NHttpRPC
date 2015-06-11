using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    public static class Executor
    {
        static readonly ConcurrentDictionary<Type, ValidSQLAPIInterface> _interfaces = new ConcurrentDictionary<Type, ValidSQLAPIInterface>();

        public static SQLCommandExecutor<TResult> ExecutorFor<TResult>(Type interfaceType, string methodName)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (methodName == null) throw new ArgumentNullException("methodName");

            var validMethod = ValidateMethod(interfaceType, methodName);

            return new SQLCommandExecutor<TResult>(validMethod);
        }

        public static SQLCommandExecutor ExecutorFor(Type interfaceType, string methodName)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (methodName == null) throw new ArgumentNullException("methodName");

            var validMethod = ValidateMethod(interfaceType, methodName);

            return new SQLCommandExecutor(validMethod);
        }

        static ValidSQLAPIMethod ValidateMethod(Type interfaceType, string methodName)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (methodName == null) throw new ArgumentNullException("methodName");

            var validInterface = _interfaces.GetOrAdd(interfaceType, (ty) =>
            {
                if (!ty.IsInterface)
                    throw new ArgumentException("Type '{0}' is not an interface".F(ty.FullName), "method");

                // The interface requires a SQLAPIAttribute:
                var apiAttr = ty.GetCustomAttribute<SQLAPIAttribute>();
                if (apiAttr == null)
                    throw new ArgumentException("Interface '{0}' has no SQLAPIAttribute declared".F(ty.FullName), "method");

                return new ValidSQLAPIInterface(ty, apiAttr);
            });

            var validMethod = validInterface.Methods.GetOrAdd(methodName, (mname) =>
            {
                // Get the method from the interface:
                var method = MethodFinder.GetMethod(interfaceType, methodName);
                if (method == null)
                    throw new ArgumentException("Method named '{0}' does not exist on interface '{1}'".F(methodName, interfaceType.FullName), "methodName");

                // We require the custom attribute be applied to the method:
                var cmdAttr = method.GetCustomAttribute<SQLCommandAttribute>();
                if (cmdAttr == null)
                    throw new ArgumentException("Method '{0}' on interface '{1}' has no {2} declared".F(method.Name, interfaceType.FullName, typeof(SQLCommandAttribute).Name), "method");

                return new ValidSQLAPIMethod(validInterface, method, cmdAttr);
            });

            return validMethod;
        }
    }
}

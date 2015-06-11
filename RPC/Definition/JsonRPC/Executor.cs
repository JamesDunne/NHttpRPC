using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPC.JsonRPC
{
    public sealed class Executor : IExecutor
    {
        public RPCAttribute RPCAttr { get; private set; }
        public Type InterfaceType { get; private set; }
        public MethodInfo Method { get; private set; }
        public HttpMethodAttribute HttpMethodAttr { get; private set; }

        public IDictionary<string, RPCMethodParameter> Parameters { get { return rpcParameters; } }

        public readonly Type ImplementationType;
        public readonly MethodInfo ImplementationMethod;

        Dictionary<string, RPCMethodParameter> rpcParameters;
        readonly int parameterCount;
        readonly int parameterMinRequiredCount;
        readonly Dictionary<string, int> parameterOrdinalMapping;

        public readonly object Instance;

        internal Executor(Type interfaceType, Type implementationType, MethodInfo method, RPCAttribute apiAttr, HttpMethodAttribute httpMethodAttr)
        {
            Debug.Assert(interfaceType != null);
            Debug.Assert(implementationType != null);
            Debug.Assert(method != null);
            Debug.Assert(apiAttr != null);
            Debug.Assert(httpMethodAttr != null);

            InterfaceType = interfaceType;
            ImplementationType = implementationType;
            Method = method;
            RPCAttr = apiAttr;
            HttpMethodAttr = httpMethodAttr;

            ImplementationMethod = ImplementationType.GetMethod(Method.Name);

            // Map parameter names to ordinals:
            var parameters = Method.GetParameters();
            parameterCount = parameters.Length;
            parameterMinRequiredCount = 0;
            parameterOrdinalMapping = new Dictionary<string, int>(parameterCount);
            rpcParameters = new Dictionary<string, RPCMethodParameter>(parameterCount);
            for (int i = 0; i < parameterCount; ++i)
            {
                var prm = parameters[i];
                if (!prm.IsOptional) ++parameterMinRequiredCount;
                rpcParameters.Add(prm.Name, new RPCMethodParameter(prm.Name, prm.ParameterType, prm.IsOptional));
                parameterOrdinalMapping.Add(prm.Name, i);
            }

            Instance = Activator.CreateInstance(implementationType);
        }

        public static Executor ExecutorFor(Type interfaceType, Type implementationType, string methodName)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (implementationType == null) throw new ArgumentNullException("implementationType");
            if (methodName == null) throw new ArgumentNullException("methodName");

            MethodInfo method;
            RPCAttribute apiAttr;
            HttpMethodAttribute cmdAttr;
            ValidateMethod(interfaceType, implementationType, methodName, out method, out apiAttr, out cmdAttr);

            return new Executor(interfaceType, implementationType, method, apiAttr, cmdAttr);
        }

        internal static void ValidateMethod<TCustomAttribute>(Type interfaceType, Type implementationType, string methodName, out MethodInfo method, out RPCAttribute apiAttr, out TCustomAttribute queryAttr)
            where TCustomAttribute : Attribute
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (implementationType == null) throw new ArgumentNullException("implementationType");
            if (methodName == null) throw new ArgumentNullException("methodName");

            // Validate the bare minimum requirements first:
            if (!interfaceType.IsInterface)
                throw new ArgumentException("Interface type '{0}' is not an interface".F(interfaceType.FullName), "interfaceType");

            // Get the method from the interface:
            try
            {
                method = MethodFinder.GetMethod(interfaceType, methodName);
                if (method == null)
                    throw new ArgumentException("Method named '{0}' does not exist on interface '{1}'".F(methodName, interfaceType.FullName), "methodName");
            }
            catch (AmbiguousMatchException)
            {
                throw new ArgumentException("Method '{0}' on interface '{1}' cannot be overloaded".F(methodName, interfaceType.FullName), "methodName");
            }

            // Validate Task return type:
            var rt = method.ReturnType;
            if (!(rt.IsGenericType && rt.GetGenericTypeDefinition() == typeof(Task<>)) && (rt != typeof(Task)))
                throw new ArgumentException("Method '{0}' on interface '{1}' must return either a Task or Task<T> type".F(methodName, interfaceType.FullName), "methodName");

            // The interface requires a RPCAttribute:
            apiAttr = interfaceType.GetCustomAttribute<RPCAttribute>();
            if (apiAttr == null)
                throw new ArgumentException("Interface '{0}' has no RPCAttribute declared".F(interfaceType.FullName), "method");

            // We require the custom attribute be applied to the method:
            queryAttr = method.GetCustomAttribute<TCustomAttribute>();
            if (queryAttr == null)
                throw new ArgumentException("Method '{0}' on interface '{1}' has no {2} declared".F(method.Name, interfaceType.FullName, typeof(TCustomAttribute).Name), "method");

            // Validate implementation type is a concrete class:
            if (!implementationType.IsClass || implementationType.IsAbstract)
                throw new ArgumentException("Implementation type '{0}' is not a concrete class".F(implementationType.FullName), "implementationType");

            // Validate implementation type implements interface:
            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new ArgumentException("Implementation type '{0}' does not implement interface type '{1}'".F(implementationType.FullName, interfaceType.FullName), "implementationType");
        }

        public async Task<object> Execute(object[] parameterValues)
        {
            // Execute the async method and get the Task to await:
            var task = (Task)ImplementationMethod.Invoke(Instance, parameterValues);

            // Await the task:
            await task;

            // Get the `Result` property from the `Task<T>`:
            var resultProp = task.GetType().GetTypeInfo().GetDeclaredProperty("Result");
            if (resultProp == null)
                return null;

            var result = resultProp.GetValue(task);
            return result;
        }

        public Task<object> Execute(IDictionary<string, object> parameterValues)
        {
            int mappedCount = 0;

            // Bind named parameters to an ordered array of parameters:
            var parameterArray = new object[parameterCount];
            foreach (var pair in parameterValues)
            {
                int ordinal;
                if (!parameterOrdinalMapping.TryGetValue(pair.Key, out ordinal))
                {
                    // No mapping; extra param?
                    continue;
                }

                parameterArray[ordinal] = pair.Value;
                ++mappedCount;
            }

            // Verify that the minimum number of required parameters are mapped:
            if (mappedCount < parameterMinRequiredCount)
            {
                // Discover missing required parameters:
                var missing = new List<string>(parameterMinRequiredCount);
                foreach (var pair in rpcParameters)
                {
                    if (parameterValues.ContainsKey(pair.Key))
                        continue;
                    missing.Add(pair.Key);
                }

                throw new ArgumentException("One or more required parameters are missing: {0}".F(String.Join(", ", missing)));
            }

            return Execute(parameterArray);
        }

        public Task<object> ExecuteMeta(string metaCommand, IDictionary<string, object> metaParameterValues)
        {
            switch (metaCommand.ToLower())
            {
                default: throw new InvalidOperationException("Unrecognized meta command '{0}'".F(metaCommand));
            }
        }
    }
}

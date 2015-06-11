using System.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Common.Web;

namespace RPC.v1
{
    /// <summary>
    /// Base class of all v1 HTTP client implementations.
    /// </summary>
    public abstract class HttpClientBase
    {
        /// <summary>
        /// Creates a dynamic class which acts as the HTTP client proxy for the interface.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static Type CompileClientFor<TInterface>(AssemblyBuilder ab = null)
        {
            var intfType = typeof(TInterface);
            if (!intfType.IsInterface)
                throw new Exception("Type {0} must be an interface".F(intfType.FullName));

            var apiattr = intfType.GetCustomAttribute<RPCAttribute>();
            if (apiattr == null)
                throw new Exception("Type {0} must be an interface that declares a RPCAttribute".F(intfType.FullName));

            var className = intfType.Name.RemoveIfStartsWith("I") + "Client";

            Type type;
            if (ab != null)
            {
                // Already constructed?
                type = ab.GetType(className);
                if (type != null) return type;
            }
            else
            {
                // Create a new dynamic assembly:
                ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("RPC.v1.HttpClient"), AssemblyBuilderAccess.RunAndCollect);
            }

            // Define a new module and define the new client type in it:
            var mdb = ab.DefineDynamicModule("RPC.v1.HttpClient.{0}.dll".F(intfType.Name));
            var tb = mdb.DefineType(
                // type name:
                className,
                // attributes:
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                // parent type:
                typeof(HttpClientBase),
                // interfaces implemented:
                new Type[] { intfType }
            );

            // Define a constructor:
            var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(Uri) });
            ctor.DefineParameter(1, ParameterAttributes.None, "uriBase");
            var ctorIL = ctor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Call, typeof(HttpClientBase).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { typeof(Uri) }, null));
            ctorIL.Emit(OpCodes.Ret);

            // Implement all methods for the interface:
            foreach (var imt in intfType.GetMethods())
            {
                // Define the method:
                var prms = imt.GetParameters();
                var mb = tb.DefineMethod(imt.Name, MethodAttributes.Public | MethodAttributes.Virtual, imt.ReturnType, prms.SelectAsArray(p => p.ParameterType));
                // Define each parameter (1-based):
                for (int i = 0; i < prms.Length; ++i)
                    mb.DefineParameter(i + 1, ParameterAttributes.None, prms[i].Name);

                // Make sure method returns Task<T>:
                // TODO(jsd): Also support non-generic `Task`!
                if (imt.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
                    throw new Exception("Method {0} return type must be `Task<T>`".F(imt.Name));

                // Grab the T from Task<T>:
                var resultType = imt.ReturnType.GetGenericArguments()[0];

                // Determine whether to use httpGet or httpPost base class method:
                MethodInfo baseHttpMethod;
                var cmdattr = imt.GetCustomAttribute<HttpMethodAttribute>();
                if (cmdattr == null)
                {
                    throw new Exception("Method {0} on interface {1} must declare an HttpMethodAttribute".F(imt.Name, intfType.GetCSharpDisplayName()));
                }

                if (cmdattr.IsGET())
                {
                    baseHttpMethod = typeof(HttpClientBase).GetMethod("httpGet", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(resultType);
                }
                else if (cmdattr.IsPOST())
                {
                    baseHttpMethod = typeof(HttpClientBase).GetMethod("httpPost", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(resultType);
                }
                else
                {
                    throw new NotSupportedException("Unsupported HTTP method {0}".F(cmdattr.HttpMethod));
                }

                // Define the method body:
                var il = mb.GetILGenerator();

                // Build dictionary of argument values:
                var lDict = il.DeclareLocal(typeof(Dictionary<string, object>));
                il.Emit(OpCodes.Newobj, lDict.LocalType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc_S, (byte)lDict.LocalIndex);

                for (int i = 0; i < prms.Length; ++i)
                {
                    il.Emit(OpCodes.Ldloc_S, (byte)lDict.LocalIndex);

                    // prmName:
                    il.Emit(OpCodes.Ldstr, prms[i].Name);

                    // prmValue:
                    if (i == 0) il.Emit(OpCodes.Ldarg_1);
                    else if (i == 1) il.Emit(OpCodes.Ldarg_2);
                    else if (i == 2) il.Emit(OpCodes.Ldarg_3);
                    else il.Emit(OpCodes.Ldarg_S, (byte)(i + 1));

                    // Box up the value type:
                    if (prms[i].ParameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, prms[i].ParameterType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Castclass, typeof(object));
                    }

                    // `dict.Add(prmName, prmValue);`
                    il.EmitCall(OpCodes.Callvirt, lDict.LocalType.GetMethod("Add"), null);
                }

                // `this`:
                il.Emit(OpCodes.Ldarg_0);

                // arg 1:
                // Create the method URI path:
                var methodPath = "/v1/{0}/{1}/{2}/{3}".F(apiattr.Category, apiattr.Version, apiattr.Name, imt.Name);
                il.Emit(OpCodes.Ldstr, methodPath);

                // arg 2:
                il.Emit(OpCodes.Ldloc_S, (byte)lDict.LocalIndex);

                // Call base HTTP method:
                il.EmitCall(OpCodes.Call, baseHttpMethod, null);

                // Return the `Task<T>`:
                il.Emit(OpCodes.Ret);

                // Define the method override for the interface:
                tb.DefineMethodOverride(mb, imt);
            }

            type = tb.CreateType();

            return type;
        }

        public static TClient InstantiateClient<TClient>(System.Reflection.Emit.AssemblyBuilder ab, Uri uriBase)
        {
            var type = CompileClientFor<TClient>(ab);
            return (TClient)Activator.CreateInstance(type, new object[1] { uriBase });
        }

        protected readonly Uri uriBase;

        protected HttpClientBase(Uri uriBase)
        {
            this.uriBase = uriBase;
        }

        protected async Task<TResult> httpGet<TResult>(string methodPath, Dictionary<string, object> parameterValues)
        {
            var ub = new UriBuilder(uriBase);
            ub.Path = ub.Path + methodPath;
            addQuery(ub, parameterValues);

            var uri = ub.Uri;

            // Submit the GET request:
            using (var client = new System.Net.Http.HttpClient())
            using (var rsp = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
            {
                // Return the parsed response or throw an exception:
                return await parseResult<TResult>(rsp);
            }
        }

        protected async Task<TResult> httpPost<TResult>(string methodPath, Dictionary<string, object> parameterValues)
        {
            var ub = new UriBuilder(uriBase);
            ub.Path = ub.Path + methodPath;
            var uri = ub.Uri;
            var encodedParameters = encodeParameters(parameterValues);

            // Submit the POST request:
            using (var client = new System.Net.Http.HttpClient())
            using (var rsp = await client.PostAsync(uri, new FormUrlEncodedContent(encodedParameters)))
            {
                // Return the parsed response or throw an exception:
                return await parseResult<TResult>(rsp);
            }
        }

        static async Task<TResult> parseResult<TResult>(HttpResponseMessage rsp)
        {
            // Don't bother parsing non-JSON data:
            if (rsp.Content.Headers.ContentType == null || rsp.Content.Headers.ContentType.MediaType != "application/json")
            {
                rsp.EnsureSuccessStatusCode();
            }

            // Read the reponse content:
            using (var content = await rsp.Content.ReadAsStreamAsync())
            using (var sr = new StreamReader(content))
            using (var jtr = new JsonTextReader(sr))
            {
                var result = Json.Serializer.Deserialize<JsonResponse<TResult, RPCContext>>(jtr);
                // If unsuccessful response code throw an exception:
                if (!rsp.IsSuccessStatusCode || !result.success)
                    throw new RPCException(result.context, result.errors);

                return result.results;
            }
        }

        static string encodeParameter(object value)
        {
            // JSON-encode the parameter:
            var sw = new StringWriter();
            Json.Serializer.Serialize(sw, value);
            var jsonValue = sw.ToString();
            return jsonValue;
        }

        static Dictionary<string, string> encodeParameters(Dictionary<string, object> parameterValues)
        {
            var encoded = new Dictionary<string, string>(parameterValues.Count);
            foreach (var pair in parameterValues)
            {
                // JSON-encode the parameter:
                var jsonValue = encodeParameter(pair.Value);
                encoded.Add(pair.Key, jsonValue);
            }
            return encoded;
        }

        static void addQuery(UriBuilder ub, Dictionary<string, object> parameterValues)
        {
            var sbQuery = new StringBuilder();
            foreach (var pair in parameterValues)
            {
                var jsonValue = encodeParameter(pair.Value);
                sbQuery.AppendFormat("{0}={1}", System.Web.HttpUtility.UrlEncode(pair.Key), System.Web.HttpUtility.UrlEncode(jsonValue));
                sbQuery.Append('&');
            }
            if (sbQuery.Length > 0)
            {
                // Remove trailing '&':
                sbQuery.Length = sbQuery.Length - 1;
            }

            ub.Query = sbQuery.ToString();
        }
    }
}

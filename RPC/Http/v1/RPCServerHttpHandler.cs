using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Common.Web;
using RPC;

namespace RPC.v1
{
    public sealed class RPCServerHttpHandler : HttpTaskAsyncHandler
    {
        private IServiceDiscoverer discoverer;

        public RPCServerHttpHandler(IServiceDiscoverer discoverer)
        {
            this.discoverer = discoverer;
        }

        #region Version 1 API

        // Handle a v1 API request:
        async Task<JsonResponse> v1Request(Context ctx)
        {
            var rawParams = v1GetRawParameterValues(ctx);
            var metaParams = v1GetMetaParameters(rawParams);
            apiContext.MetaParameterValues = metaParams.Count == 0 ? null : metaParams;

            bool showContext = true;
            object tmp;
            if (metaParams.TryGetValue("context", out tmp))
            {
                showContext = (bool)tmp;
            }

            if (ctx.route.Length == 1)
            {
                // List categories:
                apiContext.Category = null;
                apiContext.Version = null;
                apiContext.Interface = null;
                apiContext.Method = null;
                apiContext.ParameterValues = null;

                var results = (
                    from it in discoverer.DiscoverInterfaces()
                    select it.api.Category
                ).Distinct().ToList();

                return new JsonResponse(results, showContext ? apiContext : null);
            }
            else if (ctx.route.Length == 2)
            {
                // List interfaces for a category:
                var catName = ctx.route[1];

                apiContext.Category = catName;
                apiContext.Version = null;
                apiContext.Interface = null;
                apiContext.Method = null;
                apiContext.ParameterValues = null;

                var results = (
                    from it in discoverer.DiscoverInterfaces()
                    where String.Equals(it.api.Category, catName, StringComparison.OrdinalIgnoreCase)
                    select it.api.Version
                ).Distinct().ToList();

                return new JsonResponse(results, showContext ? apiContext : null);
            }
            else if (ctx.route.Length == 3)
            {
                // List interfaces for a category:
                var catName = ctx.route[1];
                var verName = ctx.route[2];

                apiContext.Category = catName;
                apiContext.Version = verName;
                apiContext.Interface = null;
                apiContext.Method = null;
                apiContext.ParameterValues = null;

                var results = (
                    from it in discoverer.DiscoverInterfaces()
                    where String.Equals(it.api.Category, catName, StringComparison.OrdinalIgnoreCase)
                    where String.Equals(it.api.Version, verName, StringComparison.OrdinalIgnoreCase)
                    select it.api.Name
                ).Distinct().ToList();

                return new JsonResponse(results, showContext ? apiContext : null);
            }
            else if (ctx.route.Length == 4)
            {
                // List methods for an interface:
                var catName = ctx.route[1];
                var verName = ctx.route[2];
                var ifName = ctx.route[3];

                apiContext.Category = catName;
                apiContext.Version = verName;
                apiContext.Interface = ifName;
                apiContext.Method = null;
                apiContext.ParameterValues = null;

                var intf = (
                    from it in discoverer.DiscoverInterfaces()
                    where String.Equals(it.api.Category, catName, StringComparison.OrdinalIgnoreCase)
                    where String.Equals(it.api.Version, verName, StringComparison.OrdinalIgnoreCase)
                    where String.Equals(it.api.Name, ifName, StringComparison.OrdinalIgnoreCase)
                    select it
                ).SingleOrDefault();

                if (intf.ty == null)
                    throw new InvalidRequestException("Category '{0}', version '{1}', interface '{2}' not found".F(catName, verName, ifName));

                apiContext.Category = intf.api.Category;
                apiContext.Version = intf.api.Version;
                apiContext.Interface = intf.api.Name;

                // Find methods:
                var results = (
                    from mt in MethodFinder.GetMethods(intf.ty).Values
                    where mt.GetCustomAttribute<HttpMethodAttribute>() != null
                    orderby mt.Name ascending
                    select mt.Name
                ).ToList();

                return new JsonResponse(results, showContext ? apiContext : null);
            }
            else if (ctx.route.Length == 5)
            {
                var catName = ctx.route[1];
                var verName = ctx.route[2];
                var ifName = ctx.route[3];
                var methodName = ctx.route[4];

                apiContext.Category = catName;
                apiContext.Version = verName;
                apiContext.Interface = ifName;
                apiContext.Method = methodName;

                // Find the interface:
                var intf = (
                    from it in discoverer.DiscoverInterfaces()
                    where String.Equals(it.api.Category, catName, StringComparison.OrdinalIgnoreCase)
                    where String.Equals(it.api.Version, verName, StringComparison.OrdinalIgnoreCase)
                    where String.Equals(it.api.Name, ifName, StringComparison.OrdinalIgnoreCase)
                    select it
                ).SingleOrDefault();

                if (intf.ty == null)
                    throw new InvalidRequestException("Category '{0}', version '{1}', interface '{2}' not found".F(catName, verName, ifName));

                apiContext.Category = intf.api.Category;
                apiContext.Version = intf.api.Version;
                apiContext.Interface = intf.api.Name;

                // Get a RPCExecutor for the method:
                var qe = intf.api.GetExecutor(intf.ty, methodName, () => discoverer.DiscoverImplementationFor(intf.ty));

                apiContext.Method = qe.Method.Name;

                object metaCommand;
                if (metaParams.TryGetValue("meta", out metaCommand))
                {
                    // Meta command request:
                    apiContext.MetaCommand = (string)metaCommand;

                    // Execute the query:
                    object results = await qe.ExecuteMeta(apiContext.MetaCommand, metaParams);

                    return new JsonResponse(results, showContext ? apiContext : null);
                }
                else
                {
                    // Normal, non-meta request:

                    // Validate that the HTTP method (GET, POST, etc.) is used appropriately:
                    // TODO(jsd): Accept replacement verb header for uncommon verbs e.g. DELETE, PUT
                    if (!qe.HttpMethodAttr.HttpMethod.Equals(ctx.req.HttpMethod, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidRequestException("This action requires the HTTP {0} method".F(qe.HttpMethodAttr.HttpMethod));

                    // Bind the parameter values:
                    var parameterValues = v1BindParameters(qe, rawParams);
                    apiContext.ParameterValues = parameterValues;

                    // Execute the query:
                    object results = await qe.Execute(parameterValues);

                    return new JsonResponse(results, showContext ? apiContext : null);
                }
            }
            else
            {
                throw new InvalidRequestException("Requests must be of the form GET or POST `/v1/{category}/{version}/{interface}/{method}`");
            }
        }

        static Dictionary<string, string> v1GetRawParameterValues(Context ctx)
        {
            var req = ctx.req;

            // Determine where to grab parameter values from in the request:
            System.Collections.Specialized.NameValueCollection nvc;
            if (req.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                nvc = req.Unvalidated.QueryString;
            }
            else
            {
                nvc = req.Unvalidated.Form;
            }

            var parameters = new Dictionary<string, string>(nvc.Count);
            foreach (string key in nvc.AllKeys)
            {
                string[] strValues = nvc.GetValues(key);
                if (strValues == null || strValues.Length == 0) continue;
                parameters.Add(key, strValues[0]);
            }
            return parameters;
        }

        static Dictionary<string, object> v1GetMetaParameters(Dictionary<string, string> rawParams)
        {
            var metaParameters = new Dictionary<string, object>(rawParams.Count);
            foreach (var pair in rawParams)
            {
                if (!pair.Key.StartsWith("$")) continue;

                string paramName = pair.Key.Substring(1);
                string jsonValue = pair.Value;

                try
                {
                    // Deserialize from JSON:
                    object value = Json.Serializer.Deserialize(new JsonTextReader(new StringReader(jsonValue)));
                    metaParameters.Add(paramName, value);
                }
                catch (Exception ex)
                {
                    throw new InvalidRequestException("While deserializing parameter '{0}': {1}".F(paramName, ex.Message));
                }
            }
            return metaParameters;
        }

#if true
        static Dictionary<string, object> v1BindParameters(IExecutor qe, Dictionary<string, string> parameterJsonValues)
        {
            var rpcParameters = qe.Parameters;

            // Bind each expected parameter string to a value, deserializing from JSON:
            var parameters = new Dictionary<string, object>(rpcParameters.Count);
            foreach (var pair in rpcParameters)
            {
                string paramName = pair.Key;
                string jsonValue;
                if (!parameterJsonValues.TryGetValue(paramName, out jsonValue))
                {
                    // Error on non-optional params:
                    if (!pair.Value.IsOptional)
                        throw new InvalidRequestException("Missing required parameter '{0}' of type '{1}'".F(paramName, pair.Value.TypeName()));
                    continue;
                }

                try
                {
                    // Try to bind the string value to a CLR type:
                    Type prmType = pair.Value.Type;

                    // Deserialize from JSON:
                    object value = Json.Serializer.Deserialize(new StringReader(jsonValue), prmType);
                    parameters.Add(paramName, value);
                }
                catch (InvalidOperationException)
                {
                    // Couldn't parse the value:
                    throw new InvalidRequestException("Invalid parameter format for '{0}' of type '{1}'; failed while parsing".F(paramName, pair.Value.TypeName()));
                }
                catch (NotSupportedException ex)
                {
                    // Type not supported for binding:
                    throw new InvalidRequestException("While binding parameter '{0}' of type '{1}': {2}".F(paramName, pair.Value.TypeName(), ex.Message));
                }
                catch (Exception ex)
                {
                    throw new InvalidRequestException("While binding parameter '{0}' of type '{1}': {2}".F(paramName, pair.Value.TypeName(), ex.Message));
                }
            }

            return parameters;
        }
#else
        static object[] v1BindParameters(IExecutor re, Dictionary<string, string> parameterJsonValues)
        {
            ParameterInfo[] methodParameters = re.Method.GetParameters();

            // Bind each expected parameter string to a value, deserializing from JSON:
            var parameters = new object[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; ++i)
            {
                var mp = methodParameters[i];
                string paramName = mp.Name;
                var paramType = mp.ParameterType;
                string jsonValue;
                if (!parameterJsonValues.TryGetValue(paramName, out jsonValue))
                    throw new InvalidRequestException("Missing required parameter '{0}' of type '{1}'".F(paramName, paramType.GetCSharpDisplayName()));

                try
                {
                    // Deserialize from JSON:
                    object value = Json.Serializer.Deserialize(new StringReader(jsonValue), paramType);
                    parameters[i] = value;
                }
                catch (InvalidOperationException)
                {
                    // Couldn't parse the value:
                    throw new InvalidRequestException("Invalid parameter format for '{0}' of type '{1}'; failed while parsing".F(paramName, paramType.GetCSharpDisplayName()));
                }
                catch (NotSupportedException ex)
                {
                    // Type not supported for binding:
                    throw new InvalidRequestException("While binding parameter '{0}' of type '{1}': {2}".F(paramName, paramType.GetCSharpDisplayName(), ex.Message));
                }
                catch (Exception ex)
                {
                    throw new InvalidRequestException("While binding parameter '{0}' of type '{1}': {2}".F(paramName, paramType.GetCSharpDisplayName(), ex.Message));
                }
            }

            return parameters;
        }
#endif

        #endregion

        struct Context
        {
            public readonly HttpRequest req;
            public readonly HttpResponse rsp;
            public readonly JsonTextWriter jtw;
            public readonly string[] route;

            public Context(HttpRequest req_, HttpResponse rsp_, JsonTextWriter jtw_, string[] route_)
            {
                req = req_;
                rsp = rsp_;
                jtw = jtw_;
                route = route_;
            }
        }

        RPCContext apiContext;

        static readonly char[] _splitChars = new char[] { '/' };

        public override bool IsReusable { get { return false; } }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var req = context.Request;

            // Set up our default response headers:
            var rsp = context.Response;
            rsp.Buffer = true;
            rsp.TrySkipIisCustomErrors = true;
            rsp.ContentType = "application/json; charset=utf-8";
            rsp.ContentEncoding = Encoding.UTF8;

            // Create a JsonTextWriter to manually stream out the response as we can:
            using (var jtw = new JsonTextWriter(context.Response.Output))
            {
                // Default to successful from here on out:
                rsp.StatusCode = 200;
                rsp.AddHeader("Access-Control-Allow-Origin", "*");

                // Remove the /path/to/file.ashx prefix from the request URL:
                var basePath = req.ApplicationPath.RemoveIfEndsWith("/") + req.AppRelativeCurrentExecutionFilePath.Substring(1);
                var url = req.Unvalidated.Url.AbsolutePath.RemoveIfStartsWith(basePath, StringComparison.OrdinalIgnoreCase).RemoveIfStartsWith("/", StringComparison.Ordinal);

                // Split the url by '/' into route portions:
                var route = url.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);
                // Unescape route parts:
                for (int i = 0; i < route.Length; ++i)
                    route[i] = Uri.UnescapeDataString(route[i]);

                var ctx = new Context(req, rsp, jtw, route);

                JsonResponse finalResponse;
                apiContext = new RPCContext();

                try
                {
                    if (route.Length == 0)
                        return;

                    // First route element must be version:
                    string version = route[0];
                    if (version == "v1")
                    {
                        finalResponse = await v1Request(ctx);
                    }
                    else
                    {
                        throw new InvalidRequestException("Unknown API version '{0}'".F(route[0]));
                    }
                }
                catch (TargetInvocationException ex)
                {
                    finalResponse = ReportException(ctx, ex.InnerException);
                }
                catch (Exception ex)
                {
                    finalResponse = ReportException(ctx, ex);
                }

                // Set the response status code:
                if (finalResponse.statusCode != 0)
                    rsp.StatusCode = finalResponse.statusCode;

                // Serialize the result:
                Json.Serializer.Serialize(jtw, finalResponse);
            }
        }

        JsonResponse ReportException(Context ctx, Exception ex)
        {
            var rsp = new JsonResponse(500, apiContext, SerializeErrors(ex));

            if (ex is InvalidRequestException)
                rsp.statusCode = 400;

            return rsp;
        }

        public static JsonError[] SerializeErrors(Exception ex)
        {
            if (ex is InvalidRequestException)
            {
                return new JsonError[1] { new JsonError {
                    message = ex.Message,
                    type = ex.GetType().FullName
                } };
            }
            else
            {
                return new JsonError[1] { new JsonError {
                    message = ex.Message,
                    type = ex.GetType().FullName,
                    stack = ex.StackTrace,
                    inner = ex.InnerException == null ? null : SerializeErrors(ex.InnerException)
                } };
            }
        }
    }

    public sealed class InvalidRequestException : Exception
    {
        public InvalidRequestException(string message)
            : base(message)
        {
        }
    }
}

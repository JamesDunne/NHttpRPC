using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Web
{
    public class JsonResponse<TResult, TContext>
        where TContext : class
    {
        // All responses must start with a boolean success value:
        [JsonProperty(Required = Required.Always)]
        public bool success { get; set; }

        // HTTP status code:
        public int statusCode { get; set; }

        // Context describing the request for proper error diagnosis:
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TContext context { get; set; }

        // Errors are optional:
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonError[] errors { get; set; }

        // Results are optional, mutually exclusive with errors:
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TResult results { get; set; }

        public JsonResponse()
        {
        }

        public JsonResponse(TResult results, TContext context = null)
        {
            this.success = true;
            this.statusCode = 200;
            this.context = context;
            this.errors = null;
            this.results = results;
        }

        public JsonResponse(int statusCode, TContext context = null, params JsonError[] errors)
        {
            this.success = false;
            this.statusCode = statusCode;
            this.context = context;
            this.errors = errors;
        }
    }

    public class JsonResponse : JsonResponse<object, object>
    {
        public JsonResponse()
            : base()
        {
        }

        public JsonResponse(object results, object context = null)
            : base(results, context)
        {
        }

        public JsonResponse(int statusCode, object context = null, params JsonError[] errors)
            : base(statusCode, context, errors)
        {
        }

        public static JsonResponse<T, object> Success<T, U>(T results, object context = null)
        {
            return new JsonResponse<T, object>(results, context);
        }

        public static JsonResponse Error(int statusCode, object context = null, params JsonError[] errors)
        {
            return new JsonResponse(statusCode, context, errors);
        }
    }
}

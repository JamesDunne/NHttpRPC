using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Web
{
    public class JsonError
    {
        // All errors must at least have a message field.
        [JsonProperty(Required = Required.Always)]
        public string message;

        public string type;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string code;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string stack;

        [JsonProperty(NullValueHandling= NullValueHandling.Ignore)]
        public JsonError[] inner;

        // Errors can have additional fields so we want to capture those.
        [JsonExtensionData]
        public Dictionary<string, JToken> _fields;

        public static JsonError[] FromException(Exception ex)
        {
            return new JsonError[1] { new JsonError {
                message = ex.Message,
                type = ex.GetType().FullName
            } };
        }
    }
}

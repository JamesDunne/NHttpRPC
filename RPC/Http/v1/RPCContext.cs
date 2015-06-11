using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPC
{
    /// <summary>
    /// An RPC method's context describing how it was called.
    /// </summary>
    public sealed class RPCContext
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Category;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Version;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Interface;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Method;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MetaCommand;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> MetaParameterValues;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> ParameterValues;
    }
}

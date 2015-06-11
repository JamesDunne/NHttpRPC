using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Web
{
    public static class Json
    {
        public static JsonConverter[] Converters = new JsonConverter[] {
        };

        /// <summary>
        /// Default contract serializer for JSON.
        /// </summary>
        public static JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>(Converters),
        });

        public static string Serialize(object obj)
        {
            using (var sw = new System.IO.StringWriter())
            {
                Serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }
    }
}

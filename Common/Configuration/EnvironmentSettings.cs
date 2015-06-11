using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Configuration
{
    public static class EnvironmentSettings
    {
        /// <summary>
        /// Attempts to get a domain-specific value by its key name and convert it to a <typeparamref name="TConvertTo"/> type.
        /// If not found, <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="converter">A function to convert the string value into a type of <typeparamref name="TConvertTo"/></param>
        /// <param name="defaultValue">A custom default value to use if the key is not found</param>
        /// <returns></returns>
        public static TConvertTo Get<TConvertTo>(string key, Converter<string, TConvertTo> converter, TConvertTo defaultValue)
        {
            string strValue = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            if (strValue == null)
                return defaultValue;

            return converter(strValue);
        }

        public static readonly ConnectionStringsCollection ConnectionStrings;

        public sealed class ConnectionStringsCollection {
            public string this[string cn] {
                get {
                    var cs = System.Configuration.ConfigurationManager.ConnectionStrings[cn];
                    if (cs == null) return null;
                    return cs.ConnectionString;
                }
            }
        }
    }
}

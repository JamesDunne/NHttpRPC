using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Web;

namespace RPC
{
    /// <summary>
    /// An RPC method exception serialized in JSON.
    /// </summary>
    public sealed class RPCException : Exception
    {
        public RPCException(RPCContext ctx, JsonError[] errors)
        {
            this.APIContext = ctx ?? new RPCContext();
            this.Errors = errors;
        }

        public JsonError[] Errors { get; set; }

        public readonly RPCContext APIContext;

        public override string StackTrace
        {
            get
            {
                var err = Errors.FirstOrDefault();
                if (err == null) return "";
                return err.stack ?? "";
            }
        }

        public override string Message
        {
            get
            {
                var sb = new StringBuilder();

                sb.AppendFormat("RPC: {0}/{1}/{2}/{3}", APIContext.Category ?? "null", APIContext.Version ?? "null", APIContext.Interface ?? "null", APIContext.Method ?? "null");
                if (APIContext.ParameterValues != null && (APIContext.ParameterValues.Count > 0))
                {
                    sb.Append("?");
                    var pv = APIContext.ParameterValues.ToArray(APIContext.ParameterValues.Count);
                    for (int i = 0; i < pv.Length; ++i)
                    {
                        var pair = pv[i];
                        sb.AppendFormat("{0}={1}", pair.Key, pair.Value);
                        if (i < pv.Length - 1) sb.Append("&");
                    }
                }
                sb.AppendLine();

                if (Errors.Length == 0)
                {
                    sb.Append("No errors");
                }
                else if (Errors.Length == 1)
                {
                    sb.Append(Errors[0].message);
                    if (Errors[0]._fields == null) return sb.ToString();
                    sb.Append("\nAdditional data:\n");
                    foreach (var pair in Errors[0]._fields)
                        sb.AppendFormat("  \"{0}\": {1}\n", pair.Key, pair.Value.ToString(Newtonsoft.Json.Formatting.None, Json.Converters));
                }
                else
                {
                    for (int i = 0; i < Errors.Length; ++i)
                    {
                        sb.AppendFormat("[{0}]: ", i);
                        sb.Append(Errors[i].message);
                        if (Errors[i]._fields == null) continue;
                        sb.Append("\n  Additional data:\n");
                        foreach (var pair in Errors[i]._fields)
                            sb.AppendFormat("    \"{0}\": {1}\n", pair.Key, pair.Value.ToString(Newtonsoft.Json.Formatting.None, Json.Converters));
                    }
                }
                return sb.ToString();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPC.SQLAPI
{
    /// <summary>
    /// The base attribute applied to SQLAPI methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class SQLCommandAttribute : HttpMethodAttribute
    {
        public readonly string CommandText;
        public readonly string ConnectionName;
        public readonly Type TextProcessorType;

        protected SQLCommandAttribute(string httpMethod, string commandText, string connectionName = null, Type textProcessorType = null)
            : base(httpMethod)
        {
            if (commandText == null) throw new ArgumentNullException("commandText");

            if (textProcessorType != null)
            {
                var expectedTextProcessorType = typeof(SQLTextProcessor);
                if (textProcessorType.GetInterfaces().Any(i => i.IsGenericType && i == expectedTextProcessorType))
                    throw new ArgumentException("textProcessorType '{0}' must implement the `{1}` interface".F(textProcessorType.FullName, expectedTextProcessorType.GetCSharpDisplayName()), "textProcessorType");

                this.TextProcessorType = textProcessorType;
            }

            this.ConnectionName = connectionName;
            this.CommandText = commandText;
        }
    }
}

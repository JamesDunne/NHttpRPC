using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the dynamic assembly builder:
            var cb = RPC.v1.ClientBuilder.Start("ClientExample.Dynamic");

            // Compile and instantiate the HTTP client implementation:
            var exClient = cb.GetClient<IExampleService>(new Uri("http://localhost/rpc"));

            // Invoke the Test() method synchronously:
            try
            {
                var result = exClient.Test().Result;

                Console.WriteLine(result);
            }
            catch (AggregateException aex)
            {
                // AggregateException is handled here because we're sync invoking async.
                foreach (var ex in aex.InnerExceptions) {
                    Console.Error.WriteLine(ex);
                }
            }
        }
    }
}

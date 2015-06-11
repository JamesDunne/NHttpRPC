using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPC.JsonRPC;

namespace ClientExample
{
    [JsonRPC("example", "v1", "test")]
    public interface IExampleService
    {
        [HttpGet()]
        Task<int> Test();
    }
}

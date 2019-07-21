using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansSimple;
using System.Threading.Tasks;

namespace BookWorm.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GreetingController : ControllerBase
    {
        IClusterClient _client;
        public GreetingController(IClusterClient client)
        {
            _client = client;
        }

        // GET: api/Greeting/some
        [HttpGet("{word}", Name = "Get")]
        public Task<string> Get(string word)
        {
            var grain = _client.GetGrain<ISimple>(0);
            return grain.SayHello(word);
        }
    }
}

using GrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System.Threading.Tasks;

namespace BookWorm.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PersistenceController : ControllerBase
	{
		IClusterClient _client;
		public PersistenceController(IClusterClient client)
		{
			_client = client;
		}

		// GET: api/Persistence/5
		[HttpGet("{id}")]
		public async Task<string> Get(int id)
		{
			var grain = _client.GetGrain<IPersistence>(id);
			var creationTime = await grain.GetCreationTime();
			return $"My ID is {id}. I was created at {creationTime.ToShortTimeString()} UTC";
		}
	}
}

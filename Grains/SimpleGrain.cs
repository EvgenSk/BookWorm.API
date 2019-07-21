using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OrleansSimple
{
	public class SimpleGrain : Orleans.Grain, ISimple
	{
		private readonly ILogger logger;

		public SimpleGrain(ILogger<SimpleGrain> logger)
		{
			this.logger = logger;
		}

		Task<string> ISimple.SayHello(string greeting)
		{
			logger.LogInformation($"\n SayHello message received: greeting = '{greeting}'");
			return Task.FromResult($"\n Client said: '{greeting}', so HelloGrain says: Hello!");
		}
	}
}
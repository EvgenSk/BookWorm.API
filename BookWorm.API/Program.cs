using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLP.API.Core;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using System.Threading.Tasks;
using WordsAPI.NET.Core;

namespace BookWorm.API
{
	public class Program
	{
		public static void Main(string[] args) =>
			CreateWebHostBuilder(args).Build().Run();

		public static IHostBuilder CreateWebHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureServices((builderContext, services) =>
			{
				// setup common services
				// WordsAPI
				// CoreNLP
			})
			.ConfigureWebHost(webHostBuilder =>
			{
				webHostBuilder.UseStartup<Startup>();
			})
			.UseOrleans((builderVontext, services) =>
			{
				// setup orleans storages
			});
	}
}

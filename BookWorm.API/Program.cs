using GrainInterfaces;
using Grains;
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
	public static class Program
	{
		public static void Main(string[] args) =>
			CreateWebHostBuilder(args).Build().Run();

		public static IHostBuilder CreateWebHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureServices((builderContext, services) =>
			{
				var wordsAPIOptionsSection = builderContext.Configuration.GetSection("WordsAPI");
				var stanfordNLPOptionsSection = builderContext.Configuration.GetSection("StanfordNLP");

				services
				.Configure<WordsAPIOptions>(wordsAPIOptionsSection)
				.Configure<StanfordNLPOptions>(stanfordNLPOptionsSection)
				.AddWordsAPIClient()
				.AddStanfordNLPClient();
			})
			.ConfigureWebHostDefaults(webHostBuilder =>
			{
				webHostBuilder.UseStartup<Startup>();
			})
			.UseOrleans((builderContext, siloBuilder) =>
			{
				siloBuilder
				.ConfigureServices(services =>
				{
					var mongoOptionsSection = builderContext.Configuration.GetSection("MongoDBGrainStorage");
					services
					.Configure<MongoDBOptions>(mongoOptionsSection)
					.Configure<MongoDBGrainStorageOptions>(OrleansHostingHelper.StorageName, mongoOptionsSection);
				})
				.UseMongoDBClient()
				.UseDashboard()
				.UseLocalhostClustering()
				.AddMongoDBGrainStorage(OrleansHostingHelper.StorageName)
				.ConfigureApplicationParts(parts =>
				{
					parts.AddApplicationPart(typeof(IWordInfoGrain).Assembly).WithReferences();
					parts.AddApplicationPart(typeof(WordInfoGrain).Assembly).WithReferences();
				});
			});
	}
}

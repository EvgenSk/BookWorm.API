using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLP.API.Core;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using OrleansSimple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordsAPI.NET.Core;

namespace BookWorm.API
{
	public static class OrleansHostingHelper
	{
		private static ISiloHost BuildSiloHost(IConfiguration configuration)
		{
			var cacheName = "Cache";
			var storageName = "Storage";
			var compoundName = "Compound";

			return new SiloHostBuilder()
				.Configure<RedisGrainStorageOptions>(configuration.GetSection("RedisGrainStorage"))
				.Configure<MongoDBGrainStorageOptions>(configuration.GetSection("MongoDBGrainStorage"))
				.Configure<WordsAPIOptions>(configuration.GetSection("WordsAPI"))
				.Configure<StanfordNLPClientOptions>(configuration.GetSection("StanfordNLPClient"))
				.UseDashboard()
				.UseLocalhostClustering()
				.AddRedisGrainStorage(cacheName)
				.AddMongoDBGrainStorage(storageName)
				.AddCompoundGrainStorage(compoundName, c => {
					c.CacheName = cacheName;
					c.StorageName = storageName;
				})
				.AddWordsAPIClient()
				.AddStanfordNLPClient()
				.ConfigureApplicationParts(parts =>
				{
					parts.AddApplicationPart(typeof(ISimple).Assembly).WithReferences();
					parts.AddApplicationPart(typeof(SimpleGrain).Assembly).WithReferences();
				})
				.Build();
		}

		public static async Task<IServiceCollection> AddOrleansClusterClient(this IServiceCollection services, IConfiguration configuration)
		{
			ISiloHost silo = BuildSiloHost(configuration);
			await silo.StartAsync();
			var client = silo.Services.GetRequiredService<IClusterClient>();
			return 
				services
				.AddSingleton<IClusterClient>(client)
				.AddSingleton<IGrainFactory>(client);
		}
	}
}

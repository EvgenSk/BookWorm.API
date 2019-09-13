using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
using WordsAPI.NET.OrleansHostingExtensions;

namespace BookWorm.API
{
	public static class OrleansHostingHelper
	{
        static string cacheName = "Cache";
        static string storageName = "Storage";
        static string compoundName = "Compound";

        private static ISiloHost BuildSiloHost(IConfiguration configuration)
		{
            var redisOptionsSection = configuration.GetSection("RedisGrainStorage");
            var mongoOptionsSection = configuration.GetSection("MongoDBGrainStorage");
            var wordsAPIOptionsSection = configuration.GetSection("WordsAPI");
            var stanfordNLPOptionsSection = configuration.GetSection("StanfordNLPClient");

            return new SiloHostBuilder()
                .ConfigureServices((hostBuilderContext, services) => {
                    hostBuilderContext.Configuration = configuration;
                    services
                    .Configure<RedisGrainStorageOptions>(cacheName, redisOptionsSection)
                    .Configure<MongoDBGrainStorageOptions>(storageName, mongoOptionsSection);
                })
                .Configure<RedisGrainStorageOptions>(redisOptionsSection)
                .Configure<MongoDBGrainStorageOptions>(mongoOptionsSection)
                .Configure<WordsAPIOptions>(wordsAPIOptionsSection)
                .Configure<StanfordNLPClientOptions>(stanfordNLPOptionsSection)
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
					parts.AddApplicationPart(typeof(IWordsAPIGrainServiceClient).Assembly).WithReferences();
				})
                .Build();
		}

		public static async Task<IServiceCollection> AddOrleansClusterClient(this IServiceCollection services, IConfiguration configuration)
		{
            ISiloHost siloHost = BuildSiloHost(configuration);
            await siloHost.StartAsync();
			var client = siloHost.Services.GetRequiredService<IClusterClient>();
			return
				services
				.AddSingleton<ISiloHost>(siloHost)		// in order to be disposed correctly
				.AddSingleton<IClusterClient>(client)
				.AddSingleton<IGrainFactory>(client);
		}
	}
}

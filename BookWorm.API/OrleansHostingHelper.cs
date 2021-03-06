﻿using Microsoft.Extensions.Configuration;
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
using System.Net.Http;
using System.Threading.Tasks;
using WordsAPI.NET.Core;
using WordsAPI.NET.OrleansHostingExtensions;

namespace BookWorm.API
{
	public static class OrleansHostingHelper
	{
		public static string StorageName => "Storage";

		private static ISiloHost BuildSiloHost(IConfiguration configuration)
		{
			var mongoOptionsSection = configuration.GetSection("MongoDBGrainStorage");
			var wordsAPIOptionsSection = configuration.GetSection("WordsAPI");
			var stanfordNLPOptionsSection = configuration.GetSection("StanfordNLP");

			return new SiloHostBuilder()
				.Configure<WordsAPIOptions>(wordsAPIOptionsSection)
				.Configure<StanfordNLPOptions>(stanfordNLPOptionsSection)
				.ConfigureServices(s =>
				{
					s.Configure<MongoDBGrainStorageOptions>(StorageName, mongoOptionsSection);
				})
				.AddWordsAPIGrainService()
				.AddStanfordNLPGrainService()
				.UseDashboard()
				.UseLocalhostClustering()
				.AddMongoDBGrainStorage(StorageName)
				.ConfigureApplicationParts(parts =>
				{
					parts.AddApplicationPart(typeof(ISimple).Assembly).WithReferences();
					parts.AddApplicationPart(typeof(SimpleGrain).Assembly).WithReferences();
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
				.AddSingleton<ISiloHost>(siloHost)      // in order to be disposed correctly
				.AddSingleton<IClusterClient>(client)
				.AddSingleton<IGrainFactory>(client);
		}
	}
}

using CommonTypes;
using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WordsAPI.NET.Core;
using WordsAPI.NET.Core.Types;
using WordsAPI.NET.OrleansHostingExtensions;

namespace Grains
{
	[StorageProvider(ProviderName = "Storage")]
	public class WordInfoGrain : Grain<WordInfo>, IWordInfoGrain
	{
		readonly IWordsAPIClient _wordsAPIClient;
		private Task _writeState;

		public WordInfoGrain(IWordsAPIClient wordsAPIClient)
		{
			_wordsAPIClient = wordsAPIClient;
		}
		public Task<WordInfo> GetWordInfo() => Task.FromResult(State);

		private static WordInfo WordInfoFromEverything(Everything everything) =>
			new WordInfo { Lemma = everything?.Word, Everything = everything };

		public override async Task OnActivateAsync()
		{
			await base.OnActivateAsync();
			if (State?.Lemma is null || State?.Everything is null)
			{
				var everything = await _wordsAPIClient.GetWordInfoAsync<Everything>(this.GrainReference.GetPrimaryKeyString());
				State = WordInfoFromEverything(everything);
				_writeState = WriteStateAsync();
			}
		}

		public override async Task OnDeactivateAsync()
		{
			await _writeState;
			await base.OnDeactivateAsync();
		}
	}
}

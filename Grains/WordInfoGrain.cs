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
using WordsAPI.NET.OrleansHostingExtensions;

namespace Grains
{
	[StorageProvider(ProviderName = "Compound")]
	public class WordInfoGrain : Grain<WordInfo>, IWordInfoGrain
	{
		readonly IWordsAPIClient _wordsAPIClient;
		private Task _writeState;

		public WordInfoGrain(IWordsAPIClient wordsAPIClient)
		{
			_wordsAPIClient = wordsAPIClient;
		}
		public Task<WordInfo> GetWordInfo() => Task.FromResult(State);

		public override async Task OnActivateAsync()
		{
			await base.OnActivateAsync();
			if (State is null)
			{
				State = await _wordsAPIClient.GetWordInfoAsync<WordInfo>(this.GrainReference.GetPrimaryKeyString());
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

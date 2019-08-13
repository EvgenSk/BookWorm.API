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

namespace Grains
{
    [StorageProvider(ProviderName = "Compound")]
    public class WordInfoGrain : Grain<WordInfo>, IWordInfoGrain
    {
        private Task _writeState;
        public Task<WordInfo> GetWordInfo() => Task.FromResult(State);

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            if(State is null)
            {
                var wordsAPIClient = ServiceProvider.GetRequiredService<IWordsAPIClient>();
                State = await wordsAPIClient.GetWordInfoAsync<WordInfo>(this.GrainReference.GetPrimaryKeyString());
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

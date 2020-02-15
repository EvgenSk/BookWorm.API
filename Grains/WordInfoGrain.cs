using CommonTypes;
using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
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

		static CommonTypes.PartOfSpeech? PosToPos(WordsAPI.NET.Core.Types.PartOfSpeech? pos) =>
			pos switch
			{
				WordsAPI.NET.Core.Types.PartOfSpeech.noun => CommonTypes.PartOfSpeech.noun,
				WordsAPI.NET.Core.Types.PartOfSpeech.pronoun => CommonTypes.PartOfSpeech.pronoun,
				WordsAPI.NET.Core.Types.PartOfSpeech.verb => CommonTypes.PartOfSpeech.verb,
				WordsAPI.NET.Core.Types.PartOfSpeech.adjective => CommonTypes.PartOfSpeech.adjective,
				WordsAPI.NET.Core.Types.PartOfSpeech.adverb => CommonTypes.PartOfSpeech.adverb,
				WordsAPI.NET.Core.Types.PartOfSpeech.preposition => CommonTypes.PartOfSpeech.preposition,
				WordsAPI.NET.Core.Types.PartOfSpeech.conjunction => CommonTypes.PartOfSpeech.conjunction,
				WordsAPI.NET.Core.Types.PartOfSpeech.interjection => CommonTypes.PartOfSpeech.interjection,
				WordsAPI.NET.Core.Types.PartOfSpeech.definite_article => CommonTypes.PartOfSpeech.definite_article,
				WordsAPI.NET.Core.Types.PartOfSpeech.indefinite_article => CommonTypes.PartOfSpeech.indefinite_article,
				null => null,
				_ => throw new ArgumentException("unexpected POS")
			};

		public static WordInfo WordInfoFromWordsAPIEverything(Everything everything) =>
			new WordInfo
			{
				Word = everything.Word,
				Pronunciation = everything.Pronunciation.All,
				Definitions = everything.Results.Select(r => new Definition
				{
					DefinitionText = r.Definition,
					Examples = r.Examples,
					PartOfSpeech = PosToPos(r.PartOfSpeech.Value),
					Synonyms = r.Synonyms
				}),
			};

		public override async Task OnActivateAsync()
		{
			await base.OnActivateAsync();
			if (State?.Word is null)
			{
				var lemma = this.GrainReference.GetPrimaryKeyString();
				var everything = await _wordsAPIClient.GetWordInfoAsync<Everything>(lemma);
				State = WordInfoFromWordsAPIEverything(everything) ?? new WordInfo { Word = lemma };
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

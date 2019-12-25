using CommonTypes;
using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using NLP.API.Core;
using NLP.API.Core.Annotations;
using NLP.API.OrleansHostingExtensions;
using Orleans;
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
	public class ParagraphAnnotatorGrain : Grain, IParagraphAnnotatorGrain
	{
		private readonly IStanfordNLPClient _stanfordNLpClient;

		private readonly IWordsAPIClient _wordsAPIClient;

		public ParagraphAnnotatorGrain(IWordsAPIClient wordsAPIClient, IStanfordNLPClient stanfordNLPClient)
		{
			_wordsAPIClient = wordsAPIClient;
			_stanfordNLpClient = stanfordNLPClient;
		}

		private static WordInfo WordInfoFromEverything(Everything everything) =>
			new WordInfo { Lemma = everything.Word, Everything = everything };

		public async Task<(AnnotatedText, Dictionary<string, WordInfo>)> AnnotateParagraph(string text)
		{
			var annotatedText = await _stanfordNLpClient.AnnotateTextAsync(text);

			var words = annotatedText.Sentences.SelectMany(s => s.Tokens.Select(t => t.lemma)).Distinct();

			var lemmaTasks = words.Select(lemma => (lemma, _wordsAPIClient.GetWordInfoAsync<Everything>(lemma))).ToList();

			await Task.WhenAll(lemmaTasks.Select(lt => lt.Item2));
			var dict = lemmaTasks.Select(lt => (lt.lemma, lt.Item2.Result)).ToDictionary(kv => kv.lemma, kv => WordInfoFromEverything(kv.Result));

			return (annotatedText, dict);
		}
	}
}

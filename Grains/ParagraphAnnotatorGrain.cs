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
		private readonly IStanfordNLPClient _stanfordNLPClient;
		private readonly IClusterClient _clusterClient;

		public ParagraphAnnotatorGrain(IClusterClient clusterClient, IStanfordNLPClient stanfordNLPClient)
		{
			_clusterClient = clusterClient;
			_stanfordNLPClient = stanfordNLPClient;
		}

		Text StanfordToCommonTypeText(AnnotatedText annotatedText)
		{
			return new Text
			{
				Sentences = annotatedText.Sentences.Select(s => new CommonTypes.Sentence
				{
					Index = s.Index,
					Tokens = s.Tokens.Select(t => new CommonTypes.Token
					{
						Index = t.index,
						Word = t.word,
						Lemma = t.lemma,
						OriginalText = t.originalText,
						After = t.after,
						Before = t.before,
						PartOfSpeech = StanfordPosToCommonPos(t.pos ?? NLP.API.Core.Annotations.PartOfSpeech.SYM)
					}).ToList()
				}).ToList()
			};
		}

		private CommonTypes.PartOfSpeech StanfordPosToCommonPos(NLP.API.Core.Annotations.PartOfSpeech pos) =>
			pos switch
			{
				NLP.API.Core.Annotations.PartOfSpeech.JJ => CommonTypes.PartOfSpeech.adjective,
				NLP.API.Core.Annotations.PartOfSpeech.JJR => CommonTypes.PartOfSpeech.adjective,
				NLP.API.Core.Annotations.PartOfSpeech.JJS => CommonTypes.PartOfSpeech.adjective,
				NLP.API.Core.Annotations.PartOfSpeech.RB => CommonTypes.PartOfSpeech.adverb,
				NLP.API.Core.Annotations.PartOfSpeech.RBR => CommonTypes.PartOfSpeech.adverb,
				NLP.API.Core.Annotations.PartOfSpeech.RBS => CommonTypes.PartOfSpeech.adverb,
				NLP.API.Core.Annotations.PartOfSpeech.WRB => CommonTypes.PartOfSpeech.adverb,
				NLP.API.Core.Annotations.PartOfSpeech.NN => CommonTypes.PartOfSpeech.noun,
				NLP.API.Core.Annotations.PartOfSpeech.NNS => CommonTypes.PartOfSpeech.noun,
				NLP.API.Core.Annotations.PartOfSpeech.NNP => CommonTypes.PartOfSpeech.noun,
				NLP.API.Core.Annotations.PartOfSpeech.NNPS => CommonTypes.PartOfSpeech.noun,
				NLP.API.Core.Annotations.PartOfSpeech.VB => CommonTypes.PartOfSpeech.verb,
				NLP.API.Core.Annotations.PartOfSpeech.VBD => CommonTypes.PartOfSpeech.verb,
				NLP.API.Core.Annotations.PartOfSpeech.VBG => CommonTypes.PartOfSpeech.verb,
				NLP.API.Core.Annotations.PartOfSpeech.VBN => CommonTypes.PartOfSpeech.verb,
				NLP.API.Core.Annotations.PartOfSpeech.VBP => CommonTypes.PartOfSpeech.verb,
				NLP.API.Core.Annotations.PartOfSpeech.VBZ => CommonTypes.PartOfSpeech.verb,
				_ => CommonTypes.PartOfSpeech.other
			};

		public async Task<(Text, Dictionary<string, WordInfo>)> AnnotateParagraph(string text)
		{
			var annotatedText = await _stanfordNLPClient.AnnotateTextAsync(text);

			var words = 
				annotatedText.Sentences.SelectMany(s => s.Tokens)
				.Where(t => t.pos != null && t.lemma.Length > 1)
				.Select(t => t.lemma)
				.Distinct();

			Task<WordInfo> GetWordInfoAsync(string lemma) =>
				_clusterClient.GetGrain<IWordInfoGrain>(lemma).GetWordInfo();

			var lemmaTasks = words.Select(lemma => (lemma, GetWordInfoAsync(lemma))).ToList();

			await Task.WhenAll(lemmaTasks.Select(lt => lt.Item2));
			var dict = lemmaTasks.Select(lt => (lt.lemma, lt.Item2.Result)).ToDictionary(kv => kv.lemma, kv => kv.Result);

			return (StanfordToCommonTypeText(annotatedText), dict);
		}
	}
}

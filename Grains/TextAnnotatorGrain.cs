using CommonTypes;
using GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using NLP.API.Core;
using NLP.API.Core.Annotations;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordsAPI.NET.Core;

namespace Grains
{
	public class TextAnnotatorGrain : Grain, ITextAnnotatorGrain
	{
		private readonly string[] ParagraphSeparators = new[] { "\r\n", "\r", "\n" };

		private IClusterClient _clusterClient { get; set; }

		public TextAnnotatorGrain(IClusterClient clusterClient)
		{
			_clusterClient = clusterClient;
		}

		public async Task<(AnnotatedText, Dictionary<string, WordInfo>)> AnnotateText(string text)
		{
			var annotatedParagraphs = await AnnotateTextByParagraphs(text);
			var texts = annotatedParagraphs.OrderBy(item => item.Item1).Select(item => item.Item2);

			AnnotatedText resultText = MergeAnnotatedParagraphs(texts);

			var dictionaries = annotatedParagraphs.Select(p => p.Item3);
			Dictionary<string, WordInfo> dictionary = MergeDictionaries(dictionaries);

			return (resultText, dictionary);
		}

		private static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
		{
			return dictionaries
				.SelectMany(dict => dict)
				.ToLookup(pair => pair.Key, pair => pair.Value)
				.ToDictionary(group => group.Key, group => group.First());
		}

		private static AnnotatedText MergeAnnotatedParagraphs(IEnumerable<AnnotatedText> texts)
		{
			var sentences = texts.SelectMany(t => t.Sentences).ToArray();
			var orderedSentences =
				Enumerable.Range(0, sentences.Length)
				.Zip(sentences, (i, s) => new Sentence { Index = i, Tokens = s.Tokens });
			var resultText = new AnnotatedText { Sentences = orderedSentences.ToList() };
			return resultText;
		}

		public async Task<List<(int, AnnotatedText, Dictionary<string, WordInfo>)>> AnnotateTextByParagraphs(string text)
		{
			var paragraphs = text.Split(ParagraphSeparators, StringSplitOptions.RemoveEmptyEntries);
			List<(int, AnnotatedText, Dictionary<string, WordInfo>)> results = new List<(int, AnnotatedText, Dictionary<string, WordInfo>)>();
			await foreach(var p in AnnotateParagraphs(paragraphs))
			{
				results.Add(p);
			}
			return results;
		}

		private async IAsyncEnumerable<(int, AnnotatedText, Dictionary<string, WordInfo>)> AnnotateParagraphs(string[] paragraphs)
		{
			foreach(var (i, p) in Enumerable.Range(0, paragraphs.Length).Zip(paragraphs, (i, p) => (i, p)))
			{
				var grain = _clusterClient.GetGrain<IParagraphAnnotatorGrain>(p.GetHashCode());
				var (text, dict) = await grain.AnnotateParagraph(p);
				yield return (i, text, dict);
			}
		}
	}
}

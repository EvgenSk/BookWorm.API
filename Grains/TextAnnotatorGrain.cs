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

		public async Task<(Text, Dictionary<string, WordInfo>)> AnnotateText(string text)
		{
			var (annotatedParagraphs, dictionary) = await AnnotateTextByParagraphs(text);
			var texts = annotatedParagraphs.OrderBy(item => item.Item1).Select(item => item.Item2);

			Text resultText = MergeAnnotatedParagraphs(texts);

			return (resultText, dictionary);
		}

		private static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
		{
			return dictionaries
				.SelectMany(dict => dict)
				.ToLookup(pair => pair.Key, pair => pair.Value)
				.ToDictionary(group => group.Key, group => group.First());
		}

		private static Text MergeAnnotatedParagraphs(IEnumerable<Text> texts)
		{
			var sentences = texts.SelectMany(t => t.Sentences).ToArray();
			var orderedSentences =
				Enumerable.Range(0, sentences.Length)
				.Zip(sentences, (i, s) => new CommonTypes.Sentence { Index = i, Tokens = s.Tokens });
			return new Text { Sentences = orderedSentences.ToList() };
		}

		public async Task<(List<(int, Text)>, Dictionary<string, WordInfo>)> AnnotateTextByParagraphs(string text)
		{
			var paragraphs = text.Split(ParagraphSeparators, StringSplitOptions.RemoveEmptyEntries);
			List<(int, Text)> resultParagraphs = new List<(int, Text)>();
			List<Dictionary<string, WordInfo>> dicts = new List<Dictionary<string, WordInfo>>();
			await foreach(var p in AnnotateParagraphs(paragraphs))
			{
				resultParagraphs.Add((p.Item1, p.Item2));
				dicts.Add(p.Item3);
			}
			return (resultParagraphs, MergeDictionaries(dicts));
		}

		private async IAsyncEnumerable<(int, Text, Dictionary<string, WordInfo>)> AnnotateParagraphs(string[] paragraphs)
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

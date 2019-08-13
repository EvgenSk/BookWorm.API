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

        private IClusterClient ClusterClient { get; set; }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            ClusterClient = ServiceProvider.GetRequiredService<IClusterClient>();
        }

        public async Task<(AnnotatedText, Dictionary<string, WordInfo>)> AnnotateText(string text)
        {
            var annotatedParagraphs = await AnnotateTextByParagraphs(text);
            var texts = annotatedParagraphs.Select(p => p.Item1);

            AnnotatedText resultText = MergeAnnotatedParagraphs(texts);

            var dictionaries = annotatedParagraphs.Select(p => p.Item2);
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

        public async Task<IEnumerable<(AnnotatedText, Dictionary<string, WordInfo>)>> AnnotateTextByParagraphs(string text)
        {
            var paragraphs = text.Split(ParagraphSeparators, StringSplitOptions.RemoveEmptyEntries);
            var tasks = AnnotateParagraphs(paragraphs).ToList();
            await Task.WhenAll(tasks.Select(t => t.Item2));

            return tasks.OrderBy(tuple => tuple.Item1).Select(t => t.Item2.Result);
        }

        private IEnumerable<(int, Task<(AnnotatedText, Dictionary<string, WordInfo>)>)> AnnotateParagraphs(string[] paragraphs)
        {
            return Enumerable.Range(0, paragraphs.Length).Zip(paragraphs, (i, p) => {
                var grain = ClusterClient.GetGrain<IParagraphAnnotatorGrain>(p.GetHashCode());
                var task = grain.AnnotateParagraph(p);
                return (i, task);
            });
        }
    }
}

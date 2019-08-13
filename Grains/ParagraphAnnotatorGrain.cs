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
    public class ParagraphAnnotatorGrain : Grain, IParagraphAnnotatorGrain
    {
        public IStanfordNLPClient StanfordNLPClient { get; private set; }
        public IWordsAPIClient WordsAPIClient { get; private set; }

        public override Task OnActivateAsync()
        {
            StanfordNLPClient = ServiceProvider.GetRequiredService<IStanfordNLPClient>();
            WordsAPIClient = ServiceProvider.GetRequiredService<IWordsAPIClient>();
            return Task.CompletedTask;
        }

        public async Task<(AnnotatedText, Dictionary<string, WordInfo>)> AnnotateParagraph(string text)
        {
            var annotatedText = await StanfordNLPClient.AnnotateTextAsync(text);

            var words = annotatedText.Sentences.SelectMany(s => s.Tokens.Select(t => t.lemma)).Distinct();
            var lemmaTasks = words.Select(lemma => (lemma, WordsAPIClient.GetWordInfoAsync<WordInfo>(lemma))).ToList();
            await Task.WhenAll(lemmaTasks.Select(lt => lt.Item2));
            var dict = lemmaTasks.Select(lt => (lt.lemma, lt.Item2.Result)).ToDictionary(kv => kv.lemma, kv => kv.Result);

            return (annotatedText, dict);
        }
    }
}

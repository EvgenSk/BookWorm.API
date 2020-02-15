using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
	public class WordInfo
	{
		public string Word { get; set; }
		public string Pronunciation { get; set; }
		public IEnumerable<Definition> Definitions { get; set; }
		public IEnumerable<Synonym> Synonyms { get; set; }
		public IEnumerable<string> Examples { get; set; }
	}

	public class Definition
	{
		public PartOfSpeech? PartOfSpeech { get; set; }
		public string DefinitionText { get; set; }
		public IEnumerable<string> Examples { get; set; }
		public IEnumerable<string> Synonyms { get; set; }
	}

	public enum PartOfSpeech
	{
		noun,
		pronoun,
		verb,
		adjective,
		adverb,
		preposition,
		conjunction,
		interjection,
		definite_article,
		indefinite_article
	}

	public class Synonym
	{
		public string SynonymText { get; set; }
		public IEnumerable<string> Examples { get; set; }
	}
}

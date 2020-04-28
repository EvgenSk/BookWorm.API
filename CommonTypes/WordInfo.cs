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
		public List<Definition> Definitions { get; set; }
		public List<Synonym> Synonyms { get; set; }
		public List<string> Examples { get; set; }
	}

	public class Definition
	{
		public PartOfSpeech? PartOfSpeech { get; set; }
		public string DefinitionText { get; set; }
		public List<string> Examples { get; set; }
		public List<string> Synonyms { get; set; }
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
		indefinite_article,
		other
	}

	public class Synonym
	{
		public string SynonymText { get; set; }
		public IEnumerable<string> Examples { get; set; }
	}

	public class Text
	{
		public List<Sentence> Sentences { get; set; }
	}

	public class Sentence
	{
		public int Index { get; set; }
		public List<Token> Tokens { get; set; }
	}

	public class Token
	{
		public int Index { get; set; }
		public string Word { get; set; }
		public string OriginalText { get; set; }
		public string Lemma { get; set; }
		public PartOfSpeech? PartOfSpeech { get; set; }
		public string Before { get; set; }
		public string After { get; set; }
	}
}

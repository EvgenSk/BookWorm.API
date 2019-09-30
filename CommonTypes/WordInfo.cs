using NLP.API.Core.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonTypes
{
	public class WordInfo
	{
		public string Word { get; set; }
		public string Lemma { get; set; }
		public PartOfSpeech? PartOfSpeech { get; set; }
	}
}

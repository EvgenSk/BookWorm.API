using CommonTypes;
using NLP.API.Core;
using NLP.API.Core.Annotations;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GrainInterfaces
{
	public interface ITextAnnotatorGrain : IGrainWithIntegerKey
	{
		Task<(Text, Dictionary<string, WordInfo>)> AnnotateText(string text);
		Task<(List<(int, Text)>, Dictionary<string, WordInfo>)> AnnotateTextByParagraphs(string text);
	}
}

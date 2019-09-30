using CommonTypes;
using NLP.API.Core.Annotations;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GrainInterfaces
{
	public interface IParagraphAnnotatorGrain : IGrainWithIntegerKey
	{
		Task<(AnnotatedText, Dictionary<string, WordInfo>)> AnnotateParagraph(string text);
	}
}

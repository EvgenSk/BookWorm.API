using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrainInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLP.API.Core;
using NLP.API.Core.Annotations;
using Orleans;

namespace BookWorm.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AnalysisController : ControllerBase
	{
		public IClusterClient ClusterClient { get; }

		public AnalysisController(IClusterClient clusterClient)
		{
			ClusterClient = clusterClient;
		}


		// POST: api/Analysis
		[HttpPost]
		public async IAsyncEnumerable<Dictionary<string, object>> Post([FromBody] string text)
		{
			// TODO: make AnnotateTextByParagraphs return stream of paragraphs (orleans streams?)
			// TODO: remove redundant entries from dictionaries
	
			var grain = ClusterClient.GetGrain<ITextAnnotatorGrain>(text.GetHashCode());
			var annotatedParagraphs = await grain.AnnotateTextByParagraphs(text);
			foreach (var (index, txt, dictionary) in annotatedParagraphs)
			{
				yield return new Dictionary<string, object>
				{
					["index"] = index,
					["Text"] = txt,
					["Dictionary"] = dictionary
				};
			}
		}

	}
}

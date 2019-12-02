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
		public async Task<string> Post([FromBody] string text)
		{
			var grain = ClusterClient.GetGrain<ITextAnnotatorGrain>(text.GetHashCode());
			var annotatedText = await grain.AnnotateText(text);
			Dictionary<string, string> textAndDictionary = new Dictionary<string, string>
			{
				["Text"] = JsonConvert.SerializeObject(annotatedText.Item1),
				["Dictionary"] = JsonConvert.SerializeObject(annotatedText.Item2)
			};

			return JsonConvert.SerializeObject(textAndDictionary);
		}

	}
}

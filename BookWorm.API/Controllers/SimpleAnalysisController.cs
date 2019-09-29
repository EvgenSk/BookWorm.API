using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLP.API.Core;

namespace BookWorm.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimpleAnalysisController : ControllerBase
    {
        public SimpleAnalysisController(IStanfordNLPClient stanfordNLPClient)
        {
            StanfordNLPClient = stanfordNLPClient;
        }

        public IStanfordNLPClient StanfordNLPClient { get; }


        // POST: api/SimpleAnalysis
        [HttpPost]
        public async Task<string> Post([FromBody] string text)
        {
            var annotatedText = await StanfordNLPClient.AnnotateTextAsync(text);
            return JsonConvert.SerializeObject(annotatedText);
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using SpellCheckService.Models;
using System.Text;

namespace SpellCheckService
{
    public static class SpellCheckService
    {
        static string _subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey");
        static string _endpoint = Environment.GetEnvironmentVariable("EndPoint");
        static string _path = "/v7.0/spellcheck?";
        static string _market = Environment.GetEnvironmentVariable("Market");
        static string _mode = Environment.GetEnvironmentVariable("Mode");

        [FunctionName("ProofReadString")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ProofRead/{query}")] HttpRequest req,
            string query,
            ILogger log)
        {
            log.LogInformation($"Proofreading query='{query}'");

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            HttpResponseMessage response = new HttpResponseMessage();
            string uri = _endpoint + _path;
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("mkt", _market));
            values.Add(new KeyValuePair<string, string>("mode", _mode));
            values.Add(new KeyValuePair<string, string>("text", query));

            using (FormUrlEncodedContent content = new FormUrlEncodedContent(values))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                response = await client.PostAsync(uri, content);
            }

            var spellingResult = new SpellingResult();
            if (response.Headers.TryGetValues("X-MSEdge-ClientID", out IEnumerable<string> header_values))
            {
                spellingResult.ClientId = header_values.First();
                log.LogInformation("Client ID: " + spellingResult.ClientId);
            }
            if (response.Headers.TryGetValues("BingAPIs-SessionId", out IEnumerable<string> session_id))
            {
                spellingResult.TraceId = session_id.First();
                log.LogInformation("Trace ID: " + spellingResult.TraceId);
            }
            try
            {
                var resultText = await response.Content.ReadAsStringAsync();
                spellingResult.Text = JsonConvert.DeserializeObject<SpellingResponseBody>(resultText);
                spellingResult.CorrectedText = ProcessResults(query, spellingResult.Text.flaggedTokens);
                log.LogInformation($"Suggested correction: {spellingResult.CorrectedText}");

                return new OkObjectResult(spellingResult.CorrectedText);
            }
            catch (Exception ex)
            {
                log.LogError(spellingResult.TraceId, ex.Message);
                return new BadRequestResult();
            }
        }

        static string ProcessResults(string text, List<FlaggedToken> flaggedTokens)
        {
            StringBuilder newTextBuilder = new StringBuilder(text);
            int indexDiff = 0;
            foreach (var token in flaggedTokens)
            {
                if (token.type == "RepeatedToken")
                {
                    newTextBuilder.Remove(token.offset - indexDiff, token.token.Length + 1);
                    indexDiff += token.token.Length + 1;
                }
                else
                {
                    if (token.suggestions.Count > 0)
                    {
                        var suggestedToken = token.suggestions.Where(x => x.score >= 0.7).FirstOrDefault();
                        if (suggestedToken == null)
                            break;
                        newTextBuilder.Remove(token.offset - indexDiff, token.token.Length);
                        newTextBuilder.Insert(token.offset - indexDiff, suggestedToken.suggestion);
                        indexDiff += token.token.Length - suggestedToken.suggestion.Length;
                    }
                }
            }
            return newTextBuilder.ToString();
        }
    }
}

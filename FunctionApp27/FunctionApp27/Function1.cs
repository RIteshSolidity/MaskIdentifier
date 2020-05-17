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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace FunctionApp27
{
    public static class MaskDetector
    {
        [FunctionName("MaskDetector")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var root = new ConfigurationBuilder().AddEnvironmentVariables()
                                .AddJsonFile("appsettings.json", true, true)
                                .Build();
            log.LogInformation("C# HTTP trigger function processed a request.");
 

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic name = JsonConvert.DeserializeObject(requestBody);
            

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Add("Prediction-Key", Environment.GetEnvironmentVariable("cognitivekey"));
            
            var content = JsonConvert.SerializeObject(name);

            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            string uri = Environment.GetEnvironmentVariable("cognitiveapi");
            client.BaseAddress = new Uri(uri);
            HttpResponseMessage response = await client.PostAsync(uri, byteContent);
            var data = JObject.Parse(await response.Content.ReadAsStringAsync());

            var properties = data.Children().Select(ch => (JProperty)ch).ToArray();

            var predictions = properties.FirstOrDefault(p => p.Name.Contains("predictions"));

            string ismask = "no mask";
            var allValues = predictions.Value.ToArray();
            foreach (var prediction in allValues) {
                decimal val = (decimal)(prediction["probability"]);
                if (val > .75M ) {
                    if ((string)(prediction["tagName"]) == "mask")
                    {
                        ismask = "mask";
                        break;
                    }
   
                }
            }



            return name != null
                ? (ActionResult)new OkObjectResult($"The image upload has people with {ismask} on")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}

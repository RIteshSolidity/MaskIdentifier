using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Internal;
using FunctionApp27;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public async Task ValidExecution()
        {

            TestHelperMethods.SetEnvironmentalVariables();
            var httpRequestMock = new Mock<HttpRequest>();
            var LogMock = new Mock<ILogger>();
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var body = new CognitiveRequestBody { url = "https://deststorage13.blob.core.windows.net/kokokokok/5e71267bc4854010b62f5925.jpg" };

            var json = JsonConvert.SerializeObject(body);
            sw.Write(json);
            sw.Flush();
            ms.Position = 0;

            httpRequestMock.Setup(req => req.Body).Returns(ms);           

            
            var result = await FunctionApp27.MaskDetector.Run(httpRequestMock.Object, LogMock.Object);

            var ismock = (OkObjectResult)result;

            Assert.NotNull(ismock.Value);
            Assert.Equal("The image upload has people with mask on", ismock.Value);
        }
    }


    public static class TestHelperMethods
    {

        public static void SetEnvironmentalVariables()
        {
            Environment.SetEnvironmentVariable("cognitiveapi", "https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/25345a18-238b-481a-8d30-4655af085f1b/detect/iterations/Iteration1/url");
            Environment.SetEnvironmentVariable("cognitivekey", "e543ecac28254ba6913bcfebe3af434d");

        }


    }

    public class CognitiveRequestBody { 
        public string url { get; set; }
    }

   
}

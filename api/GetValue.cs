using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;



[assembly: FunctionsStartup(typeof(SwaFunc.Blog.Startup))]
namespace SwaFunc.Blog
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IProvideValue, ProvideHelloWorld>();
        }
    }

    public interface IProvideValue {
        string Value {get;}
    }

    public class ProvideHelloWorld : IProvideValue
    {
        public string Value => "Azure functions";
    }

    public class GetValue
    {
        private readonly IProvideValue provideValue;
        public GetValue(IProvideValue provideValue)
        {
            this.provideValue = provideValue;
            
        }

        [FunctionName("GetValue")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This was returned via ({provideValue.Value}).";

            return new OkObjectResult(responseMessage);
        }
    }
}

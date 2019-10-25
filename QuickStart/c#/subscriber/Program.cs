// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.EventGridEdge.QuickStart.Subscriber
{
    public static class Subscriber
    {
        [FunctionName("subscriber")]
        public static async Task<IActionResult> Run(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
                    HttpRequest req, ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation($"Received event data {data}\n");
            return new OkResult();
        }
    }
}

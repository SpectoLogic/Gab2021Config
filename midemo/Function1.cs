using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace midemo
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                // DefaultAzureCredential mi = new DefaultAzureCredential();
                Azure.Identity.ManagedIdentityCredential mi = new Azure.Identity.ManagedIdentityCredential();

                string container = "demo";
                
                Azure.Storage.Blobs.BlobContainerClient containerClient = new Azure.Storage.Blobs.BlobContainerClient(
                    new Uri($"https://midemo2.blob.core.windows.net/{container}"), 
                    mi);

                using Stream streamData = ToStream("Hello World!");
                await containerClient.UploadBlobAsync("demo.txt", streamData);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            return new OkResult();
        }

        private static Stream ToStream(string text)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}


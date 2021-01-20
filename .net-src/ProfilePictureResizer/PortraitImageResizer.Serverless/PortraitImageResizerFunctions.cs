// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;

namespace PortraitImageResizer.Serverless
{
    public static class PortraitImageResizerFunctions
    {
        [FunctionName(nameof(HandleNewPictures))]
        public static async Task HandleNewPictures([EventGridTrigger]EventGridEvent receivedEvent, 
            [Blob("{data.Url}", FileAccess.Read, Connection = "PortraitStorageConnection")] Stream input,
            ExecutionContext context,
            ILogger log)
        {
            var eventData = JsonConvert.DeserializeObject<StorageBlobCreatedEventData>(receivedEvent.Data.ToString());
            string classifierPath = Path.Combine(context.FunctionAppDirectory, "data/haarcascade_frontalface_default.xml");

            PortraitGenerator generator = new PortraitGenerator(classifierPath);
            Stream portrait = generator.GeneratePortrait(input);

            CloudStorageAccount storageAccount;
            if(CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("PortraitStorageConnection"), out storageAccount))
            {
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("portraits");

                string filename = Path.GetFileName(eventData.Url);
                var blob = container.GetBlockBlobReference($"generated/{filename}");

                await blob.UploadFromStreamAsync(portrait);
                log.LogInformation($"Generated portrait for '{filename}'");
            }
        }
    }

    
}


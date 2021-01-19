// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using PortraitImageResizer.Serverless.Model;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;

namespace PortraitImageResizer.Serverless
{
    public static class PortraitImageResizerFunctions
    {
        [FunctionName(nameof(HandleNewPictures))]
        public static async Task HandleNewPictures([EventGridTrigger] EventGridEvent receivedEvent,
            [Blob("{data.Url}", FileAccess.Read, Connection = "PortraitStorageConnection")] Stream input,
            ExecutionContext context,
            ILogger log)
        {
            // TODO: fix the docs that suggested this:
            // /blobServices/default/containers/portaits/raw-images/blobs/

            // TODO: when it should have been this:
            // /blobServices/default/containers/portraits/blobs/raw-images

            StorageBlobCreatedEventData eventData = JsonConvert.DeserializeObject<StorageBlobCreatedEventData>(receivedEvent.Data.ToString());

            // todo: run our code on the picture
            PortraitGenerator generator = new PortraitGenerator(context);
            Stream portrait = generator.GeneratePortrait(input);

            // todo: save the new picture to a different folder

            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("PortraitStorageConnection"), out storageAccount))
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


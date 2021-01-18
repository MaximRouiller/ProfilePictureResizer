// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace PortraitImageResizer.Serverless
{
    public static class PortraitImageResizerFunctions
    {
        [FunctionName(nameof(HandleNewPictures))]
        public static void HandleNewPictures([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            // /blobServices/default/containers/portaits/raw-images/blobs/
            // /blobServices/default/containers/portraits/blobs/raw-images


            log.LogInformation(eventGridEvent.Subject);
            log.LogInformation(eventGridEvent.Data.ToString());

            // todo: read image raw data

            // todo: run our code on the picture

            // todo: save the new picture to a different folder
        }
    }

    
}


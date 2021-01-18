using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PortraitImageResizer.Serverless.Model
{
    public partial class EventGridStorageEvent
    {
        [JsonProperty("api")]
        public string Api { get; set; }

        [JsonProperty("clientRequestId")]
        public Guid ClientRequestId { get; set; }

        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("eTag")]
        public string ETag { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentLength")]
        public long ContentLength { get; set; }

        [JsonProperty("blobType")]
        public string BlobType { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("sequencer")]
        public string Sequencer { get; set; }
    }

    public partial class EventGridStorageEvent
    {
        public static EventGridStorageEvent FromJson(string json) => JsonConvert.DeserializeObject<EventGridStorageEvent>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this EventGridStorageEvent self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

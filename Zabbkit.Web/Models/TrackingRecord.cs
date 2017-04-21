using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zabbkit.Web.Models
{
    public class TrackingRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string DeviceId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceType DeviceType { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TrackingStatus Status { get; set; }
        public string SenderIp { get; set; }
        public string Description { get; set; }
        public int Attempt { get; set; }
    }

    public enum TrackingStatus
    {
        Accepted,
        Processing,
        Requeue,
        Delivered,
        Error,
        Ignored
    }
}
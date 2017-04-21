using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Zabbkit.Web.Models
{
    public enum DeviceType
    {
        iOS,
        Android,
        WP
    }

    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DeviceType Type { get; set; }
        [Required]
        [MinLength(Const.Validation.MinTokenLength)]
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUsed { get; set; }
    }
}
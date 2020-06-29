using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace RTL.API.Models
{
    public class Show
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("id")]
        public int ShowId { get; set; }

        [JsonProperty("name")]
        public string ShowName { get; set; }

        public List<Actor> Cast { get; set; }

        [JsonProperty("updated")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastUpdateTime { get; set; }
    }
}

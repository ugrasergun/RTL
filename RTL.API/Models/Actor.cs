using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace RTL.API.Models
{
    public class Actor
    {
        [JsonProperty("id")]
        public int ActorId { get; set; }

        [JsonProperty("name")]
        public string ActorName { get; set; }

        //[JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? BirthDay { get; set; }
    }
}

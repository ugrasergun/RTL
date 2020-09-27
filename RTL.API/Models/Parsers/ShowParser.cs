using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace RTL.API.Models.Parsers
{
    public class ShowParser
    {
        public Show ParseShowFromJSON(string jsonStr)
        {
            JObject showJson = JObject.Parse(jsonStr);
            var show = showJson.ToObject<Show>();
            var cast = showJson.Value<JObject>("_embedded")?.Value<JArray>("cast").Select(c => c.SelectToken("person").ToObject<JObject>().ToObject<Actor>());
            show.Cast = cast?.OrderByDescending(c=> c.BirthDay ?? DateTime.MinValue).ToList();

            return show;
        }
    }
}

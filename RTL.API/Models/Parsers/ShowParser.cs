using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTL.API.Models.Parsers
{
    public class ShowParser
    {
        public Show ParseShowFromJSON(string jsonStr)
        {
            JObject showJson = JObject.Parse(jsonStr);
            var show = showJson.ToObject<Show>();
            var cast = showJson.Value<JObject>("_embedded")?.Value<JArray>("cast").Select(c => c.SelectToken("person").ToObject<JObject>().ToObject<Actor>()).ToList();
            show.Cast = cast?.OrderByDescending(c=> c.BirthDay ?? DateTime.MinValue).ToList();

            return show;
        }
    }
}

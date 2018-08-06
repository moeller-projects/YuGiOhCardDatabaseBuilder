using System.Collections.Generic;
using Newtonsoft.Json;

namespace DuelLinksMeta.Models
{
    public class Skill
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }

        [JsonProperty("exclusive")]
        public bool Exclusive { get; set; }

        [JsonProperty("characters")]
        public List<Character> Characters { get; set; }
    }
}

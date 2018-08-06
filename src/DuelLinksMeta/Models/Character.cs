using Newtonsoft.Json;

namespace DuelLinksMeta.Models
{
    public class Character
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("how")]
        public string How { get; set; }
    }
}

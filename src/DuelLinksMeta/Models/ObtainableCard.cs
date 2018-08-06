using Newtonsoft.Json;

namespace DuelLinksMeta.Models
{
    public class ObtainableCard
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        [JsonProperty("how")]
        public string How { get; set; }
    }
}

using Newtonsoft.Json;

namespace DuelLinksMeta.Models
{
    public class DuelLinksCard
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

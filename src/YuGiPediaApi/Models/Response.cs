using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YuGiPediaApi.Models
{
    public class Continue
    {

        [JsonProperty("cmcontinue")]
        public string Cmcontinue { get; set; }

        [JsonProperty("continue")]
        public string NormalContinue { get; set; }
    }

    public class Categorymember
    {

        [JsonProperty("pageid")]
        public int Pageid { get; set; }

        [JsonProperty("ns")]
        public int Ns { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class Query
    {

        [JsonProperty("categorymembers")]
        public IEnumerable<Categorymember> Categorymembers { get; set; }
    }

    public class Parse
    {

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("pageid")]
        public int Pageid { get; set; }

        [JsonProperty("wikitext")]
        public string Wikitext { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Response
    {
        [JsonProperty("parse")]
        public Parse Parse { get; set; }

        [JsonProperty("batchcomplete")]
        public string Batchcomplete { get; set; }

        [JsonProperty("continue")]
        public Continue Continue { get; set; }

        [JsonProperty("query")]
        public Query Query { get; set; }
    }
}

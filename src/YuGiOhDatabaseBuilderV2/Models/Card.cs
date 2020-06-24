using System.Collections.Generic;

namespace YuGiOhDatabaseBuilderV2.Models
{
    public class Card
    {
        public Card()
        {
            AdditionalInformation = new Dictionary<string, string>();
        }

        public int Id { get; set; }
        public string NameEnglish { get; set; }
        public string DescriptionEnglish { get; set; }
        public string NameFrench { get; set; }
        public string DescriptionFrensh { get; set; }
        public string NameGerman { get; set; }
        public string DescriptionGerman { get; set; }
        public string NameItalian { get; set; }
        public string DescriptionItalian { get; set; }
        public string NamePortuguese { get; set; }
        public string DescriptionPortuguese { get; set; }
        public string NameSpanish { get; set; }
        public string DescriptionSpanish { get; set; }
        public string Attribute { get; set; }
        public string CardType { get; set; }
        public string MonsterTypes { get; set; }
        public string Level { get; set; }
        public string Attack { get; set; }
        public string Defense { get; set; }
        public string Passcode { get; set; }
        public string EffectTypes { get; set; }
        public string Rank { get; set; }
        public string LinkMarkers { get; set; }
        public string Link { get; set; }
        public string Property { get; set; }
        public string ImageUrl { get; set; }
        public string Archetypes { get; set; }
        public IDictionary<string, string> AdditionalInformation { get; set; }
    }
}

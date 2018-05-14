using System.Collections.Generic;

namespace YuGiOhWikiaApi.Models
{
    public class Card
    {
        public Card()
        {
        }
        public string realName { get; set; }
        public string name_english { get; set; }
        public string name_french { get; set; }
        public string name_german { get; set; }
        public string name_italian { get; set; }
        public string name_portuguese { get; set; }
        public string name_spanish { get; set; }
        public string attribute { get; set; }
        public string cardType { get; set; }
        public string description_english { get; set; }
        public string description_french { get; set; }
        public string description_german { get; set; }
        public string description_italian { get; set; }
        public string description_portuguese { get; set; }
        public string description_spanish { get; set; }
        public List<string> types { get; set; }
        public string level { get; set; }
        public string atk { get; set; }
        public string def { get; set; }
        public string passcode { get; set; }
        public List<string> effectTypes { get; set; }
        public string materials { get; set; }
        public string fusionMaterials { get; set; }
        public string rank { get; set; }
        public string ritualSpell { get; set; }
        public string pendulumScale { get; set; }
        public string linkMarkers { get; set; }
        public string link { get; set; }
        public string property { get; set; }
        public string summonedBy { get; set; }
        public string limitText { get; set; }
        public string synchroMaterial { get; set; }
        public string ritualMonster { get; set; }
        public string lore { get; set; }
        public List<string> archetype { get; set; }
        public string ocgStatus { get; set; }
        public string tcgAdvStatus { get; set; }
        public string tcgTrnStatus { get; set; }
        public string img { get; set; }
    }
}

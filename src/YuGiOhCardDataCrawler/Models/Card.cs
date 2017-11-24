using LiteDB;

namespace YuGiOhCardDataCrawler.Models
{
    public class Card
    {
        public Card(YuGiOhWikiaApi.Models.Card card)
        {
            Passcode = card.passcode;
            NameEnglish = card.name_english;
            NameFrench = card.name_french;
            NameGerman = card.name_german;
            NameItalian = card.name_italian;
            NamePortuguese = card.name_portuguese;
            NameSpanish = card.name_spanish;
            Attribute = card.attribute;
            CardType = card.cardType;
            DescriptionEnglish = card.description_english;
            DescriptionFrensh = card.description_french;
            DescriptionGerman = card.description_german;
            DescriptionItalian = card.description_italian;
            DescriptionPortuguese = card.description_portuguese;
            DescriptionSpanish = card.description_spanish;
            MonsterTypes = card.types;
            Level = card.level;
            Attack = card.atk;
            Defense = card.def;
            EffectTypes = card.effectTypes;
            Materials = card.materials;
            FusionMaterials = card.fusionMaterials;
            Rank = card.rank;
            RitualSpell = card.ritualSpell;
            PendulumScale = card.pendulumScale;
            LinkMarkers = card.linkMarkers;
            Link = card.link;
            Property = card.property;
            SummonedBy = card.summonedBy;
            LimitText = card.limitText;
            SynchroMaterial = card.synchroMaterial;
            RitualMonster = card.ritualMonster;
            Archetype = card.archetype;
            OcgStatus = card.ocgStatus;
            TcgAdvancedStatus = card.tcgAdvStatus;
            TcgTraditionalStatus = card.tcgTrnStatus;
            ImageUrl = card.img;
        }

        [BsonId(true)]
        public int Id { get; set; }

        [BsonField("name_english")]
        public string NameEnglish { get; set; }

        [BsonField("name_french")]
        public string NameFrench { get; set; }

        [BsonField("name_german")]
        public string NameGerman { get; set; }

        [BsonField("name_italian")]
        public string NameItalian { get; set; }

        [BsonField("name_portuguese")]
        public string NamePortuguese { get; set; }

        [BsonField("name_spanish")]
        public string NameSpanish { get; set; }

        [BsonField("attribute")]
        public string Attribute { get; set; }

        [BsonField("card_type")]
        public string CardType { get; set; }

        [BsonField("description_english")]
        public string DescriptionEnglish { get; set; }
        
        [BsonField("description_french")]
        public string DescriptionFrensh { get; set; }

        [BsonField("description_german")]
        public string DescriptionGerman { get; set; }

        [BsonField("description_italian")]
        public string DescriptionItalian { get; set; }

        [BsonField("description_portuguese")]
        public string DescriptionPortuguese { get; set; }

        [BsonField("description_spanish")]
        public string DescriptionSpanish { get; set; }

        [BsonField("monster_types")]
        public string MonsterTypes { get; set; }

        [BsonField("level")]
        public string Level { get; set; }

        [BsonField("attack")]
        public string Attack { get; set; }

        [BsonField("defense")]
        public string Defense { get; set; }

        [BsonField("passcode")]
        public string Passcode { get; set; }

        [BsonField("effect_types")]
        public string EffectTypes { get; set; }

        [BsonField("materials")]
        public string Materials { get; set; }

        [BsonField("fusion_materials")]
        public string FusionMaterials { get; set; }

        [BsonField("rank")]
        public string Rank { get; set; }

        [BsonField("ritual_spell")]
        public string RitualSpell { get; set; }

        [BsonField("pendulum_scale")]
        public string PendulumScale { get; set; }

        [BsonField("link_markers")]
        public string LinkMarkers { get; set; }

        [BsonField("link")]
        public string Link { get; set; }

        [BsonField("property")]
        public string Property { get; set; }

        [BsonField("summoned_by")]
        public string SummonedBy { get; set; }

        [BsonField("limit_text")]
        public string LimitText { get; set; }

        [BsonField("synchro_material")]
        public string SynchroMaterial { get; set; }

        [BsonField("ritual_monster")]
        public string RitualMonster { get; set; }

        [BsonField("arche_type")]
        public string Archetype { get; set; }

        [BsonField("ocg_status")]
        public string OcgStatus { get; set; }

        [BsonField("tcg_advanced_status")]
        public string TcgAdvancedStatus { get; set; }

        [BsonField("tcg_traditional_status")]
        public string TcgTraditionalStatus { get; set; }

        [BsonField("image_url")]
        public string ImageUrl { get; set; }
    }
}

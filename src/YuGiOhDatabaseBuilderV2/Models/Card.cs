using LiteDB;

namespace YuGiOhCardDatabaseBuilder.Models
{
    public class Card
    {
        public Card() { }

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
        [BsonField("supports")]
        public string Supports { get; internal set; }
        [BsonField("archetypes_and_series")]
        public string ArchetypesAndSeries { get; internal set; }
        [BsonField("supports_archetypes")]
        public string SupportsArchetypes { get; internal set; }
        [BsonField("related_to_archetype_and_series")]
        public string RelatedToArchetypeAndSeries { get; internal set; }
        [BsonField("card_categorie")]
        public string CardCategories { get; internal set; }
        [BsonField("summoning_categories")]
        public string SummoningCategories { get; internal set; }
        [BsonField("miscellaneous")]
        public string Miscellaneous { get; internal set; }
        [BsonField("counter")]
        public string Counters { get; internal set; }
        [BsonField("actions")]
        public string Actions { get; internal set; }
        [BsonField("banished_categories")]
        public string BanishedCategories { get; internal set; }
        [BsonField("anti_supports")]
        public string AntiSupports { get; internal set; }
        [BsonField("attack_categories")]
        public string AttackCategories { get; internal set; }
        [BsonField("lp_categories")]
        public string LpCategories { get; internal set; }
        [BsonField("stat_changes")]
        public string StatChanges { get; internal set; }
        [BsonField("fusion_material_for")]
        public string FusionMaterialFor { get; internal set; }
        [BsonField("anti_supports_archetypes")]
        public string AntiSupportsArchetypes { get; internal set; }
        [BsonField("physical")]
        public string Physical { get; internal set; }
        [BsonField("synchron_material_for")]
        public string SynchroMaterialFor { get; internal set; }
        [BsonField("other_names")]
        public string OtherNames { get; internal set; }
        [BsonField("password")]
        public string Password { get; internal set; }
        [BsonField("witual_monster_required")]
        public string RitualMonsterRequired { get; internal set; }
        [BsonField("ritual_spell_card_requires")]
        public string RitualSpellCardRequired { get; internal set; }
        [BsonField("source_card")]
        public string SourceCard { get; internal set; }
        [BsonField("sommoned_by_the_effect_of")]
        public string SummonedByTheEffectOf { get; internal set; }
    }
}

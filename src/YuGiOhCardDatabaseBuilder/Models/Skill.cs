using LiteDB;
using Newtonsoft.Json;

namespace YuGiOhCardDatabaseBuilder.Models
{
    public class Skill
    {
        public Skill()
        {
            
        }

        public Skill(DuelLinksMeta.Models.Skill skill)
        {
            NameEnglish = skill.Name;
            DescriptionEnglish = skill.Description;
            Exclusive = skill.Exclusive;
            Characters = JsonConvert.SerializeObject(skill.Characters);
        }

        [BsonField("name_english")]
        public string NameEnglish { get; set; }

        [BsonField("name_german")]
        public string NameGerman { get; set; }

        [BsonField("description_english")]
        public string DescriptionEnglish { get; set; }

        [BsonField("description_german")]
        public string DescriptionGerman { get; set; }

        [BsonField("exclusive")]
        public bool Exclusive { get; set; }

        [BsonField("characters")]
        public string Characters { get; set; }
    }
}

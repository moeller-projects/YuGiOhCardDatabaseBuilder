using LiteDB;
using SQLite;

namespace YuGiOhCardDatabaseBuilder.Models
{
    public class BoosterCard
    {
        public BoosterCard(YuGiOhWikiaApi.Models.BoosterCard boosterCard)
        {
            Language = boosterCard.language;
            SetNumber = boosterCard.setnumber;
            Name = boosterCard.name;
            Rarity = boosterCard.rarity;
            Category = boosterCard.category;
        }
        
        [BsonId(true)]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [BsonField("language")]
        public string Language { get; set; }

        [BsonField("setnumber")]
        [Indexed]
        public string SetNumber { get; set; }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("rarity")]
        public string Rarity { get; set; }

        [BsonField("category")]
        public string Category { get; set; }
    }
}

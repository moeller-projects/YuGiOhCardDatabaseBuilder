using LiteDB;
using SQLite;

namespace YuGiOhCardDatabaseBuilder.Models
{
    public class Booster
    {
        public Booster() { }

        public Booster(YuGiOhWikiaApi.Models.Booster booster)
        {
            Name = booster.name;
            ReleaseDateEnglish = booster.enReleaseDate;
            ReleaseDateJapanese = booster.jpReleaseDate;
            ReleaseDateSouthKorea = booster.skReleaseDate;
            ReleaseDateWorldWide = booster.worldwideReleaseDate;
            Imageurl = booster.imgSrc;
            Prefixes = booster.prefixes;
            Prefix = booster.prefix;
        }

        [BsonField("name")]
        public string Name { get; set; }

        [BsonField("release_date_english")]
        public string ReleaseDateEnglish { get; set; }

        [BsonField("release_date_japanese")]
        public string ReleaseDateJapanese { get; set; }

        [BsonField("release_date_south_korea")]
        public string ReleaseDateSouthKorea { get; set; }

        [BsonField("release_date_world_wide")]
        public string ReleaseDateWorldWide { get; set; }

        [BsonField("image_url")]
        public string Imageurl { get; set; }

        [BsonField("prefixes")]
        public string Prefixes { get; set; }

        [BsonField("prefix")]
        public string Prefix { get; set; }
    }
}

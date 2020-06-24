using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YuGiOhDatabaseBuilderV2.Models;

namespace YuGiOhDatabaseBuilderV2.DataStores
{
    [DataStore]
    public class JsonFileDataStore : DataStoreBase
    {
        public JsonFileDataStore() : base(nameof(JsonFileDataStore))
        {
        }

        public override async Task RunAsync(IEnumerable<Card> cards)
        {
            var path = $"C:\\temp\\cards-{DateTime.Now:yyMMdd}.json";
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(cards));
        }
    }
}
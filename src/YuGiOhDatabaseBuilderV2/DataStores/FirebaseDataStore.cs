using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using YuGiOhDatabaseBuilderV2.Extensions;
using YuGiOhDatabaseBuilderV2.Models;

namespace YuGiOhDatabaseBuilderV2.DataStores
{
    [DataStore]
    public class FirebaseDataStore : DataStoreBase
    {
        private readonly FirebaseClient _firebaseClient;
        private const string AuthToken = "fE3BjhuISusvB8YsLhFodNaOVmJ064OEhz5m677q";
        private const string BaseUrl = "https://grossvaters-db.firebaseio.com/";

        public FirebaseDataStore() : base(nameof(FirebaseDataStore))
        {
            _firebaseClient = new FirebaseClient(
                BaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(AuthToken)
                });
        }

        public override async Task RunAsync(IEnumerable<Card> cards)
        {
            var cardBatches = cards.Split(500);

            var batchTasks = cardBatches.Select(batch => Task.Run(async () =>
            {
                foreach (var card in batch)
                {
                    var result = await _firebaseClient
                        .Child("cards")
                        .Child(card.NameEnglish?.RemoveSpecialCharacters() ?? Guid.NewGuid().ToString("N"))
                        .PostAsync(card);
                }
            })).ToArray();

            Task.WaitAll(batchTasks);
        }
    }
}
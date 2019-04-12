using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuGiOhCardDatabaseBuilder.Models;

namespace YuGiOhDatabaseBuilderV2.Modules
{
    public abstract class ModuleBase
    {
        public string ModuleName { get; private set; }
        protected IEnumerable<Card> Cards;

        public ModuleBase(string moduleName)
        {
            ModuleName = moduleName;
            Cards = new List<Card>();
        }

        public async Task<ModuleInfo> RunAsync()
        {
            var cardUrls = await GetCardUrlsAsync();
            var cards = await ParseCardsAsync(cardUrls);
            return new ModuleInfo()
            {
                Cards = cards
            };

        }

        protected abstract Task<IEnumerable<(string name, int pageId)>> GetCardUrlsAsync();
        protected abstract Task<IEnumerable<Card>> ParseCardsAsync(IEnumerable<(string name, int pageId)> cardNames);
    }
}

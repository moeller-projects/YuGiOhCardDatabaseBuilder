using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuGiOhDatabaseBuilderV2.Models;

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
            var cards = await ParseCardsAsync(cardUrls
                //.Where(w => w.Value == "Flash Knight" || w.Value == "Harmonizing Magician").ToDictionary(key => key.Key, value => value.Value)
            );
            return new ModuleInfo()
            {
                Cards = cards
            };

        }

        protected abstract Task<IDictionary<int, string>> GetCardUrlsAsync();
        protected abstract Task<IEnumerable<Card>> ParseCardsAsync(IDictionary<int, string> cardNames);
    }
}

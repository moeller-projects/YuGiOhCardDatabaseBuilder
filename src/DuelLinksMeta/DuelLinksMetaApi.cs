using System.Collections.Generic;
using DuelLinksMeta.Models;

namespace DuelLinksMeta
{
    public class DuelLinksMetaApi
    {
        //public const string CardImageFilterUrl = "https://www.duellinksmeta.com/data/cardImageFilter.json";

        private readonly WebApiClient _api;

        public DuelLinksMetaApi()
        {
            _api = new WebApiClient("https://www.duellinksmeta.com/");
        }

        public IEnumerable<Skill> GetAllSkills()
        {
            return _api.Get<IEnumerable<Skill>>("data/skills.json");
        }

        public IEnumerable<ObtainableCard> GetAllObtainableCards()
        {
            return _api.Get<IEnumerable<ObtainableCard>>("data/cardObtain.json");
        }

        public IEnumerable<DuelLinksCard> GetAllDuelLinksCards()
        {
            return _api.Get<IEnumerable<DuelLinksCard>>("data/cards-dl.json");
        }
    }
}

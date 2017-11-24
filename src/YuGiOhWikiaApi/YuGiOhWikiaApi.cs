using HtmlAgilityPack;

namespace YuGiOhWikiaApi
{
    public class YuGiOhWikiaApi
    {
        private readonly WebApiClient _api;

        public YuGiOhWikiaApi()
        {
            _api = new WebApiClient("http://yugioh.wikia.com/");
        }

        public HtmlDocument GetCardInfo(string cardLink)
        {
            return _api.GetHtmlDocument(cardLink);
        }

        public dynamic GetTcgCardList(string offset)
        {
            var url = "api/v1/Articles/List?category=TCG_cards&limit=5000&namespaces=0";
            if (!string.IsNullOrEmpty(offset))
            {
                url += $"&offset={offset}";
            }
            return _api.Get(url);
        }

        public dynamic GetOcgCardList(string offset)
        {
            var url = "api/v1/Articles/List?category=OCG_cards&limit=5000&namespaces=0";
            if (!string.IsNullOrEmpty(offset))
            {
                url += $"&offset={offset}";
            }
            return _api.Get(url);
        }

        public dynamic GetTcgBoosterList(string offset)
        {
            var url = "api/v1/Articles/List?category=TCG_Booster_Packs&limit=5000&namespaces=0";
            if (!string.IsNullOrEmpty(offset))
            {
                url += $"&offset={offset}";
            }
            return _api.Get(url);
        }

        public dynamic GetOcgBoosterList(string offset)
        {
            var url = "api/v1/Articles/List?category=OCG_Booster_Packs&limit=5000&namespaces=0";
            if (!string.IsNullOrEmpty(offset))
            {
                url += $"&offset={offset}";
            }
            return _api.Get(url);
        }

        public HtmlDocument GetBoosterInfo(string boosterLink)
        {
            return _api.GetHtmlDocument(boosterLink);
        }
    }
}

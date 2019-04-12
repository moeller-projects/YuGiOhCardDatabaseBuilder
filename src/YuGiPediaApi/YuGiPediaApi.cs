using Newtonsoft.Json.Linq;
using System;
using YuGiOhWikiaApi;
using YuGiPediaApi.Models;

namespace YuGiPediaApi
{
    public class YuGiPediaApi
    {
        private readonly WebApiClient _api;
        public string Url;

        public YuGiPediaApi()
        {
            _api = new WebApiClient("https://yugipedia.com/");
            Url = "https://yugipedia.com/";
        }

        public Response GetCard(int pageId)
        {
            //var url = $"api.php?action=parse&format=json&prop=wikitext&formatversion=2&pageid={Uri.EscapeUriString(pageId.ToString())}";
            var url = $"api.php?action=parse&format=json&prop=text&formatversion=2&pageid={Uri.EscapeUriString(pageId.ToString())}";
            return _api.Get<Response>(url);
        }

        public Response GetTcgCardList(string @continue = null)
        {
            var url = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3ATCG_cards&cmlimit=500";
            if (!string.IsNullOrEmpty(@continue))
            {
                url += $"&cmcontinue={Uri.EscapeUriString(@continue)}";
            }
            return _api.Get<Response>(url);
        }

        public Response GetOcgCardList(string @continue = null)
        {
            var url = "api.php?action=query&format=json&list=categorymembers&cmtitle=Category%3AOCG_cards&cmlimit=500";
            if (!string.IsNullOrEmpty(@continue))
            {
                url += $"&cmcontinue={Uri.EscapeUriString(@continue)}";
            }
            return _api.Get<Response>(url);
        }
    }
}

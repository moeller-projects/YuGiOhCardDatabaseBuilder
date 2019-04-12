using MwParserFromScratch;
using System;
using AngleSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuGiOhCardDatabaseBuilder.Models;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
using System.Net;
using AngleSharp.Dom;
using YuGiOhDatabaseBuilderV2.Parser;

namespace YuGiOhDatabaseBuilderV2.Modules
{
    [Module]
    public class YuGiPediaModule : ModuleBase
    {
        private readonly YuGiPediaApi.YuGiPediaApi yugiPediaApi;
        private readonly MediaWikiParser mediaWikiParser;

        public YuGiPediaModule() : base(nameof(YuGiPediaModule))
        {
            yugiPediaApi = new YuGiPediaApi.YuGiPediaApi();
            mediaWikiParser = new MediaWikiParser(new HtmlParser());
        }

        protected override async Task<IEnumerable<(string name, int pageId)>> GetCardUrlsAsync()
        {
            var cardPages = new List<(string name, int pageId)>();
            bool processNextPage;
            string @continue = null;
            do
            {
                processNextPage = false;
                var response = yugiPediaApi.GetTcgCardList(@continue);

                cardPages.AddRange(response.Query.Categorymembers.Select(s => (name: s.Title, pageId: s.Pageid)));

                if (!string.IsNullOrEmpty(response?.Continue?.Cmcontinue ?? null))
                {
                    @continue = response.Continue.Cmcontinue;
                    processNextPage = true;
                }
            } while (processNextPage);

            return await Task.FromResult(cardPages);
        }

        protected override async Task<IEnumerable<Card>> ParseCardsAsync(IEnumerable<(string name, int pageId)> cardLinks)
        {
            var cards = new List<Card>();
            foreach (var cardLink in cardLinks)
            {
                var response = yugiPediaApi.GetCard(cardLink.pageId);
                cards.Add(await ParseCardAsync(response?.Parse?.Pageid, response?.Parse?.Title, response?.Parse?.Text));
            }
            return await Task.FromResult(Cards);
        }

        private async Task<Card> ParseCardAsync(int? pageid, string title, string html)
        {
            return await mediaWikiParser.ParseAsync(html);
        }
    }
}

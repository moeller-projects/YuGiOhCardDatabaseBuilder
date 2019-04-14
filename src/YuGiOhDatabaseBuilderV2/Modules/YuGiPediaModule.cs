using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuGiOhCardDatabaseBuilder.Models;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using YuGiOhDatabaseBuilderV2.Parser;
using YuGiOhDatabaseBuilderV2.Extensions;
using YuGiOhDatabaseBuilderV2.Reporter;
using YuGiOhDatabaseBuilderV2.Models;
using JsonFlatFileDataStore;

namespace YuGiOhDatabaseBuilderV2.Modules
{
    [Module]
    public class YuGiPediaModule : ModuleBase
    {
        private readonly YuGiPediaApi.YuGiPediaApi yugiPediaApi;
        private readonly MediaWikiParser mediaWikiParser;
        private readonly MissingFieldReporter missingFieldReporter;

        public YuGiPediaModule() : base(nameof(YuGiPediaModule))
        {
            yugiPediaApi = new YuGiPediaApi.YuGiPediaApi();
            missingFieldReporter = new MissingFieldReporter();
            mediaWikiParser = new MediaWikiParser(new HtmlParser(), missingFieldReporter);
        }

        protected override async Task<IDictionary<int, string>> GetCardUrlsAsync()
        {
            var cards = new Dictionary<int, string>();
            await ProcessTcgCardsAsync(ref cards);
            await ProcessOcgCardsAsync(ref cards);

            return await Task.FromResult(cards);
        }

        private Task ProcessTcgCardsAsync(ref Dictionary<int, string> cards)
        {
            bool processNextPage;
            string @continue = null;
            do
            {
                processNextPage = false;
                var response = yugiPediaApi.GetTcgCardList(@continue);

                foreach (var card in response?.Query?.Categorymembers)
                {
                    if (!cards.ContainsKey(card.Pageid))
                        cards.Add(card.Pageid, card.Title);
                }

                if (!string.IsNullOrEmpty(response?.Continue?.Cmcontinue ?? null))
                {
                    @continue = response.Continue.Cmcontinue;
                    processNextPage = true;
                }
            } while (processNextPage);

            return Task.CompletedTask;
        }

        private Task ProcessOcgCardsAsync(ref Dictionary<int, string> cards)
        {
            bool processNextPage;
            string @continue = null;
            do
            {
                processNextPage = false;
                var response = yugiPediaApi.GetOcgCardList(@continue);

                foreach (var card in response?.Query?.Categorymembers)
                {
                    if (!cards.Contains(KeyValuePair.Create(card.Pageid, card.Title)))
                        cards.Add(card.Pageid, card.Title);
                }

                if (!string.IsNullOrEmpty(response?.Continue?.Cmcontinue ?? null))
                {
                    @continue = response.Continue.Cmcontinue;
                    processNextPage = true;
                }
            } while (processNextPage);

            return Task.CompletedTask;
        }

        protected override async Task<IEnumerable<Card>> ParseCardsAsync(IDictionary<int, string> cardLinks)
        {
            var cards = new CardList();

            var cardsToDownload = cardLinks
#if DEBUG
                //.Take(100)
#endif
                .Split();

            Console.Write(string.Empty);
            var tasks = cardsToDownload
                .Select(s => Task.Run(async () =>
                {
                    foreach (var cardLink in s)
                    {
                        var response = yugiPediaApi.GetCard(cardLink.Key);
                        cards.Add(await ParseCardAsync(response?.Parse?.Pageid, response?.Parse?.Title, response?.Parse?.Text));
                        Console.WriteLine($"Processed Cards {cards.Count} / {cardLinks.Count}");
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);

            Console.WriteLine();
            missingFieldReporter.OnCompleted();

            return await Task.FromResult(cards);
        }

        private async Task<Card> ParseCardAsync(int? pageid, string title, string html)
        {
            return await mediaWikiParser.ParseAsync(html);
        }
    }
}

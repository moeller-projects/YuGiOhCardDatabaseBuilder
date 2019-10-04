using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using YuGiOhDatabaseBuilderV2.Extensions;
using YuGiOhDatabaseBuilderV2.Models;
using YuGiOhDatabaseBuilderV2.Parser;
using YuGiOhDatabaseBuilderV2.Reporter;

namespace YuGiOhDatabaseBuilderV2.Modules
{
    [Module]
    public class YuGiPediaModule : ModuleBase
    {
        private readonly YuGiPediaApi.YuGiPediaApi _yugiPediaApi;
        private readonly MediaWikiParser _mediaWikiParser;
        private readonly MissingFieldReporter _missingFieldReporter;

        public YuGiPediaModule() : base(nameof(YuGiPediaModule))
        {
            _yugiPediaApi = new YuGiPediaApi.YuGiPediaApi();
            _missingFieldReporter = new MissingFieldReporter();
            _mediaWikiParser = new MediaWikiParser(new HtmlParser(), _missingFieldReporter);
        }

        protected override async Task<IDictionary<int, string>> GetCardUrlsAsync()
        {
            var cards = new Dictionary<int, string>();
            await ProcessAllCardsAsync(ref cards);
            await ProcessTcgCardsAsync(ref cards);
            await ProcessOcgCardsAsync(ref cards);

            return await Task.FromResult(cards.Where(w => !w.Value.EndsWith(")")).ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        private Task ProcessAllCardsAsync(ref Dictionary<int, string> cards)
        {
            bool processNextPage;
            string @continue = null;
            do
            {
                processNextPage = false;
                var response = _yugiPediaApi.GetAllCardsList(@continue);

                if (response?.Query?.Categorymembers != null && response.Query.Categorymembers.Any())
                {
                    foreach (var card in response?.Query?.Categorymembers)
                    {
                        if (!cards.ContainsKey(card.Pageid))
                            cards.Add(card.Pageid, card.Title);
                    }
                }

                if (string.IsNullOrEmpty(response?.Continue?.Cmcontinue)) continue;

                @continue = response.Continue.Cmcontinue;
                processNextPage = true;
            } while (processNextPage);

            return Task.CompletedTask;
        }

        private Task ProcessTcgCardsAsync(ref Dictionary<int, string> cards)
        {
            bool processNextPage;
            string @continue = null;
            do
            {
                processNextPage = false;
                var response = _yugiPediaApi.GetTcgCardList(@continue);

                if (response?.Query?.Categorymembers != null && response.Query.Categorymembers.Any())
                {
                    foreach (var card in response?.Query?.Categorymembers)
                    {
                        if (!cards.ContainsKey(card.Pageid))
                            cards.Add(card.Pageid, card.Title);
                    }
                }

                if (string.IsNullOrEmpty(response?.Continue?.Cmcontinue)) continue;

                @continue = response.Continue.Cmcontinue;
                processNextPage = true;
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
                var response = _yugiPediaApi.GetOcgCardList(@continue);

                if (response?.Query?.Categorymembers != null && response.Query.Categorymembers.Any())
                {
                    foreach (var card in response?.Query?.Categorymembers)
                    {
                        if (!cards.Contains(KeyValuePair.Create(card.Pageid, card.Title)))
                            cards.Add(card.Pageid, card.Title);
                    }
                }

                if (string.IsNullOrEmpty(response?.Continue?.Cmcontinue)) continue;

                @continue = response.Continue.Cmcontinue;
                processNextPage = true;
            } while (processNextPage);

            return Task.CompletedTask;
        }

        protected override async Task<IEnumerable<Card>> ParseCardsAsync(IDictionary<int, string> cardLinks)
        {
            var cards = new List<Card>();

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
                        var response = _yugiPediaApi.GetCard(cardLink.Key);
                        cards.Add(await ParseCardAsync(response?.Parse?.Pageid, response?.Parse?.Title, response?.Parse?.Text));
                        Console.WriteLine($"Processed Cards {cards.Count} / {cardLinks.Count}");
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);

            Console.WriteLine();
            _missingFieldReporter.OnCompleted();

            return await Task.FromResult(cards);
        }

        private async Task<Card> ParseCardAsync(int? pageid, string title, string html)
        {
            return await _mediaWikiParser.ParseAsync(html);
        }
    }
}

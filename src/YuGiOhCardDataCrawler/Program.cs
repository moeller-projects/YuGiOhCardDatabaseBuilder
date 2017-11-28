using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LiteDB;
using Newtonsoft.Json.Linq;
using NLog;
using YuGiOhWikiaApi.Models;
using Logger = NLog.Logger;

namespace YuGiOhCardDataCrawler
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static YuGiOhWikiaApi.YuGiOhWikiaApi _api;
        public static List<dynamic> CardList = new List<dynamic>();
        public static List<dynamic> BoosterList = new List<dynamic>();
        public static List<Models.Card> Cards = new List<Models.Card>();
        public static List<Models.Booster> Boosters = new List<Models.Booster>();

        public static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _api = new YuGiOhWikiaApi.YuGiOhWikiaApi();
            Logger.Info("initializing card list");
            InitializeCardLists();
            Logger.Info("initializing booster list");
            InitializeBoosterList();

            Task.WaitAll(
                Task.Factory.StartNew(ProcessCards),
                Task.Factory.StartNew(ProcessBoosters));

            Logger.Info("writing cards into database");
            WriteCardsToDatabase();
            Logger.Info("writing booster into database");
            WriteBoostersToDatabase();

            stopwatch.Stop();
            Logger.Info("-- FINISHED --");
            Logger.Info($"time elapsed: {stopwatch.Elapsed.TotalMinutes} minutes");
            Console.ReadLine();
        }

        private static void WriteBoostersToDatabase()
        {
            using (var db = new LiteDatabase(@"Filename=ygo.db; Password=ygo"))
            {
                var col = db.GetCollection<Models.Booster>("boosters");
                col.Upsert(Boosters.Distinct());
                col.EnsureIndex(i => i.Id, true);
                db.Shrink();
                db.Dispose();
            }
        }

        private static void WriteCardsToDatabase()
        {
            using (var db = new LiteDatabase(@"Filename=ygo.db; Password=ygo"))
            {
                var col = db.GetCollection<Models.Card>("cards");
                col.Upsert(Cards.Distinct());
                col.EnsureIndex(i => i.Id, true);
                db.Shrink();
                db.Dispose();
            }
        }

        private static void ProcessBoosters()
        {
            Logger.Info($"processing {BoosterList.Count} boosters");

            var boosterlists = SplitList(BoosterList.Distinct().ToList());

            var tasks = boosterlists
                .Select(boosters => Task.Factory.StartNew(() => ProcessBoosters(boosters)))
                .ToArray();

            Logger.Info($"waiting for {tasks.Length} booster processing tasks");

            Task.WaitAll(tasks);

            Logger.Info("finished processing boosters");
        }

        private static void ProcessBoosters(IEnumerable<dynamic> boosters)
        {
            foreach (var boosterInfo in boosters)
            {
                HtmlDocument booster = _api.GetBoosterInfo(boosterInfo.url.ToString());
                Booster result = BoosterParser.ForHtmlDocument(boosterInfo, booster);
                Boosters.Add(new Models.Booster(result));
                UpdateProgress();
            }
        }

        private static void UpdateProgress()
        {
            var cards = (decimal)Cards.Count / CardList.Count;
            var boosters = (decimal)Boosters.Count / BoosterList.Count;
            Console.Title = $"Cards: {cards:P2} | Boosters: {boosters:P2}";
        }

        private static void ProcessCards()
        {
            Logger.Info($"processing {CardList.Count} cards");

            var cardlists = SplitList(CardList.Distinct().ToList(), 1000);

            var tasks = cardlists
                .Select(cards => Task.Factory.StartNew(() => ProcessCards(cards)))
                .ToArray();

            Logger.Info($"waiting for {tasks.Length} card processing tasks");

            Task.WaitAll(tasks);

            Logger.Info("finished processing cards");
        }

        private static void ProcessCards(IEnumerable<dynamic> cards)
        {
            foreach (var cardInfo in cards)
            {
                HtmlDocument card = _api.GetCardInfo(cardInfo.url.ToString());
                Card result = CardParser.ForHtmlDocument(cardInfo, card);
                Cards.Add(new Models.Card(result));
                UpdateProgress();
            }
        }

        private static void InitializeCardLists()
        {
            AddTcgCardsToList(null);
            AddOcgCardsToList(null);
        }

        private static void InitializeBoosterList()
        {
            AddTcgBoosterToList(null);
            AddOcgBoosterToList(null);
        }

        private static void AddTcgBoosterToList(string offset)
        {
            while (true)
            {
                var response = _api.GetTcgBoosterList(offset);
                foreach (var boosterToken in JArray.Parse(response.items.ToString()))
                {
                    var booster = boosterToken.ToObject<dynamic>();
                    BoosterList.Add(booster);
                }

                if (!string.IsNullOrEmpty((string)response.offset))
                {
                    offset = (string)response.offset;
                    continue;
                }
                break;
            }
        }

        private static void AddOcgBoosterToList(string offset)
        {
            while (true)
            {
                var response = _api.GetOcgBoosterList(offset);
                foreach (var boosterToken in JArray.Parse(response.items.ToString()))
                {
                    var booster = boosterToken.ToObject<dynamic>();
                    BoosterList.Add(booster);
                }

                if (!string.IsNullOrEmpty((string)response.offset))
                {
                    offset = (string)response.offset;
                    continue;
                }
                break;
            }
        }

        private static void AddOcgCardsToList(string offset)
        {
            while (true)
            {
                var response = _api.GetOcgCardList(offset);
                foreach (var cardToken in JArray.Parse(response.items.ToString()))
                {
                    var card = cardToken.ToObject<dynamic>();
                    CardList.Add(card);
                }

                if (!string.IsNullOrEmpty((string)response.offset))
                {
                    offset = (string)response.offset;
                    continue;
                }
                break;
            }
        }

        private static void AddTcgCardsToList(string offset)
        {
            while (true)
            {
                var response = _api.GetTcgCardList(offset);
                foreach (var cardToken in JArray.Parse(response.items.ToString()))
                {
                    var card = cardToken.ToObject<dynamic>();
                    CardList.Add(card);
                }
                if (!string.IsNullOrEmpty((string)response.offset))
                {
                    offset = (string)response.offset;
                    continue;
                }
                break;
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> list, int nSize = 100)
        {
            for (var i = 0; i < list.Count; i += nSize)
            {
                yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using DuelLinksMeta;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;
using SQLite;
using YuGiOhCardDatabaseBuilder.Models;
using Booster = YuGiOhWikiaApi.Models.Booster;
using Card = YuGiOhWikiaApi.Models.Card;
using Logger = NLog.Logger;

namespace YuGiOhCardDatabaseBuilder
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static YuGiOhWikiaApi.YuGiOhWikiaApi _api;
        private static DuelLinksMetaApi _duelLinksMetaApi;
        public static List<dynamic> CardList = new List<dynamic>();
        public static List<dynamic> BoosterList = new List<dynamic>();
        public static List<DuelLinksMeta.Models.Skill> Skills = new List<DuelLinksMeta.Models.Skill>();
        public static List<Models.Card> Cards = new List<Models.Card>();
        public static List<Models.Booster> Boosters = new List<Models.Booster>();
        public static List<Models.BoosterCard> BoosterCards = new List<Models.BoosterCard>();

        public static void Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();
            try
            {
                Parser.Default.ParseArguments<BuildArguments>(args)
                    .WithParsed(Build)
                    .WithNotParsed(Errors);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                timer.Stop();
                Logger.Info($"Time elapsed: {timer.Elapsed.ToOutput()}\n");
                Environment.Exit(1);
            }
            timer.Stop();
            Logger.Info("-- FINISHED --");
            Logger.Info($"Time elapsed: {timer.Elapsed.ToOutput()}\n");

#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void Errors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Logger.Error(error);
            }
            Environment.Exit(1);
        }

        public static void Build(BuildArguments arguments)
        {
            _api = new YuGiOhWikiaApi.YuGiOhWikiaApi();
            _duelLinksMetaApi = new DuelLinksMetaApi();

            Logger.Info("initializing skill list");
            InitializeSkillList();
            Logger.Info("writing skills into database");
            WriteSkillsToDatabase(
                Path.Combine(arguments.DatabasePath, arguments.DatabaseName));

            Logger.Info("initializing card list");
            InitializeCardLists();
            Logger.Info("initializing booster list");
            InitializeBoosterList();

            Task.WaitAll(
                Task.Factory.StartNew(ProcessCards),
                Task.Factory.StartNew(ProcessBoosters));

            Logger.Info("writing cards into database");
            WriteCardsToDatabase(
                Path.Combine(arguments.DatabasePath, arguments.DatabaseName));
            Logger.Info("writing booster into database");
            WriteBoostersToDatabase(
                Path.Combine(arguments.DatabasePath, arguments.DatabaseName));
            Logger.Info("writing boostercards into database");
            WriteBoosterCardsToDatabase(
                Path.Combine(arguments.DatabasePath, arguments.DatabaseName));
        }

        private static void InitializeSkillList()
        {
            Skills.AddRange(_duelLinksMetaApi.GetAllSkills());
        }

        private static void WriteSkillsToDatabase(string sqliteDbConnectionString)
        {
            using (var database = new SQLiteConnection(sqliteDbConnectionString))
            {
                database.CreateTable<Models.Skill>();
                database.InsertAll(Skills.Select(s => new Models.Skill(s)));
            }
        }

        private static void WriteBoosterCardsToDatabase(string sqliteDbConnectionString)
        {
            using (var database = new SQLiteConnection(sqliteDbConnectionString))
            {
                database.CreateTable<Models.BoosterCard>(CreateFlags.FullTextSearch4);
                database.InsertAll(BoosterCards.Distinct());
            }
        }

        private static void WriteBoostersToDatabase(string sqliteDbConnectionString)
        {
            using (var database = new SQLiteConnection(sqliteDbConnectionString))
            {
                database.CreateTable<Models.Booster>(CreateFlags.FullTextSearch4);
                database.InsertAll(Boosters.Distinct());
            }
        }

        private static void WriteCardsToDatabase(string sqliteDbConnectionString)
        {
            using (var database = new SQLiteConnection(sqliteDbConnectionString))
            {
                database.CreateTable<Models.Card>(CreateFlags.FullTextSearch4);
                database.InsertAll(Cards.Distinct());
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
                BoosterCards.AddRange(result.cardList.Select(s => new Models.BoosterCard(s)));
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

            var cardlists = SplitList(CardList.Distinct()
#if DEBUG
                .Take(100)
#endif
                .ToList(), 1000);

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
            //AddOcgCardsToList(null);
        }

        private static void InitializeBoosterList()
        {
            AddTcgBoosterToList(null);
            //AddOcgBoosterToList(null);
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

using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YuGiOhCardDatabaseBuilder.Models;
using YuGiOhDatabaseBuilderV2.Reporter;

namespace YuGiOhDatabaseBuilderV2.Parser
{
    public class MediaWikiParser : IParser<Card>
    {
        private readonly IHtmlParser htmlParser;
        private readonly MissingFieldReporter missingFieldReporter;

        public MediaWikiParser(IHtmlParser htmlParser, MissingFieldReporter missingFieldReporter)
        {
            this.htmlParser = htmlParser;
            this.missingFieldReporter = missingFieldReporter;
        }

        public async Task<Card> ParseAsync(string html)
        {
            var card = new Card();

            try
            {
                var dom = (await htmlParser.ParseDocumentAsync(html))
                .GetElementsByClassName("mw-parser-output")
                .FirstOrDefault();

                await ParseBasics(dom, ref card);
                await ParseCardTable(dom, ref card);
                await ParseHList(dom, ref card);
                await ParseWikitable(dom, ref card);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {

            }
            return card;
        }

        private Task ParseHList(IElement dom, ref Card card)
        {
            var hlists = dom.GetElementsByClassName("hlist")
                .Where(w => w.GetElementsByTagName("dt").Any() && w.GetElementsByTagName("dd").Any());

            foreach (var hlist in hlists)
            {
                var header = hlist.GetElementsByTagName("dt").FirstOrDefault()?.TextContent.Replace("\n", " ").Trim();
                var value = string.Join("|", hlist.GetElementsByTagName("dd").Select(s => s.TextContent.Trim()));

                switch (header?.ToLower())
                {
                    case "supports":
                        card.Supports = value;
                        break;
                    case "anti-supports":
                        card.AntiSupports = value;
                        break;
                    case "anti-supports archetypes":
                        card.AntiSupportsArchetypes = value;
                        break;
                    case "archetypes and series":
                        card.ArchetypesAndSeries = value;
                        break;
                    case "supports archetypes":
                        card.SupportsArchetypes = value;
                        break;
                    case "related to archetypes and series":
                        card.RelatedToArchetypeAndSeries = value;
                        break;
                    case "monster/spell/trap categories":
                        card.CardCategories = value;
                        break;
                    case "summoning categories":
                        card.SummoningCategories = value;
                        break;
                    case "miscellaneous":
                        card.Miscellaneous = value;
                        break;
                    case "counters":
                        card.Counters = value;
                        break;
                    case "banished categories":
                        card.BanishedCategories = value;
                        break;
                    case "actions":
                        card.Actions = value;
                        break;
                    case "attack categories":
                        card.AttackCategories = value;
                        break;
                    case "fusion material for":
                        card.FusionMaterialFor = value;
                        break;
                    case "lp":
                        card.LpCategories = value;
                        break;
                    case "stat changes":
                        card.StatChanges = value;
                        break;
                    case "physical":
                        card.Physical = value;
                        break;
                    case "synchro material for":
                        card.SynchroMaterialFor = value;
                        break;
                    case null:
                    case "":
                        break;
                    default:
                        missingFieldReporter.OnNext(new Models.MissingField("Hlist", header));
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ParseWikitable(IElement dom, ref Card card)
        {
            var otherLanguages = dom.GetElementsByTagName("h2")
                .FirstOrDefault(f => f.TextContent.ToLower().Trim() == "other languages");

            var wikitable = otherLanguages?.NextElementSibling;
            var rows = wikitable?.GetElementsByTagName("tr");

            if (rows == null)
                return Task.CompletedTask;

            foreach (var row in rows)
            {
                var language = row.GetElementsByTagName("th").FirstOrDefault()?.TextContent.Replace("\n", " ").Trim();
                var name = row.GetElementsByTagName("td").FirstOrDefault()?.TextContent.Trim();
                //var description = row.GetElementsByTagName("td").Skip(1).FirstOrDefault()?.TextContent.Trim();

                var descriptionFormatted = Regex.Replace(row.GetElementsByTagName("td").Skip(1).FirstOrDefault()?.InnerHtml.Replace("<br>", Environment.NewLine) ?? string.Empty, "<[^>]*>", "").Trim();
                var description = WebUtility.HtmlDecode(descriptionFormatted);

                switch (language?.ToLower())
                {
                    case "french":
                        card.NameFrench = name;
                        card.DescriptionFrensh = description;
                        break;
                    case "german":
                        card.NameGerman = name;
                        card.DescriptionGerman = description;
                        break;
                    case "italian":
                        card.NameItalian = name;
                        card.DescriptionItalian = description;
                        break;
                    case "portuguese":
                        card.NamePortuguese = name;
                        card.DescriptionPortuguese = description;
                        break;
                    case "spanish":
                        card.NameSpanish = name;
                        card.DescriptionSpanish = description;
                        break;
                    case "":
                    case null:
                        break;
                    default:
                        missingFieldReporter.OnNext(new Models.MissingField("Wikitable", language));
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ParseCardTable(IElement dom, ref Card card)
        {
            var cardTable = dom.GetElementsByClassName("cardtable").FirstOrDefault()?.FirstElementChild ?? throw new NullReferenceException("cardTable");
            var tableRows = cardTable.GetElementsByClassName("cardtablerow");

            foreach (var row in tableRows)
            {

                if (row.FirstElementChild.ClassName == "cardtablespanrow")
                {
                    if (string.IsNullOrEmpty(card.PendulumScale) && row.FirstElementChild.FirstElementChild.TagName == "P")
                    {
                        var descriptionUnformatted = row.FirstElementChild;
                        var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", Environment.NewLine), "<[^>]*>", "").Trim();
                        card.DescriptionEnglish = WebUtility.HtmlDecode(descriptionFormatted);
                    }
                    else if (!string.IsNullOrEmpty(card.PendulumScale) && row.TextContent.Contains("effect", StringComparison.InvariantCultureIgnoreCase))
                        card.DescriptionEnglish = row.TextContent.Replace("\n", " ").Trim();
                }
                else
                {
                    var header = row.GetElementsByClassName("cardtablerowheader").FirstOrDefault()?.TextContent;
                    var data = row.GetElementsByClassName("cardtablerowdata").FirstOrDefault()?.TextContent?.Replace("\n", " ").Trim();

                    switch (header?.ToLower())
                    {

                        case "card type":
                            card.CardType = data;
                            break;
                        case "card effect types":
                            card.EffectTypes = data;
                            break;
                        case "attribute":
                            card.Attribute = data;
                            break;
                        case "types":
                        case "type":
                            card.MonsterTypes = string.Join('|', data.Split(" / ", StringSplitOptions.RemoveEmptyEntries));
                            break;
                        case "level":
                            card.Level = data;
                            break;
                        case "rank":
                            card.Rank = data;
                            break;
                        case "pendulum scale":
                            card.PendulumScale = data;
                            break;
                        case "materials":
                            card.Materials = data;
                            break;
                        case "fusion material":
                            card.FusionMaterials = data;
                            break;
                        case "atk / def":
                            var array = data.Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
                            card.Attack = array[0];
                            card.Defense = array[1];
                            break;
                        case "atk / link":
                            array = data.Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);

                            if (array.Length == 2)
                            {

                                card.Attack = array[0];
                                card.Link = array[1];

                            }
                            else
                                card.Level = array[0];

                            break;
                        case "property":
                            card.Property = data;
                            break;
                        case "link arrows":
                            card.LinkMarkers = data.Replace(" , ", "|");
                            break;
                        case "passcode":
                            card.Passcode = data.TrimStart('0');
                            break;
                        case "limitation text":
                            card.LimitText = data;
                            break;
                        case "other names":
                            card.OtherNames = data;
                            break;
                        case "ritual monster required":
                            card.RitualMonsterRequired = data;
                            break;
                        case "ritual spell card required":
                            card.RitualSpellCardRequired = data;
                            break;
                        case "source card":
                            card.SourceCard = data;
                            break;
                        case "summoned by the effect of":
                            card.SummonedByTheEffectOf = data;
                            break;
                        case "synchro material":
                            card.SynchroMaterial = data;
                            break;
                        case "statuses":
                        case null:
                        case "":
                            break;
                        default:
                            missingFieldReporter.OnNext(new Models.MissingField("Cardtable", header));
                            break;

                    }
                }
            }

            return Task.CompletedTask;
        }

        private Task ParseBasics(IElement dom, ref Card card)
        {
            card.NameEnglish = dom.GetElementsByClassName("cardtable-header").FirstOrDefault()?.TextContent.Trim();
            card.ImageUrl = dom.GetElementsByClassName("cardtable-cardimage").FirstOrDefault()?
                .GetElementsByTagName("img").First().GetAttribute("srcset")?.Split(' ').First()
                ?? dom.GetElementsByClassName("cardtable-cardimage").FirstOrDefault()?.GetElementsByTagName("img").First().GetAttribute("src");

            return Task.CompletedTask;
        }
    }
}

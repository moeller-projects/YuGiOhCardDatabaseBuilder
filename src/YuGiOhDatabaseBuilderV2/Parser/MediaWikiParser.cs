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
                var value = string.Join("/", hlist.GetElementsByTagName("dd").Select(s => s.TextContent.Trim()));

                switch (header)
                {
                    case "Supports":
                        card.Supports = value;
                        break;
                    case "Anti-supports":
                        card.AntiSupports = value;
                        break;
                    case "Anti-supports archetypes":
                        card.AntiSupportsArchetypes = value;
                        break;
                    case "Archetypes and series":
                        card.ArchetypesAndSeries = value;
                        break;
                    case "Supports archetypes":
                        card.SupportsArchetypes = value;
                        break;
                    case "Related to archetypes and series":
                        card.RelatedToArchetypeAndSeries = value;
                        break;
                    case "Monster/Spell/Trap categories":
                        card.CardCategories = value;
                        break;
                    case "Summoning categories":
                        card.SummoningCategories = value;
                        break;
                    case "Miscellaneous":
                        card.Miscellaneous = value;
                        break;
                    case "Counters":
                        card.Counters = value;
                        break;
                    case "Banished categories":
                        card.BanishedCategories = value;
                        break;
                    case "Actions":
                        card.Actions = value;
                        break;
                    case "Attack categories":
                        card.AttackCategories = value;
                        break;
                    case "Fusion Material for":
                        card.FusionMaterialFor = value;
                        break;
                    case "LP":
                        card.LpCategories = value;
                        break;
                    case "Stat changes":
                        card.StatChanges = value;
                        break;
                    case "Physical":
                        card.Physical = value;
                        break;
                    case "Synchro Material for":
                        card.SynchroMaterialFor = value;
                        break;
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

                switch (language)
                {
                    case "French":
                        card.NameFrench = name;
                        card.DescriptionFrensh = description;
                        break;
                    case "German":
                        card.NameGerman = name;
                        card.DescriptionGerman = description;
                        break;
                    case "Italian":
                        card.NameItalian = name;
                        card.DescriptionItalian = description;
                        break;
                    case "Portuguese":
                        card.NamePortuguese = name;
                        card.DescriptionPortuguese = description;
                        break;
                    case "Spanish":
                        card.NameSpanish = name;
                        card.DescriptionSpanish = description;
                        break;
                    case "":
                        break;
                    default:
                        missingFieldReporter.OnNext(new Models.MissingField("Wikirable", language));
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

                    switch (header)
                    {

                        case "Card type":
                            card.CardType = data;
                            break;
                        case "Card effect types":
                            card.EffectTypes = data;
                            break;
                        case "Attribute":
                            card.Attribute = data;
                            break;
                        case "Types":
                        case "Type":
                            card.MonsterTypes = data;
                            break;
                        case "Level":
                            card.Level = data;
                            break;
                        case "Rank":
                            card.Rank = data;
                            break;
                        case "Pendulum Scale":
                            card.PendulumScale = data;
                            break;
                        case "Materials":
                            card.Materials = data;
                            break;
                        case "Fusion Material":
                            card.FusionMaterials = data;
                            break;
                        case "ATK / DEF":
                            var array = data.Split(new string[] { " / " }, StringSplitOptions.None);
                            card.Attack = array[0];
                            card.Defense = array[1];
                            break;
                        case "ATK / LINK":
                            array = data.Split(new string[] { " / " }, StringSplitOptions.None);

                            if (array.Length == 2)
                            {

                                card.Attack = array[0];
                                card.Link = array[1];

                            }
                            else
                                card.Level = array[0];

                            break;
                        case "Property":
                            card.Property = data;
                            break;
                        case "Link Arrows":
                            card.LinkMarkers = data.Replace(" , ", ", ");
                            break;
                        case "Passcode":
                            card.Passcode = data.TrimStart('0');
                            break;
                        case "Limitation text":
                            card.LimitText = data;
                            break;
                        case "Other names":
                            card.OtherNames = data;
                            break;
                        case "Password":
                            card.Password = data;
                            break;
                        case "Ritual Monster required":
                            card.RitualMonsterRequired = data;
                            break;
                        case "Ritual Spell Card required":
                            card.RitualSpellCardRequired = data;
                            break;
                        case "Source card":
                            card.SourceCard = data;
                            break;
                        case "Summoned by the effect of":
                            card.SummonedByTheEffectOf = data;
                            break;
                        case "Synchro Material":
                            card.SynchroMaterial = data;
                            break;
                        case "Statuses":
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

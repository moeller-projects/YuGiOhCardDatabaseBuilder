using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuGiOhCardDatabaseBuilder.Models;

namespace YuGiOhDatabaseBuilderV2.Parser
{
    public class MediaWikiParser : IParser<Card>
    {
        private readonly IHtmlParser htmlParser;

        public MediaWikiParser(IHtmlParser htmlParser)
        {
            this.htmlParser = htmlParser;
        }

        public async Task<Card> ParseAsync(string html)
        {
            var card = new Card();

            var dom = (await htmlParser.ParseDocumentAsync(html))
                .GetElementsByClassName("mw-parser-output")
                .FirstOrDefault();

            await ParseBasics(dom, ref card);
            await ParseCardTable(dom, ref card);
            await ParseHList(dom, ref card);
            await ParseWikitable(dom, ref card);

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
                    case "Archetypes and series":
                        card.ArchetypesAndSeries = value;
                        break;
                    case "Supports archetypes":
                        card.SupportsArchetypes = value;
                        break;
                    case "Related to archetypes and series":
                        card.RelatedToArchetypeAndSeries = value;
                        break;
                    case "Monster / Spell / Trap categories":
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
                    default:
                        Console.WriteLine($"Missing Hlist Categorie \"{header}\": \"{value}\"");
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ParseWikitable(IElement dom, ref Card card)
        {
            var otherLanguages = dom.GetElementsByTagName("h2")
                .FirstOrDefault(f => f.TextContent.ToLower().Trim() == "other languages");

            var t = otherLanguages.NextElementSibling;
            var rows = t.GetElementsByTagName("tr");

            foreach (var row in rows)
            {
                var language = row.GetElementsByTagName("th").FirstOrDefault()?.TextContent.Replace("\n", " ").Trim();
                var name = row.GetElementsByTagName("td").FirstOrDefault()?.TextContent.Trim();
                var description = row.GetElementsByTagName("td").Skip(1).FirstOrDefault()?.TextContent.Trim();

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
                    default:
                        Console.WriteLine($"Missing Wikitable Language {language}");
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
                        case "Statuses":
                        default:
                            Console.WriteLine($"Missing CardTable Categorie {header}");
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

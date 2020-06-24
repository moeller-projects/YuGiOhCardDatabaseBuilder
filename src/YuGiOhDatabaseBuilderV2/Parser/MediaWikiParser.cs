using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YuGiOhDatabaseBuilderV2.Extensions;
using YuGiOhDatabaseBuilderV2.Models;
using YuGiOhDatabaseBuilderV2.Reporter;

namespace YuGiOhDatabaseBuilderV2.Parser
{
    public class MediaWikiParser : IParser<Card>
    {
        private readonly IHtmlParser _htmlParser;
        private readonly MissingFieldReporter _missingFieldReporter;

        public MediaWikiParser(IHtmlParser htmlParser, MissingFieldReporter missingFieldReporter)
        {
            _htmlParser = htmlParser;
            _missingFieldReporter = missingFieldReporter;
        }

        public async Task<Card> ParseAsync(string html)
        {
            var card = new Card();

            try
            {
                var dom = (await _htmlParser.ParseDocumentAsync(html))
                .GetElementsByClassName("mw-parser-output")
                .FirstOrDefault();

                await ParseBasicsAsync(dom, ref card);
                await ParseCardTableAsync(dom, ref card);
                await ParseHListAsync(dom, ref card);
                await ParseWikitableAsync(dom, ref card);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }

            return card;
        }

        private Task ParseHListAsync(IElement dom, ref Card card)
        {
            var hlists = dom.GetElementsByClassName("hlist")
                .Where(w => w.GetElementsByTagName("dt").Any() && w.GetElementsByTagName("dd").Any());

            foreach (var hlist in hlists)
            {
                var header = hlist.GetElementsByTagName("dt").FirstOrDefault()?.TextContent.Replace("\n", " ").Trim();
                var value = string.Join("|", hlist.GetElementsByTagName("dd").Select(s => s.TextContent.Trim()));
                AddInformationToCard(ref card, header, value);
            }

            return Task.CompletedTask;
        }

        private static void AddInformationToCard(ref Card card, string header, string value)
        {
            switch (header?.ToLower())
            {
                case "card type":
                    card.CardType = value;
                    break;
                case "card effect types":
                case "effect types":
                case "effect type":
                case "effect type(s)":
                    card.EffectTypes = value;
                    break;
                case "attribute":
                    card.Attribute = value;
                    break;
                case "types":
                case "type":
                    card.MonsterTypes = string.Join('|', value.Split(" / ", StringSplitOptions.RemoveEmptyEntries));
                    break;
                case "level":
                    card.Level = value;
                    break;
                case "rank":
                    card.Rank = value;
                    break;
                case "atk/def":
                case "atk / def":
                    var array = value.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    card.Attack = array[0];
                    card.Defense = array[1];
                    break;
                case "atk / link":
                    array = value.Split("/", StringSplitOptions.RemoveEmptyEntries);

                    if (array.Length == 2)
                    {

                        card.Attack = array[0];
                        card.Link = array[1];

                    }
                    else
                        card.Level = array[0];

                    break;
                case "property":
                    card.Property = value;
                    break;
                case "link arrows":
                    card.LinkMarkers = value.Replace(" , ", "|");
                    break;
                case "passcode":
                    card.Passcode = value.TrimStart('0');
                    break;
                case "archetype":
                case "archetypes":
                case "archetype(s)":
                case "archetypes and series":
                    card.Archetypes = value;
                    break;
                case null:
                case "":
                    break;
                default:
                    card.AdditionalInformation.Add(header?.Trim().Replace(Environment.NewLine, string.Empty).RemoveSpecialCharacters(), value);
                    break;
            }
        }

        private Task ParseWikitableAsync(IElement dom, ref Card card)
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

                //var descriptionFormatted = Regex.Replace(row.GetElementsByTagName("td").Skip(1).FirstOrDefault()?.InnerHtml.Replace("<br>", Environment.NewLine) ?? string.Empty, "<[^>]*>", "").Trim();
                var descriptionFormatted = Regex.Replace(string.Join(Environment.NewLine, row.GetElementsByTagName("td").Skip(1).Select(s => s?.InnerHtml.Replace("<br>", Environment.NewLine) ?? string.Empty)), "<[^>]*>", "").Trim();
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
                        // card.AdditionalInformation.Add($"{language?.Trim()} Name", name);
                        // card.AdditionalInformation.Add($"{language?.Trim()} Description", description);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ParseCardTableAsync(IElement dom, ref Card card)
        {
            var cardTable = dom.GetElementsByClassName("innertable").FirstOrDefault()?.FirstElementChild ?? throw new NullReferenceException("cardTable");
            var tableRows = cardTable.GetElementsByTagName("tr");

            foreach (var row in tableRows)
            {

                if (row.FirstElementChild?.FirstElementChild?.ClassName == "lore")
                {
                    if (row.FirstElementChild.FirstElementChild.TagName != "P"
                        && row.TextContent.ToLower().Contains("effect") == false) continue;

                    var descriptionUnformatted = row.FirstElementChild;
                    var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", Environment.NewLine), "<[^>]*>", "").Trim();
                    card.DescriptionEnglish = WebUtility.HtmlDecode(descriptionFormatted);
                    //if (string.IsNullOrEmpty(card.PendulumScale) && row.FirstElementChild.FirstElementChild.TagName == "P")
                    //{
                    //    var descriptionUnformatted = row.FirstElementChild;
                    //    var descriptionFormatted = Regex.Replace(descriptionUnformatted.InnerHtml.Replace("<br>", Environment.NewLine), "<[^>]*>", "").Trim();
                    //    card.DescriptionEnglish = WebUtility.HtmlDecode(descriptionFormatted);
                    //}
                    //else if (!string.IsNullOrEmpty(card.PendulumScale) && row.TextContent.Contains("effect", StringComparison.InvariantCultureIgnoreCase))
                    //    card.DescriptionEnglish = row.TextContent.Replace("\n", " ").Trim();
                }
                else
                {
                    var header = row.GetElementsByTagName("th").FirstOrDefault()?.TextContent.Trim('\n');
                    var value = row.GetElementsByTagName("td").FirstOrDefault()?.TextContent?.Replace("\n", " ").Trim();

                    switch (header?.ToLower().Trim())
                    {
                        case "statuses":
                        case null:
                        case "":
                            break;
                        default:
                            AddInformationToCard(ref card, header?.Trim(), value);
                            break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static Task ParseBasicsAsync(IElement dom, ref Card card)
        {
            card.NameEnglish = dom.GetElementsByClassName("heading").FirstOrDefault()
                ?.TextContent.Trim();
            card.ImageUrl = dom.GetElementsByClassName("cardtable-main_image-wrapper").FirstOrDefault()
                                ?.GetElementsByTagName("img").First().GetAttribute("srcset")?.Split(' ').First()
                ?? dom.GetElementsByClassName("cardtable-main_image-wrapper").FirstOrDefault()?.GetElementsByTagName("img").First().GetAttribute("src");

            return Task.CompletedTask;
        }
    }
}

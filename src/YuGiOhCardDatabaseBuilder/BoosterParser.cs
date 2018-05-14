using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using YuGiOhWikiaApi.Models;

namespace YuGiOhCardDatabaseBuilder
{
    public static class BoosterParser
    {
        public static Booster ForHtmlDocument(dynamic boosterInfo, HtmlDocument document)
        {
            var result = new Booster();
            
            RemoveStuffFromHtml(ref document);

            var element = document.GetElementbyId("mw-content-text");

            result.name = boosterInfo.title.ToString();

            try
            {
                result.imgSrc = element.SelectNodes(".//*[contains(@class, 'image-thumbnail')]").First().GetAttributeValue("href", null);
            }
            catch (Exception)
            {
                // ignored
            }

            result.enReleaseDate = GetReleaseDate("NorthAmerica", element);
            result.jpReleaseDate = GetReleaseDate("Japan", element);
            result.skReleaseDate = GetReleaseDate("South Korea", element);
            result.worldwideReleaseDate = GetReleaseDate("Worldwide", element);
            var prefixes = RegexPrefixes(GetPrefixes(element));
            result.prefixes = prefixes;
            result.prefix = GetSetPrefix(prefixes);
            result.cardList = GetCardList(element);

            return result;
        }

        private static string RegexPrefixes(string prefixes)
        {
            if (prefixes == null) return null;

            var reg = new Regex(@"\((.*?)\)");
            var matches = reg.Matches(prefixes);
            prefixes = matches.Cast<object>().Aggregate(prefixes, (current, match) => current.Replace(match.ToString(), ""));
            var test = string.Join("|", prefixes.Split(' ')
                .Where(w => !string.IsNullOrEmpty(w) || !string.IsNullOrWhiteSpace(w))
                .Select(s => s.Trim()));
            return test;
        }

        private static string GetSetPrefix(string prefixes)
        {
            return prefixes?.Split('|').First().Split('-').First();
        }

        private static List<BoosterCard> GetCardList(HtmlNode element)
        {
            var cardList = new List<BoosterCard>();

            var sections = element.SelectNodes(".//*[contains(@class, 'tabbertab')]");
            if (sections == null) return cardList;

            foreach (var section in sections)
            {
                var language = HttpUtility.HtmlDecode(section.GetAttributeValue("title", null));
                if (language == null || !language.Equals("English")) continue;

                var tables = section.SelectNodes(".//table");
                var rows = tables?.First().SelectNodes(".//tr");
                if (rows == null) continue;

                foreach (var row in rows)
                {
                    var data = row.SelectNodes(".//td");
                    if (data == null) continue;

                    var card = new BoosterCard {language = language};

                    for (var i = 0; i < 4; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                card.setnumber = HttpUtility.HtmlDecode(data[i].InnerText.Trim());
                                break;
                            case 1:
                                card.name = HttpUtility.HtmlDecode(data[i].InnerText.Trim());
                                break;
                            case 2:
                                card.rarity = HttpUtility.HtmlDecode(data[i].InnerText.Trim());
                                break;
                            case 3:
                                card.category = HttpUtility.HtmlDecode(data[i].InnerText.Trim());
                                break;
                            default:
                                break;
                        }
                    }
                    cardList.Add(card);
                }
            }
            return cardList;
        }

        private static string GetPrefixes(HtmlNode element)
        {
            string date = null;
            try
            {
                var sections = element.SelectNodes(".//*[contains(@class, 'portable-infobox')]").First().SelectNodes(".//section[contains(@class, 'pi-item')]");
                foreach (var section in sections)
                {
                    if (section.InnerText.StartsWith("Set Information"))
                    {
                        var rows = section.SelectNodes(".//div[contains(@class, 'pi-item')]");
                        foreach (var row in rows)
                        {
                            var headers = row.SelectNodes(".//h3[contains(@class, 'pi-data-label')]");
                            if (headers.Count > 0)
                            {
                                var header = headers[0];

                                if (header.InnerText.Contains("Prefix"))
                                {
                                    date = row.SelectNodes(".//div[contains(@class, 'pi-data-value')]").First().InnerText;
                                }

                                if (header.InnerText.Equals("Prefix"))
                                {
                                    return date;
                                }
                            }
                        }
                        return date;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return date;
        }

        private static string GetReleaseDate(string type, HtmlNode element)
        {
            string date = null;
            try
            {
                var sections = element.SelectNodes(".//*[contains(@class, 'portable-infobox')]").First().SelectNodes(".//section[contains(@class, 'pi-item')]");
                foreach (var section in sections)
                {
                    if (section.InnerText.StartsWith("Release dates"))
                    {
                        var rows = section.SelectNodes(".//div[contains(@class, 'pi-item')]");
                        foreach (var row in rows)
                        {
                            var headers = row.SelectNodes(".//h3[contains(@class, 'pi-data-label')]");
                            if (headers.Count > 0)
                            {
                                var header = headers[0];

                                if (header.InnerText.StartsWith(type))
                                {
                                    date = row.SelectNodes(".//div[contains(@class, 'pi-data-value')]").First().InnerText;
                                }

                                if (header.InnerText.Equals(type))
                                {
                                    return date;
                                }
                            }
                        }
                        return date;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return date;
        }

        private static void RemoveStuffFromHtml(ref HtmlDocument document)
        {
            foreach (var script in document.DocumentNode.Descendants("script").ToArray())
                script.Remove();
            foreach (var noscript in document.DocumentNode.Descendants("noscript").ToArray())
                noscript.Remove();
            foreach (var style in document.DocumentNode.Descendants("style").ToArray())
                style.Remove();
            foreach (var sup in document.DocumentNode.Descendants("sup").ToArray())
                sup.Remove();
        }
    }
}

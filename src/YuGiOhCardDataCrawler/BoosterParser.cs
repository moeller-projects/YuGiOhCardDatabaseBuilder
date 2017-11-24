using HtmlAgilityPack;
using System;
using System.Linq;
using YuGiOhWikiaApi.Models;

namespace YuGiOhCardDataCrawler
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

            return result;
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

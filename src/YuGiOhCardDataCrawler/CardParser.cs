using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using YuGiOhWikiaApi.Models;

namespace YuGiOhCardDataCrawler
{
    public static class CardParser
    {
        public static Card ForHtmlDocument(dynamic cardInfo, HtmlDocument document)
        {
            var result = new Card();

            RemoveStuffFromHtml(ref document);

            var element = document.GetElementbyId("mw-content-text");
            var cardTableCollection = element.SelectNodes(".//table[contains(@class, 'cardtable')]");
            if (cardTableCollection == null)
            {
                return result;
            }
            var cardTable = cardTableCollection.First();
            var rows = cardTable.SelectNodes(".//*[contains(@class, 'cardtablerow')]");
            if (rows == null)
            {
                return result;
            }

            try
            {
                var image = element.SelectNodes(".//*[contains(@class, 'cardtable-cardimage')]").First()
                    .SelectNodes(".//*[contains(@class, 'image-thumbnail')]").First();
                var imageUrl = image.GetAttributeValue("href", null);
                //var match = Regex.Match(imageUrl, @"http(s?)://vignette(\d)\.wikia\.nocookie\.net/yugioh/images/./(.*?)/(.*?)/");
                //var matches = match.Groups.Cast<List<string>>();
                //var imageShortUrl = $"{string.Join("", matches.Cast<List<string>>().Skip(2))}";
                result.img = imageUrl;
            }
            catch (Exception)
            {
                // ignored
            }

            var inPageName = Regex.Replace(rows.First().SelectNodes(".//*[contains(@class, 'cardtablerowdata')]").First().InnerText, @"\n|\\", "");
            if (cardInfo.title.ToString() != inPageName)
            {
                result.realName = inPageName;
            }
            else
            {
                result.realName = cardInfo.title.ToString();
            }

            HandleRows(ref result, rows);

            var cardTableCategoriesCollection = document.DocumentNode.SelectNodes(".//*[contains(@class, 'cardtable-categories')]");
            if (cardTableCategoriesCollection != null)
            {
                var cardTableCategories = cardTableCategoriesCollection.First();
                foreach (var hlist in cardTableCategories.SelectNodes(".//*[contains(@class, 'hlist')]"))
                {
                    var dl = hlist.SelectNodes(".//dl").First();
                    var dt = dl.SelectNodes(".//dt").First().InnerText;

                    if (!dt.ToLower().Contains("archetypes"))
                    {
                        continue;
                    }

                    var archetypes = dl.SelectNodes(".//dd");

                    result.archetype = string.Join(", ", archetypes.Select(type => HttpUtility.HtmlDecode(type.InnerText)?.Trim()));
                }
            }

            return result;
        }

        private static void HandleRows(ref Card result, HtmlNodeCollection rows)
        {
            foreach (var row in rows)
            {
                var headerCollection = row.SelectNodes(".//*[contains(@class, 'cardtablerowheader')]");
                HtmlNode header = null;
                if (headerCollection != null)
                {
                    header = headerCollection.DefaultIfEmpty(null).First();
                }
                //if (header == null)
                //{
                //    continue;
                //}
                var headerText = header?.InnerText;

                var dataCollection = row.SelectNodes(".//*[contains(@class, 'cardtablerowdata')]");
                string data = null;
                if (dataCollection != null)
                {
                    data = dataCollection.DefaultIfEmpty(null).First().InnerText.Trim();
                }

                var spanrows = row.SelectNodes(".//*[contains(@class, 'cardtablespanrow')]");
                if (spanrows != null)
                {
                    HandleSpanRows(ref result, spanrows);
                }

                if (data == null)
                    continue;

                switch (headerText)
                {
                    case "English": result.name_english = HttpUtility.HtmlDecode(data); break;
                    case "French": result.name_french = HttpUtility.HtmlDecode(data); break;
                    case "German": result.name_german = HttpUtility.HtmlDecode(data); break;
                    case "Italian": result.name_italian = HttpUtility.HtmlDecode(data); break;
                    case "Portuguese": result.name_portuguese = HttpUtility.HtmlDecode(data); break;
                    case "Spanish": result.name_spanish = HttpUtility.HtmlDecode(data); break;
                    case "Attribute": result.attribute = data; break; // EARTH
                    case "Card type": result.cardType = data; break; // Spell, Monster
                    case "Types": result.types = data; break; // Token, Fairy / Effect
                    case "Type": result.types = data; break; // Fiend
                    case "Level": result.level = data; break; // 6
                    case "ATK / DEF":
                        {                              // 2500 / 2000
                            result.atk = data.Split('/')[0].Trim();
                            result.def = data.Split('/')[1].Trim();
                            break;
                        }
                    case "ATK / LINK":
                        {                              // 1400 / 2
                            result.atk = data.Split('/')[0].Trim();
                            result.link = data.Split('/')[1].Trim();
                            break;
                        }
                    case "Passcode": result.passcode = data; break; // 64163367
                    case "Card effect types": result.effectTypes = data; break; // Continuous-like, Trigger-like
                    case "Materials": result.materials = data; break; // "Genex Controller" + 1 or more non-Tuner WATER monsters
                    case "Fusion Material": result.fusionMaterials = data; break; // "Blue-Eyes White Dragon"
                    case "Rank": result.rank = data; break; // 4
                    case "Ritual Spell Card required": result.ritualSpell = data; break; // "Zera Ritual"
                    case "Pendulum Scale": result.pendulumScale = data; break; // 1
                    case "Link Arrows": result.linkMarkers = data.Replace(" , ", ", "); break; // Top , Bottom-Left , Bottom-Right
                    case "Property": result.property = data; break; // Continuous
                    case "Summoned by the effect of": result.summonedBy = data; break; // "Gorz the Emissary of Darkness"
                    case "Limitation text": result.limitText = data; break; // This card cannot be in a Deck.
                    case "Synchro Material": result.synchroMaterial = data; break; // "Genex Controller"
                    case "Ritual Monster required": result.ritualMonster = data; break; // "Zera the Mant"
                    case "Statuses":                                                     // Legal
                        break;
                    //    try
                    //    {
                    //        var rowspan = header.Attributes.Where(w => w.Name == "rowspan").DefaultIfEmpty(null).First();
                    //        int numStatusRow;
                    //        if (rowspan != null && !rowspan.Equals(""))
                    //        {
                    //            numStatusRow = int.Parse(rowspan.Value);
                    //        }
                    //        else
                    //        {
                    //            numStatusRow = 1;
                    //        }

                    //        for (int r = 0; r < numStatusRow; r++)
                    //        {
                    //            var statusRow = rows.get(i + r);
                    //            String statusRowData = statusRow.getElementsByClass("cardtablerowdata").first().text().trim();
                    //            String status;
                    //            if (statusRowData.contains("Not yet released"))
                    //            {
                    //                status = "Not yet released";
                    //            }
                    //            else
                    //            {
                    //                status = statusRowData.split(" ")[0];
                    //            }

                    //            if (status.equals("Unlimited"))
                    //            {
                    //                status = "U";
                    //            }

                    //            if (numStatusRow == 1)
                    //            {
                    //                ocgStatus = status;
                    //                tcgAdvStatus = status;
                    //                tcgTrnStatus = status;
                    //            }
                    //            else
                    //            {
                    //                if (statusRowData.contains("OCG"))
                    //                {
                    //                    ocgStatus = status;
                    //                }

                    //                if (statusRowData.contains("Advanced"))
                    //                {
                    //                    tcgAdvStatus = status;
                    //                }

                    //                if (statusRowData.contains("Traditional"))
                    //                {
                    //                    tcgTrnStatus = status;
                    //                }
                    //            }
                    //        }

                    //        // skip through the status rows
                    //        i = i + numStatusRow - 1;
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Console.WriteLine("Error getting status: " + cardInfo.title.ToString());
                    //        Console.WriteLine(JsonConvert.SerializeObject(e));
                    //    }
                    //    break;
                    default:
                        Debug.WriteLine("Attribute not found: " + headerText);
                        break;
                }
            }
        }

        private static void HandleSpanRows(ref Card result, HtmlNodeCollection spanrows)
        {
            foreach (var spanrow in spanrows)
            {
                var spanData = spanrow.SelectNodes(".//b").DefaultIfEmpty(null).First().InnerText.Trim();
                switch (spanData)
                {
                    case "Card descriptions":
                        var spanTables = spanrow.SelectNodes(".//table[contains(@class, 'navbox hlist')]").DefaultIfEmpty(null);
                        foreach (var spanTable in spanTables)
                        {
                            var title = HttpUtility.HtmlDecode(spanTable.SelectNodes(".//th[contains(@class, 'navbox-title')]").DefaultIfEmpty(null).First().InnerText)?.Trim();
                            var data = HttpUtility.HtmlDecode(spanTable.SelectNodes(".//td[contains(@class, 'navbox-list')]").DefaultIfEmpty(null).First().InnerText)?.Trim();
                            switch (title)
                            {
                                case "English":
                                    result.description_english = HttpUtility.HtmlDecode(data);
                                    break;
                                case "German":
                                    result.description_german = HttpUtility.HtmlDecode(data);
                                    break;
                                case "French":
                                    result.description_french = HttpUtility.HtmlDecode(data);
                                    break;
                                case "Italian":
                                    result.description_italian = HttpUtility.HtmlDecode(data);
                                    break;
                                case "Portuguese":
                                    result.description_portuguese = HttpUtility.HtmlDecode(data);
                                    break;
                                case "Spanish":
                                    result.description_spanish = HttpUtility.HtmlDecode(data);
                                    break;
                                default:
                                    Debug.WriteLine("Attribute not found: " + spanData);
                                    break;
                            }
                        }
                        break;
                    default:
                        Debug.WriteLine("Attribute not found: " + spanData);
                        break;
                }
            }
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
            var sysops = document.DocumentNode.SelectNodes(".//*[contains(@class, 'sysop-show')]");
            if (sysops == null) return;
            foreach (var sysop in sysops.ToArray())
                sysop.Remove();
        }
    }
}

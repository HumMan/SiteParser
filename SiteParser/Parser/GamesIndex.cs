using HtmlAgilityPack;
using Shared.Model;
using SiteParser.CacheProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SiteParser.Parser
{
    class GamesIndex
    {
        public static GameInfo[] ParseSite()
        {
            //загружаем все страницы
            var firstDoc = Cache.LoadPage(1);
            int maxPage;
            //максимальное кол-во страниц
            maxPage = FindMaxPages(firstDoc);
            //обходим все страницы (первая уже загружена)
            {

                var info = new List<GameInfo>();
                info.AddRange(ParsePage(firstDoc));
#if PARALLEL
                Parallel.For(2, maxPage + 1, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (currIndex) =>
#else
                for (int currIndex = 2; currIndex <= maxPage; currIndex++)
#endif
                {
                    var doc = Cache.LoadPage(currIndex);
                    var parseResult = ParsePage(doc);
#if PARALLEL
                    lock (info)
#endif
                    {
                        info.AddRange(parseResult);
                    }
                }
#if PARALLEL
                );
#endif

                {
                    var ids = info.Select(i => i.Id).ToArray();
                    Array.Sort(ids);
                    var lastVal = ids[0];
                    for (int i = 1; i < ids.Length; i++)
                    {
                        if (ids[i] == lastVal)
                            throw new Exception("Duplicate record error");
                        lastVal = ids[i];
                    }
                }

                {
                    int i = 1;
                    int total = info.Count;
#if PARALLEL
                    Parallel.For(0, total, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (currIndex) =>
#else
                    foreach (var gameInfo in info)
#endif
                    {
#if PARALLEL
                            var gameInfo = info[currIndex];
#endif
                        GameDesc.GetGameDesc(gameInfo);
#if PARALLEL
                            if (Interlocked.Increment(ref i) % 500 == 0)
                                Console.WriteLine("{0} gameDesc of \t{1} total", i, total);
                    });
#else
                    }
#endif
                }
                var result = info.OrderBy(i => i.Id).ToArray();
                return result;
            }
        }

        private static GameInfo[] ParsePage(HtmlDocument doc)
        {
            var result = new List<GameInfo>();
            var gamesTable = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/table");
            foreach (var tr in gamesTable.SelectNodes("tr[td]"))
            {
                GameInfo gameInfo = new GameInfo();
                var gameNodes = tr.ChildNodes.Where(i => i.Name == "td").ToArray();
                {
                    var titleNode = gameNodes[0].SelectSingleNode("table/tr/td[2]/a");
                    gameInfo.Name = HttpUtility.HtmlDecode(titleNode.GetAttributeValue("title", null));
                    gameInfo.Id = Shared.GetGameIdFromUrl(titleNode.GetAttributeValue("href", null));
                }
                {
                    var columnNode = gameNodes[1].SelectSingleNode("a");
                    gameInfo.Genre = columnNode.InnerText;

                    columnNode = gameNodes[2].SelectSingleNode("a");
                    gameInfo.Year = columnNode.InnerText;

                    columnNode = gameNodes[3].SelectSingleNode("a");
                    gameInfo.Platform = columnNode.InnerText;

                    columnNode = gameNodes[4].SelectSingleNode("a");
                    gameInfo.Publisher = HttpUtility.HtmlDecode(columnNode.InnerText);

                    columnNode = gameNodes[5].SelectSingleNode("img");

                    var stars = columnNode.GetAttributeValue("title", null);
                    if (stars == "Оценка отсутствует")
                    {
                        gameInfo.Stars = -1;
                    }
                    else
                    {
                        var match = Regex.Match(stars, @"Оценка рецензента - (\d+) из 10");
                        gameInfo.Stars = int.Parse(match.Groups[1].Value);
                    }
                }

                result.Add(gameInfo);
            }
            return result.ToArray();
        }

        private static int FindMaxPages(HtmlDocument doc)
        {
            int maxPage;
            {
                var pagerEl = doc.DocumentNode.SelectSingleNode(".//ul[@class='pager']");
                var pageNode = pagerEl.LastChild;
                while (!int.TryParse(pageNode.InnerText, out maxPage))
                {
                    pageNode = pageNode.PreviousSibling;
                }
            }

            return maxPage;
        }
    }
}

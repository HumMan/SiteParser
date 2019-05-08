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
        public static GameInfo[] ParseGamesCatalog(ICache cache)
        {
            //загружаем все страницы
            var firstDoc = cache.LoadPage(1);
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
                    var doc = cache.LoadPage(currIndex);
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
                var result = info.OrderBy(i => i.Id).ToArray();

                CheckDuplicates(result);

                return result;
            }
        }

        private static void CheckDuplicates(GameInfo[] info)
        {
            var lastVal = info[0].Id;
            for (int i = 1; i < info.Length; i++)
            {
                if (info[i].Id == lastVal)
                    throw new Exception("Duplicate record error");
                lastVal = info[i].Id;
            }
        }

        public static void EnrichGamesList(ICache cache, GameInfo[] info)
        {       
                int total = info.Length;
#if PARALLEL
                int i = 1;
                Parallel.For(0, total, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (currIndex) =>
#else
                foreach (var gameInfo in info)
#endif
                {
#if PARALLEL
                            var gameInfo = info[currIndex];
#endif
                    GameDesc.GetGameDesc(cache, gameInfo);
#if PARALLEL
                            if (Interlocked.Increment(ref i) % 500 == 0)
                                Console.WriteLine("{0} gameDesc of \t{1} total", i, total);
                    });
#else
                }
#endif
            
        }

        private static GameInfo[] ParsePage(HtmlDocument doc)
        {
            var result = new List<GameInfo>();
            var gamesTable = doc.DocumentNode.SelectSingleNode(".//div[@class='main-content']/table");
            var items = gamesTable.SelectNodes("tr[td]");
            foreach (var tr in items)
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

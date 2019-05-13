using HtmlAgilityPack;
using Shared.Model;
using SiteParser.CacheProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                for (int currIndex = 2; currIndex <= maxPage; currIndex++)
                {
                    var doc = cache.LoadPage(currIndex);
                    var parseResult = ParsePage(doc);
                    {
                        info.AddRange(parseResult);
                    }
                }
                var result = info.OrderBy(i => i.Id).ToArray();

                CheckDuplicates(result);

                return result;
            }
        }

        public static void CompareCatalog(GameInfo[] oldList, GameInfo[] newList, out int newGames, out int deletedGames)
        {
            newGames = 0;
            deletedGames = 0;
            int i = 0, k = 0;
            while (i < oldList.Length && k < newList.Length)
            {
                if (i == oldList.Length - 1)
                {
                    k++;
                    newGames++;
                }
                else if (k == newList.Length - 1)
                {
                    i++;
                    deletedGames++;
                }
                else if (oldList[i].Id < newList[k].Id)
                {
                    i++;
                    deletedGames++;
                }
                else if (oldList[i].Id > newList[k].Id)
                {
                    k++;
                    newGames++;
                }
                else
                {
                    k++;
                    i++;
                }
            }
        }

        public class CompareResult
        {
            public int newScreenshots;
            public int newGameGroups;
            public int newComments;
            public int newRecomended;
            public int newMods;

            public int deletedScreenshots;
            public int deletedGameGroups;
            public int deletedComments;
            public int deletedRecomended;
            public int deletedMods;
        }


        public static void CompareGames(GameInfo[] oldList, GameInfo[] newList, ref CompareResult result)
        {
            int i = 0, k = 0;
            while (i < oldList.Length && k < newList.Length)
            {
                if (oldList[i].Id < newList[k].Id)
                {
                    i++;
                }
                else if (oldList[i].Id > newList[k].Id)
                {
                    k++;
                }
                else
                {
                    CompareGame(oldList[i], newList[k], ref result);
                    i++;
                    k++;
                }
            }
        }

        private static void Delta(int newCount, int oldCount, ref int newResult, ref int deletedResult)
        {
            var result = newCount - oldCount;
            if (result >= 0)
                newResult += result;
            else
                deletedResult += -result;
        }

        public static void CompareGame(GameInfo oldGame, GameInfo newGame, ref CompareResult result)
        {
            Delta(newGame.Comments.Count, oldGame.Comments.Count, ref result.newComments, ref result.deletedComments);
            Delta(newGame.GameGroups.Count, oldGame.GameGroups.Count, ref result.newGameGroups, ref result.deletedGameGroups);
            Delta(newGame.Mods.Count, oldGame.Mods.Count, ref result.newMods, ref result.deletedMods);
            Delta(newGame.Recomended.Count, oldGame.Recomended.Count, ref result.newRecomended, ref result.deletedRecomended);
            Delta(newGame.Screenshots.Count, oldGame.Screenshots.Count, ref result.newScreenshots, ref result.deletedScreenshots);
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
            foreach (var gameInfo in info)
            {
                GameDesc.GetGameDesc(cache, gameInfo);
            }
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

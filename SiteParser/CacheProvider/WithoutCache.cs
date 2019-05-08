using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SiteParser.CacheProvider
{
    /// <summary>
    /// Используется когда весь парсинг отлажен и нам не обязательно хранить страницы
    /// </summary>
    public class WithoutCache : ICache
    {
        public readonly string TargetSite;

        private static HtmlDocument Download(string url)
        {
            var http = new HtmlWeb();
            Console.WriteLine("Downloading {0}", url);
            return http.Load(url);
        }

        public HtmlDocument LoadPage(int id)
        {
            var result = Download(TargetSite + $"catalog/?page={id}");
            return result;
        }

        public HtmlDocument LoadComments(int gameId, int id, string threadId)
        {
            var url = TargetSite + $"game/game_comments.php";
            string myParameters = $"gameid={gameId}&gamethreadid={threadId}&page={id}";
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
                Console.WriteLine("Downloading comments {0}/{1}", url, myParameters);
                string HtmlResult = wc.UploadString(url, myParameters);
                var result = new HtmlDocument();
                result.LoadHtml(HtmlResult);
                return result;
            }
        }

        public HtmlDocument LoadGameDesc(int gameId)
        {
            var result = Download(TargetSite + $"game/{gameId}.html");
            return result;
        }

        public HtmlDocument LoadGameScreenshots(int gameId)
        {
            var result = Download(TargetSite + $"game/screenshots/{gameId}.html");
            return result;
        }
    }
}

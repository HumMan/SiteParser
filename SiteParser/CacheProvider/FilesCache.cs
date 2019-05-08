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
    /// Кешируем страницы в виде файловой структуры   
    /// </summary>
    public class FilesCache: ICache
    {
        public readonly string TargetSite;

        private static HtmlDocument Download(string url)
        {
            var http = new HtmlWeb();
            Console.WriteLine("Downloading {0}", url);
            return http.Load(url);
        }
        private static void CreateDir(string path)
        {
            if (!Directory.GetParent(path).Exists)
                Directory.CreateDirectory(Directory.GetParent(path).FullName);
        }

        public FilesCache(string targetSite)
        {
            TargetSite = targetSite;
        }

        public HtmlDocument LoadPage(int id)
        {
            var path = $"data/pages_html/{id}.html.cached";
            if (File.Exists(path))
            {
                var result = new HtmlDocument();
                result.Load(path);
                return result;
            }
            else
            {
                var result = Download(TargetSite + $"catalog/?page={id}");
                result.Save(path);
                CreateDir(path);
                return result;
            }
        }

        public HtmlDocument LoadComments(int gameId, int id, string threadId)
        {
            var path = $"data/pages_html/{gameId}/comments/{id}.html.cached";
            if (File.Exists(path))
            {
                var result = new HtmlDocument();
                result.Load(path);
                return result;
            }
            else
            {
                var url = TargetSite+$"game/game_comments.php";
                string myParameters = $"gameid={gameId}&gamethreadid={threadId}&page={id}";
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
                    Console.WriteLine("Downloading comments {0}/{1}", url, myParameters);
                    string HtmlResult = wc.UploadString(url, myParameters);
                    var result = new HtmlDocument();
                    result.LoadHtml(HtmlResult);
                    CreateDir(path);
                    result.Save(path);
                    return result;
                }
            }
        }

        public HtmlDocument LoadGameDesc(int gameId)
        {
            var path = $"data/pages_html/{gameId}/desc.html.cached";
            if (File.Exists(path))
            {
                var result = new HtmlDocument();
                result.Load(path);
                return result;
            }
            else
            {
                var result = Download(TargetSite+$"game/{gameId}.html");
                CreateDir(path);
                result.Save(path);
                return result;
            }
        }

        public HtmlDocument LoadGameScreenshots(int gameId)
        {
            var path = $"data/pages_html/{gameId}/screenshots.html.cached";
            if (File.Exists(path))
            {
                var result = new HtmlDocument();
                result.Load(path);
                return result;
            }
            else
            {
                var result = Download(TargetSite + $"game/screenshots/{gameId}.html");
                CreateDir(path);
                result.Save(path);
                return result;
            }
        }


    }
}

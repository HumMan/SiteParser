using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace SiteParser.CacheProvider
{
    /// <summary>
    /// Кешируем страницы в виде единого архива
    /// При обновлении архив полностью перезаписывается
    /// </summary>
    public class ZipArchiveCache : ICache, IDisposable
    {
        private readonly string _targetSite;
        private readonly string _archivePath;

        private ZipArchive _archive;
        private FileStream _zipFileStream;

        private readonly bool _readOnly;

        private static HtmlDocument Download(string url)
        {
            var http = new HtmlWeb();
            Console.WriteLine("Downloading {0}", url);
            return http.Load(url);
        }

        public ZipArchiveCache(string targetSite, string archivePath, bool readOnly = false)
        {
            _targetSite = targetSite;
            _archivePath = archivePath;
            _readOnly = readOnly;

            OpenArchive();
        }

        private void OpenArchive()
        {
            _zipFileStream = new FileStream(_archivePath, _readOnly ? FileMode.Open:FileMode.Create);
            _archive = new ZipArchive(_zipFileStream, _readOnly ? ZipArchiveMode.Read:ZipArchiveMode.Create);
        }

        public HtmlDocument LoadPage(int id)
        {
            var path = $"{id}.html.cached";
            if (_readOnly)
            {
                var result = new HtmlDocument();
                var entry = _archive.GetEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Load(entryStream);
                }
                return result;
            }
            else
            {
                var result = Download(_targetSite + $"catalog/?page={id}");
                var entry = _archive.CreateEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Save(entryStream);
                }
                return result;
            }
        }

        public HtmlDocument LoadComments(int gameId, int id, string threadId)
        {
            var path = $"{gameId}/comments/{id}.html.cached";
            if (_readOnly)
            {
                var result = new HtmlDocument();
                var entry = _archive.GetEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Load(entryStream);
                }
                return result;
            }
            else
            {
                var url = _targetSite+$"game/game_comments.php";
                string myParameters = $"gameid={gameId}&gamethreadid={threadId}&page={id}";
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";
                    Console.WriteLine("Downloading comments {0}/{1}", url, myParameters);
                    string HtmlResult = wc.UploadString(url, myParameters);
                    var result = new HtmlDocument();
                    result.LoadHtml(HtmlResult);
                    var entry = _archive.CreateEntry(path);
                    using (var entryStream = entry.Open())
                    {
                        result.Save(entryStream);
                    }
                    return result;
                }
            }
        }

        public HtmlDocument LoadGameDesc(int gameId)
        {
            var path = $"{gameId}/desc.html.cached";
            if (_readOnly)
            {
                var result = new HtmlDocument();
                var entry = _archive.GetEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Load(entryStream);
                }
                return result;
            }
            else
            {
                var result = Download(_targetSite+$"game/{gameId}.html");
                var entry = _archive.CreateEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Save(entryStream);
                }
                return result;
            }
        }

        public HtmlDocument LoadGameScreenshots(int gameId)
        {
            var path = $"{gameId}/screenshots.html.cached";
            if (_readOnly)
            {
                var result = new HtmlDocument();
                var entry = _archive.GetEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Load(entryStream);
                }
                return result;
            }
            else
            {
                var result = Download(_targetSite + $"game/screenshots/{gameId}.html");
                var entry = _archive.CreateEntry(path);
                using (var entryStream = entry.Open())
                {
                    result.Save(entryStream);
                }
                return result;
            }
        }

        public void Dispose()
        {
            _archive.Dispose();
            _zipFileStream.Close();
        }
    }
}

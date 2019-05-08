using Shared.Model;
using Shared.ModelSerialize;
using SiteParser.CacheProvider;
using SiteParser.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SiteParser
{
    class Program
    {
        static JsonModelSerializer modelSerialize = new JsonModelSerializer();

        static void Main(string[] args)
        {
            var TargetSite = "https://www.old-games.ru/";

            var startTime = DateTime.Now;
            Console.WriteLine("Parser started " + startTime.ToString("yyyy.MM.dd HH-mm-ss"));
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            if (!Directory.Exists("data/pages_html"))
                Directory.CreateDirectory("data/pages_html");

            GameInfo[] info;
            {

                if (!File.Exists(JsonModelSerializer.CatalogZip))
                {
                    using (var cache = new ZipArchiveCache(TargetSite, "data/catalog_cache.zip", true))
                    {
                        info = GamesIndex.ParseGamesCatalog(cache);
                    }
                    modelSerialize.SaveCatalog(info);
                }
                else
                {
                    info = modelSerialize.LoadCatalog();
                }
            }
            {
                if (!File.Exists(JsonModelSerializer.FullGamesListZip))
                {
                    using (var cache = new ZipArchiveCache(TargetSite, "data/pages_cache.zip", true))
                    {
                        GamesIndex.EnrichGamesList(cache, info);
                    }
                    Console.WriteLine("Pages list parsed " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss"));
                    modelSerialize.SaveGamesList(info);
                }
                else
                {
                    info = modelSerialize.LoadGamesList();
                }

            }

            Assets.DownloadImages(TargetSite, info);
            Console.WriteLine("Screenshots downloaded " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss"));
            Console.WriteLine(string.Format("All time {0} sec", (DateTime.Now - startTime).TotalSeconds));
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}

using CommandLine;
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SiteParser
{
    enum CacheType
    {
        None,
        Files,
        Archive
    }
    enum CacheOperation
    {
        None,
        Update,
        Parse
    }
    class Options
    {
        [Option("site",
           Default = "https://www.old-games.ru/",
           HelpText = "")]
        public string TargetSite { get; set; }
        [Option("cacheType",
            Default = CacheType.Archive,
            HelpText = "")]
        public CacheType CacheType { get; set; }
        [Option("catalog", Default = CacheOperation.None,
            HelpText = "")]
        public CacheOperation Catalog { get; set; }
        [Option("games", Default = CacheOperation.None,
            HelpText = "")]
        public CacheOperation GamesList { get; set; }

        [Option("preloadImages", Default = false,
            HelpText = "")]
        public bool PreloadImages { get; set; }
    }
    class Program
    {
        private const string CatalogZipCachePath = "data/catalog_cache.zip";
        private const string GamesListZipCachePath = "data/pages_cache.zip";

        private const string CatalogFilesCacheDir = "data/catalog_html";
        private const string GamesListFilesCacheDir = "data/games_list_html";

        static JsonModelSerializer modelSerialize = new JsonModelSerializer();

        private static ICache CreateCache(bool isCatalog, CacheType type, string targetSite, bool readOnly)
        {
            switch (type)
            {
                case CacheType.None:
                    return new WithoutCache();
                case CacheType.Files:
                    return new FilesCache(targetSite, isCatalog ? CatalogFilesCacheDir : GamesListFilesCacheDir);
                case CacheType.Archive:
                    return new ZipArchiveCache(targetSite, isCatalog ? CatalogZipCachePath : GamesListZipCachePath, readOnly);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void PrintFields(object obj)
        {
            FieldInfo[] objFields = obj.GetType().GetFields();
            foreach (var field in objFields)
            {
                object value = field.GetValue(obj);
                Console.WriteLine("{0}: {1}", field.Name, value.ToString());
            }
        }

        static void Main(string[] args)
        {

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       var startTime = DateTime.Now;
                       Console.WriteLine("Parser started " + startTime.ToString("yyyy.MM.dd HH-mm-ss"));

                       if (!Directory.Exists("data"))
                           Directory.CreateDirectory("data");

                       GameInfo[] info;
                       {
                           var catalogExists = modelSerialize.CatalogExists();
                           if (!catalogExists || o.Catalog != CacheOperation.None)
                           {
                               GameInfo[] oldCatalog = null;
                               if (catalogExists)
                               {
                                   oldCatalog = modelSerialize.LoadCatalog();
                                   modelSerialize.RemoveCatalog();
                               }

                               using (var cache = CreateCache(true, o.CacheType, o.TargetSite, o.Catalog != CacheOperation.Update))
                               {
                                   info = GamesIndex.ParseGamesCatalog(cache);
                               }

                               modelSerialize.SaveCatalog(info);

                               if (catalogExists)
                               {
                                   GamesIndex.CompareCatalog(oldCatalog, info, out int newGames, out int deletedGames);
                                   Console.WriteLine("{0} games compare result", info.Length);
                                   Console.WriteLine("New games: {0} Deleted: {0} ", newGames, deletedGames);
                               }
                           }
                           else
                           {
                               info = modelSerialize.LoadCatalog();
                           }
                       }
                       {
                           var gamesListExists = modelSerialize.GamesListExists();
                           if (!gamesListExists || o.GamesList != CacheOperation.None)
                           {
                               GameInfo[] oldGamesList = null;
                               if (gamesListExists)
                               {
                                   oldGamesList = modelSerialize.LoadGamesList();
                                   modelSerialize.RemoveGamesList();
                               }

                               using (var cache = CreateCache(false, o.CacheType, o.TargetSite, o.GamesList != CacheOperation.Update))
                               {
                                   GamesIndex.EnrichGamesList(cache, info);
                               }
                               Console.WriteLine("Pages list parsed " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss"));
                               modelSerialize.SaveGamesList(info);

                               if (gamesListExists)
                               {
                                   var compareResult = new GamesIndex.CompareResult();
                                   GamesIndex.CompareGames(oldGamesList, info, ref compareResult);
                                   PrintFields(compareResult);
                               }
                           }
                           else
                           {
                               info = modelSerialize.LoadGamesList();
                           }

                       }

                       if (o.PreloadImages)
                       {
                           Assets.DownloadImages(o.TargetSite, info);
                           Console.WriteLine("Screenshots downloaded " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss"));
                       }
                       Console.WriteLine(string.Format("All time {0} sec", (DateTime.Now - startTime).TotalSeconds));
                       Console.WriteLine("Finished");
                       Console.ReadKey();

                   });
        }
    }
}

using Shared.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiteParser.Parser
{
    public class Assets
    {
        public static void DownloadImages(GameInfo[] info)
        {
            var screenshots = info.SelectMany(i => i.Screenshots);

            Console.WriteLine("Downloading thumbs");
            DownloadFilesList(screenshots.Select(i => i.ThumbUrl).ToList(), "assets");
            Console.WriteLine("Downloading screenshots");
            DownloadFilesList(screenshots.Select(i => i.Url).ToList(), "assets");
            Console.WriteLine("Downloading covers");
            DownloadFilesList(info.Where(i => i.CoverImageUrl != null).Select(i => i.CoverImageUrl).ToList(), "assets");
        }

        private static void DownloadFilesList(List<string> files, string dirName)
        {

            var i = 1;
            var total = files.Count();
#if PARALLEL
            Parallel.For(0, total, new ParallelOptions { MaxDegreeOfParallelism = 2 }, (currIndex) =>
#else
                foreach (var s in files)
#endif
            {
#if PARALLEL
                var s = files[currIndex];
#endif
                var path = Path.Combine("data", dirName, s.TrimStart('/'));
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(path))
                {
                    try
                    {
                        var url = CacheProvider.Cache.TargetSite + s;
                        Console.WriteLine("Downloading file {0}", url);
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(url, path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
#if PARALLEL
                if (Interlocked.Increment(ref i) % 500 == 0)
                    Console.WriteLine("{0} downloaded of \t{1} total", i, total);
#endif
            }
#if PARALLEL
                );
#endif
        }
    }
}

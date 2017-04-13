using Shared.Model;
using Shared.ModelSerialize;
using SiteParser.Parser;
using System;
using System.Collections.Generic;
using System.IO;
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
            var startTime = DateTime.Now;
            Console.WriteLine("Parser started "+ startTime.ToString("yyyy.MM.dd HH-mm-ss"));
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            if (!Directory.Exists("data/pages_html"))
                Directory.CreateDirectory("data/pages_html");

            GameInfo[] info;

            if (!File.Exists(JsonModelSerializer.GamesListFileNameZip))
            {
                info = GamesIndex.ParseSite();
                modelSerialize.SaveGamesList(info);
            }
            else
            {
                info = modelSerialize.LoadGamesList();
            }
            Console.WriteLine("Pages list parsed " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss"));
            Assets.DownloadImages(info);
            Console.WriteLine("Screenshots downloaded " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss"));
            Console.WriteLine("All time " + (DateTime.Now-startTime).ToString());
            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}

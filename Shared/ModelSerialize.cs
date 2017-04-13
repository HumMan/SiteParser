using Newtonsoft.Json;
using Shared.Model;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Shared.ModelSerialize
{
    public interface IModelSerializer
    {
        GameInfo[] LoadGamesList();
        void SaveGamesList(GameInfo[] list);
    }

    public class JsonModelSerializer: IModelSerializer
    {
        public const string GamesListFileName = "data/games.json";
        public const string GamesListArchiveEntry = "games.json";
        public const string GamesListFileNameZip = GamesListFileName + ".zip";

        private static void Serialize(object value, Stream s)
        {
            using (StreamWriter writer = new StreamWriter(s))
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer ser = new JsonSerializer();
                    ser.Formatting = Formatting.Indented;
                    ser.Serialize(jsonWriter, value);
                }
            }
        }

        private static T Deserialize<T>(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public GameInfo[] LoadGamesList()
        {
            using (var zipFileStream = new FileStream(GamesListFileNameZip, FileMode.Open))
            using (var archive = new ZipArchive(zipFileStream))
            {
                ZipArchiveEntry entry = archive.Entries.First();
                {
                    var stream = entry.Open();
                    return Deserialize<GameInfo[]>(stream);
                }
            }
        }

        public void SaveGamesList(GameInfo[] list)
        {
            using (var stream = new FileStream(GamesListFileNameZip, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(GamesListArchiveEntry);
                    Serialize(list, readmeEntry.Open());
                }
            }
        }
    }
}

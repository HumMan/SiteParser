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
        GameInfo[] LoadCatalog();
        void SaveCatalog(GameInfo[] list);
    }

    public class JsonModelSerializer: IModelSerializer
    {
        public const string FullGamesListFileName = "data/games.json";
        public const string FullGamesListArchiveEntry = "games.json";
        public const string FullGamesListZip = FullGamesListFileName + ".zip";

        public const string CatalogFileName = "data/catalog.json";
        public const string CatalogArchiveEntry = "catalog.json";
        public const string CatalogZip = CatalogFileName + ".zip";

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
            return Load(FullGamesListZip);
        }

        public GameInfo[] LoadCatalog()
        {
            return Load(CatalogZip);
        }

        private static GameInfo[] Load(string fileName)
        {
            using (var zipFileStream = new FileStream(fileName, FileMode.Open))
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
            Save(list, FullGamesListZip, FullGamesListArchiveEntry);
        }

        public void SaveCatalog(GameInfo[] list)
        {
            Save(list, CatalogZip, CatalogArchiveEntry);
        }

        private static void Save(GameInfo[] list, string fileName, string entryName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry archiveEntry = archive.CreateEntry(entryName);
                    Serialize(list, archiveEntry.Open());
                }
            }
        }
    }
}

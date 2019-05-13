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
        private const string FullGamesListFileName = "data/games.json";
        private const string FullGamesListArchiveEntry = "games.json";
        private const string FullGamesListZip = FullGamesListFileName + ".zip";

        private const string CatalogFileName = "data/catalog.json";
        private const string CatalogArchiveEntry = "catalog.json";
        private const string CatalogZip = CatalogFileName + ".zip";

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
            using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
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

        public bool GamesListExists()
        {
            return File.Exists(FullGamesListZip);
        }

        public void RemoveGamesList()
        {
            File.Move(FullGamesListZip, FullGamesListZip + ".old");
        }

        public void SaveCatalog(GameInfo[] list)
        {
            Save(list, CatalogZip, CatalogArchiveEntry);
        }

        public bool CatalogExists()
        {
            return File.Exists(CatalogZip);
        }

        public void RemoveCatalog()
        {
            File.Move(CatalogZip, CatalogZip + ".old");
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

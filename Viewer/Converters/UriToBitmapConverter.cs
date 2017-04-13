using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Data;

namespace Viewer.Converters
{
    public class UriToBitmapConverter : IValueConverter
    {
        private static HashSet<string> cached = new HashSet<string>();
        private static string currDir = Directory.GetCurrentDirectory();

        private readonly string BaseUrl = "http://www.old-games.ru/";

        public UriToBitmapConverter()
        {
        }
        public UriToBitmapConverter(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            var stringValue = value.ToString();
            var localPath = Path.Combine(currDir, "data", "assets" + stringValue);
            if (!cached.Contains(localPath))
            {
                if (!File.Exists(localPath))
                {
                    try
                    {
                        var client = new WebClient();
                        var dir = Path.GetDirectoryName(localPath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        client.DownloadFile(BaseUrl + stringValue, localPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                cached.Add(localPath);
            }
            return localPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

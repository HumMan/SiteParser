using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Data;

namespace Viewer.Converters
{
    public class IntToStarImageUrl : IValueConverter
    {
        private static HashSet<string> cached = new HashSet<string>();
        private static string currDir = Directory.GetCurrentDirectory();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            var intValue = (int)value;
            if (intValue == -1)
                return null;
            return string.Format(@"/img/rating/star{0}.png", intValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

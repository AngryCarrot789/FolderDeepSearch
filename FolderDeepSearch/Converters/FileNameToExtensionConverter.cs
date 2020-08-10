using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FolderDeepSearch.Converters
{
    public class FileNameToExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string fileName ? Path.GetExtension(fileName) : "*";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value}";
        }
    }
}

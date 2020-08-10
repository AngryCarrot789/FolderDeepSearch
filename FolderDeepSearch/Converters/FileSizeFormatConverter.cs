using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;

namespace FolderDeepSearch.Converters
{
    public class FileSizeFormatConverter : IValueConverter
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern int StrFormatByteSize(
            long fileSize,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer,
            int bufferSize);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size)
            {
                if (size == long.MaxValue)
                    return "";

                StringBuilder sb = new StringBuilder(20);
                StrFormatByteSize(size, sb, 20);
                return sb.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (long)1;
        }
    }
}

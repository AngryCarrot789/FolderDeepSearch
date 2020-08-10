using System;
using System.Windows;
using System.Windows.Media;

namespace TheRThemes
{
    public static class ThemesController
    {
        public enum ThemeTypes
        {
            Light,
            Dark,
        }

        public static ThemeTypes CurrentTheme { get; set; }

        private static ResourceDictionary ThemeDictionary
        {
            get { return Application.Current.Resources.MergedDictionaries[0]; }
            set { Application.Current.Resources.MergedDictionaries[0] = value; }
        }

        private static void ChangeTheme(Uri uri)
        {
            ThemeDictionary = new ResourceDictionary() { Source = uri };
        }

        public static SolidColorBrush GetSolidBrush(string name)
        {
            object brush = ThemeDictionary[name];
            return brush is SolidColorBrush bruh ? bruh : new SolidColorBrush(Colors.Transparent);
        }

        public static void SetTheme(ThemeTypes theme)
        {
            string themeName = null;
            CurrentTheme = theme;
            switch (theme)
            {
                case ThemeTypes.Dark: themeName = "DarkTheme"; break;
                case ThemeTypes.Light: themeName = "LightTheme"; break;
            }

            try
            {
                if (!string.IsNullOrEmpty(themeName))
                    ChangeTheme(new Uri($"Themes/{themeName}.xaml", UriKind.Relative));
            }
            catch { }
        }
    }
}

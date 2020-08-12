using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Drawing.Printing;
using System.Windows.Interop;

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

        public static T FindDescendant<T>(this DependencyObject d) where T : DependencyObject
        {
            if (d == null)
                return null;

            int childCount = VisualTreeHelper.GetChildrenCount(d);

            for (var i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, i);
                T result = child as T ?? FindDescendant<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }

    public static class WindowDropShadows
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        /// <summary>
        /// Drops a standard shadow to a WPF Window, even if the window is borderless. Only works with DWM (Windows Vista or newer).
        /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect,
        /// as AllowsTransparency involves a huge performance issue (hardware acceleration is turned off for all the window).
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        public static void DropShadowToWindow(Window window)
        {
            if (!DropShadow(window))
            {
                window.SourceInitialized += new EventHandler(window_SourceInitialized);
            }
        }

        private static void window_SourceInitialized(object sender, EventArgs e)
        {
            Window window = (Window)sender;

            DropShadow(window);

            window.SourceInitialized -= new EventHandler(window_SourceInitialized);
        }

        /// <summary>
        /// The actual method that makes API calls to drop the shadow to the window
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        /// <returns>True if the method succeeded, false if not</returns>
        private static bool DropShadow(Window window)
        {
            try
            {
                WindowInteropHelper helper = new WindowInteropHelper(window);
                int val = 2;
                int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                if (ret1 == 0)
                {
                    Margins m = new Margins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
                    int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                    return ret2 == 0;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                // Probably dwmapi.dll not found (incompatible OS)
                return false;
            }
        }
    }
}

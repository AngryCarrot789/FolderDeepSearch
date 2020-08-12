using FolderDeepSearch.ViewModels;
using System.Windows;
using TheRThemes;

namespace FolderDeepSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            DataContext = new MainViewModel();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                string themeName = ThemesController.CurrentTheme == ThemesController.ThemeTypes.Dark ? "D" : "L";
                Properties.Settings.Default.Width = ActualWidth;
                Properties.Settings.Default.Height = ActualHeight;
                Properties.Settings.Default.ThemeName = themeName;
                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.ThemeName)) ThemesController.SetTheme(ThemesController.ThemeTypes.Light);
                else if (Properties.Settings.Default.ThemeName == "D") ThemesController.SetTheme(ThemesController.ThemeTypes.Dark);
                else if (Properties.Settings.Default.ThemeName == "L") ThemesController.SetTheme(ThemesController.ThemeTypes.Light);
                Width = Properties.Settings.Default.Width;
                Height = Properties.Settings.Default.Height;
            }
            catch { }


            //WindowDropShadows.DropShadowToWindow(this);
        }
    }
}

using FolderDeepSearch.FileOpener;
using FolderDeepSearch.Files;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FolderDeepSearch.Search
{
    /// <summary>
    /// Interaction logic for SearchResultControl.xaml
    /// </summary>
    public partial class SearchResultControl : UserControl
    {
        private SearchResultViewModel Result
        {
            get => this.DataContext as SearchResultViewModel;
            set => this.DataContext = value;
        }

        public SearchResultControl()
        {
            InitializeComponent();
        }

        private bool PathExists()
        {
            return Result != null && (Result.Path.IsFile() || Result.Path.IsDirectory());
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && PathExists())
                Opener.OpenFile(Result.Path);
        }

        private void ContextMenuCommands(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((FrameworkElement)sender).Uid)
                {
                    case "OFI":
                        if (PathExists())
                            Opener.OpenFile(Result.Path);
                        break;
                    case "CP":
                        if (PathExists())
                            Clipboard.SetDataObject(Result.Path);
                        break;
                    case "OF":
                        if (PathExists())
                        {
                            Process.Start("explorer.exe", "/select, \"" + Result.Path + "\"");
                        }
                        break;
                }
            }
            catch { }
        }
    }
}

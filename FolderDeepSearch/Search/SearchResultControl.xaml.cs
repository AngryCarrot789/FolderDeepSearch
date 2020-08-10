using FolderDeepSearch.FileOpener;
using FolderDeepSearch.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FolderDeepSearch.Search
{
    /// <summary>
    /// Interaction logic for SearchResultControl.xaml
    /// </summary>
    public partial class SearchResultControl : UserControl
    {
        SearchResultViewModel Result
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
            return Result.Path.IsFile() || Result.Path.IsDirectory();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && PathExists())
                Opener.OpenFile(Result.Path);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PathExists())
                Opener.OpenFile(Result.Path);
        }
    }
}

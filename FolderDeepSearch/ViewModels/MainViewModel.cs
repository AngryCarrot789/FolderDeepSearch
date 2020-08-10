using FolderDeepSearch.Files;
using FolderDeepSearch.Search;
using FolderDeepSearch.Utilities;
using Ookii.Dialogs.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace FolderDeepSearch.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private SearchPreference _searchPreferences;
        public SearchPreference SearchPreferences
        {
            get => _searchPreferences;
            set => RaisePropertyChanged(ref _searchPreferences, value);
        }

        private bool _isCaseSensitive;
        public bool IsCaseSensitive
        {
            get => _isCaseSensitive;
            set => RaisePropertyChanged(ref _isCaseSensitive, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => RaisePropertyChanged(ref _searchText, value);
        }

        private string _toBeSearchedDirectory;
        public string ToBeSearchedDirectory
        {
            get => _toBeSearchedDirectory;
            set => RaisePropertyChanged(ref _toBeSearchedDirectory, value);
        }

        private bool _searchRecursively;
        public bool SearchRecursively
        {
            get => _searchRecursively;
            set => RaisePropertyChanged(ref _searchRecursively, value);
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set => RaisePropertyChanged(ref _isSearching, value);
        }

        public bool CanSearch { get; set; }

        public ObservableCollection<SearchResultViewModel> FindResults { get; set; }

        public ICommand ClearResultsCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand CancelSearchCommand { get; }

        public MainViewModel()
        {
            FindResults = new ObservableCollection<SearchResultViewModel>();

            CancelSearchCommand = new Command(CancelSearch);
            SelectFolderCommand = new Command(SelectFolderToBeSearched);
            FindCommand = new Command(Find);
            ClearResultsCommand = new Command(ClearResults);
        }

        public void SelectFolderToBeSearched()
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.Description = "Select a directory to be searched";
            if (fbd.ShowDialog() == true)
            {
                if (fbd.SelectedPath.IsDirectory())
                    ToBeSearchedDirectory = fbd.SelectedPath;
                else
                    MessageBox.Show("Seletced folder does not exist");
            }
        }

        public void ClearResults()
        {
            FindResults.Clear();
        }

        public void CancelSearch()
        {
            SetSearchingStatus(false);
            CanSearch = false;
        }

        public void Find()
        {
            CancelSearch();

            CanSearch = true;

            if (!string.IsNullOrEmpty(SearchText))
            {
                if (ToBeSearchedDirectory.IsDirectory())
                {
                    ClearResults();

                    Task.Run(() =>
                    {
                        SetSearchingStatus(true);

                        string searchText = IsCaseSensitive ? SearchText : SearchText.ToLower();

                        if (SearchRecursively)
                        {
                            if (SearchPreferences == SearchPreference.FileContents)
                            {
                                void DirSearch(string toSearchDir)
                                {
                                    foreach (string directory in Directory.GetDirectories(toSearchDir))
                                    {
                                        if (!CanSearch) return;

                                        foreach (string filename in Directory.GetFiles(directory))
                                        {
                                            if (!CanSearch) return;

                                            string fileText = File.ReadAllText(filename);
                                            if (!IsCaseSensitive)
                                                fileText = fileText.ToLower();
                                            if (fileText.Contains(searchText))
                                            {
                                                ResultsFoundAsync(filename, searchText);
                                            }
                                        }

                                        DirSearch(directory);
                                    }
                                }

                                DirSearch(ToBeSearchedDirectory);

                                foreach (string filename in Directory.GetFiles(ToBeSearchedDirectory))
                                {
                                    if (!CanSearch) return;

                                    string fileText = File.ReadAllText(filename);
                                    if (!IsCaseSensitive)
                                        fileText = fileText.ToLower();
                                    if (fileText.Contains(searchText))
                                    {
                                        ResultsFoundAsync(filename, searchText);
                                    }
                                }
                            }

                            if (SearchPreferences == SearchPreference.File)
                            {
                                void DirSearch(string toSearchDir)
                                {
                                    foreach (string directory in Directory.GetDirectories(toSearchDir))
                                    {
                                        if (!CanSearch) return;

                                        foreach (string filename in Directory.GetFiles(directory))
                                        {
                                            if (!CanSearch) return;

                                            if (Path.GetFileNameWithoutExtension(filename).Contains(searchText))
                                                ResultsFoundAsync(filename, searchText);
                                        }

                                        DirSearch(directory);
                                    }
                                }

                                DirSearch(ToBeSearchedDirectory);

                                foreach (string filename in Directory.GetFiles(ToBeSearchedDirectory))
                                {
                                    if (!CanSearch) return;

                                    if (Path.GetFileNameWithoutExtension(filename).Contains(searchText))
                                        ResultsFoundAsync(filename, searchText);
                                }
                            }

                            if (SearchPreferences == SearchPreference.Folder)
                            {
                                void DirSearch(string toSearchDir)
                                {
                                    foreach (string directory in Directory.GetDirectories(toSearchDir))
                                    {
                                        if (!CanSearch) return;

                                        string dirName = directory.GetDirectoryName();
                                        if (dirName.Contains(searchText))
                                            ResultsFoundAsync(directory, searchText);

                                        DirSearch(directory);
                                    }
                                }

                                DirSearch(ToBeSearchedDirectory);
                            }

                            if (SearchPreferences == SearchPreference.All)
                            {
                                void DirSearch(string toSearchDir)
                                {
                                    foreach (string directory in Directory.GetDirectories(toSearchDir))
                                    {
                                        if (!CanSearch) return;

                                        string dirName = directory.GetDirectoryName();
                                        if (dirName.Contains(searchText))
                                            ResultsFoundAsync(directory, searchText);

                                        foreach (string filename in Directory.GetFiles(directory))
                                        {
                                            if (!CanSearch) return;

                                            bool hasFoundFile = false;
                                            if (Path.GetFileNameWithoutExtension(filename).Contains(searchText))
                                            {
                                                ResultsFoundAsync(filename, searchText);
                                                hasFoundFile = true;
                                            }

                                            string fileText = File.ReadAllText(filename);
                                            if (!IsCaseSensitive)
                                                fileText = fileText.ToLower();
                                            if (fileText.Contains(searchText))
                                            {
                                                if (!hasFoundFile)
                                                {
                                                    ResultsFoundAsync(filename, searchText);
                                                }
                                            }
                                        }

                                        DirSearch(directory);
                                    }
                                }

                                DirSearch(ToBeSearchedDirectory);

                                foreach (string filename in Directory.GetFiles(ToBeSearchedDirectory))
                                {
                                    if (!CanSearch) return;

                                    bool hasFoundFile = false;
                                    if (Path.GetFileNameWithoutExtension(filename).Contains(searchText))
                                    {
                                        ResultsFoundAsync(filename, searchText);
                                        hasFoundFile = true;
                                    }

                                    string fileText = File.ReadAllText(filename);
                                    if (!IsCaseSensitive)
                                        fileText = fileText.ToLower();
                                    if (fileText.Contains(searchText))
                                    {
                                        if (!hasFoundFile)
                                        {
                                            ResultsFoundAsync(filename, searchText);
                                        }
                                    }
                                }
                            }
                        }

                        else
                        {
                            switch (SearchPreferences)
                            {

                                case SearchPreference.File:
                                    foreach (string file in Directory.GetFiles(ToBeSearchedDirectory))
                                    {
                                        if (!CanSearch) return;

                                        if (Path.GetFileNameWithoutExtension(file).Contains(searchText))
                                            ResultsFoundAsync(file, searchText);
                                    }
                                    break;

                                case SearchPreference.Folder:
                                    foreach (string folder in Directory.GetDirectories(ToBeSearchedDirectory))
                                    {
                                        if (!CanSearch) return;

                                        string dirName = folder.GetDirectoryName();
                                        if (dirName.Contains(searchText))
                                            ResultsFoundAsync(folder, searchText);
                                    }
                                    break;

                                case SearchPreference.FileContents:
                                    foreach (string file in Directory.GetFiles(ToBeSearchedDirectory))
                                    {
                                        if (!CanSearch) return;

                                        string fileText = File.ReadAllText(file);
                                        if (!IsCaseSensitive)
                                            fileText = fileText.ToLower();
                                        if (fileText.Contains(searchText))
                                        {
                                            ResultsFoundAsync(file, searchText);
                                        }
                                    }
                                    break;

                                case SearchPreference.All:

                                    List<string> foundFiles = new List<string>();

                                    foreach (string folder in Directory.GetDirectories(ToBeSearchedDirectory))
                                    {
                                        if (!CanSearch) return;

                                        string dirName = folder.GetDirectoryName();
                                        if (dirName.Contains(searchText))
                                            ResultsFoundAsync(folder, searchText);
                                    }

                                    foreach (string file in Directory.GetFiles(ToBeSearchedDirectory))
                                    {
                                        if (!CanSearch) return;

                                        bool hasFoundFile = false;
                                        if (Path.GetFileNameWithoutExtension(file).Contains(searchText))
                                        {
                                            ResultsFoundAsync(file, searchText);
                                            hasFoundFile = true;
                                        }

                                        string fileText = File.ReadAllText(file);
                                        if (!IsCaseSensitive)
                                            fileText = fileText.ToLower();
                                        if (fileText.Contains(searchText))
                                        {
                                            if (!hasFoundFile)
                                            {
                                                ResultsFoundAsync(file, searchText);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        SetSearchingStatus(false);
                    });
                }
            }
        }

        public void SetSearchingStatus(bool isSearching)
        {
            IsSearching = isSearching;
        }

        public void ResultsFoundAsync(string path, string selectionText)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SearchResultViewModel searchResult = CreateResultFromPath(path, selectionText);
                if (searchResult != null)
                    AddResult(searchResult);
            });
        }

        public SearchResultViewModel CreateResultFromPath(string path, string selectionText)
        {
            if (path.IsFile())
            {
                return Fetcher.CreateResultFromFile(path, selectionText);
            }

            else if (path.IsDirectory())
            {
                return Fetcher.CreateResultFromFolder(path, selectionText);
            }

            else return null;
        }

        public void AddResult(SearchResultViewModel result)
        {
            FindResults.Add(result);
        }

        public void RemoveResult(SearchResultViewModel result)
        {
            FindResults.Remove(result);
        }
    }
}

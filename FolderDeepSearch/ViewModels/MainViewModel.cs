using FolderDeepSearch.Files;
using FolderDeepSearch.Search;
using FolderDeepSearch.Utilities;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TheRThemes;

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

        private bool _ignoreExtension;
        public bool IgnoreExtension
        {
            get => _ignoreExtension;
            set => RaisePropertyChanged(ref _ignoreExtension, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => RaisePropertyChanged(ref _searchText, value);
        }

        private string _searchStartFolder;
        public string SearchStartFolder
        {
            get => _searchStartFolder;
            set => RaisePropertyChanged(ref _searchStartFolder, value);
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

        private bool _isSorting;
        public bool IsSorting
        {
            get => _isSorting;
            set => RaisePropertyChanged(ref _isSorting, value);
        }

        private int _foldersSearched;
        public int FoldersSearched
        {
            get => _foldersSearched;
            set => RaisePropertyChanged(ref _foldersSearched, value);
        }

        private int _filesSearched;
        public int FilesSearched
        {
            get => _filesSearched;
            set => RaisePropertyChanged(ref _filesSearched, value);
        }

        public bool CanSearch { get; set; }

        const int FileMaxReadSize = 1024; // read the file by chunks of 1KB

        public ObservableCollection<SearchResultViewModel> FindResults { get; set; }

        public ICommand ClearResultsCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand CancelSearchCommand { get; }
        public ICommand SetThemeCommand { get; }
        public ICommand ExportFilesToFolderCommand { get; }
        public ICommand SortFilesCommand { get; }

        public MainViewModel()
        {
            FindResults = new ObservableCollection<SearchResultViewModel>();

            SortFilesCommand = new Command(SortItemsByNameAndType);
            ExportFilesToFolderCommand = new Command(ExportFilesToFolder);
            SetThemeCommand = new CommandParam<string>(SetTheme);
            CancelSearchCommand = new Command(CancelSearch);
            SelectFolderCommand = new Command(SelectFolderToBeSearched);
            FindCommand = new Command(Find);
            ClearResultsCommand = new Command(ClearResults);

            SearchRecursively = true;
            SearchPreferences = SearchPreference.File;
            SearchStartFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SearchText = "";
            IgnoreExtension = true;
        }

        public void SortItemsByNameAndType()
        {
            List<SearchResultViewModel> original = FindResults.ToList();
            IsSorting = true;
            Task.Run(async() =>
            {
                List<SearchResultViewModel> sorted = original.OrderBy(a => a.Name).ToList().OrderBy(a => a.Type).ToList();
                ClearResultsAsync();
                foreach (SearchResultViewModel result in sorted)
                {
                    AddResultAsync(result);
                    // stops lag
                    await Task.Delay(2);
                }

                IsSorting = false;
                original.Clear();
                sorted.Clear();
            });
        }

        public void ExportFilesToFolder()
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.UseDescriptionForTitle = true;
            fbd.Description = "Select a directory for the found FILES (not folders) to be copied to";
            if (fbd.ShowDialog() == true)
            {
                if (fbd.SelectedPath.IsDirectory())
                {
                    foreach(SearchResultViewModel result in FindResults)
                    {
                        if (result.Path.IsFile())
                        {
                            File.Copy(result.Path, Path.Combine(fbd.SelectedPath, result.Name));
                        }
                    }
                }    
                else MessageBox.Show("Seletced folder does not exist");
            }
        }

        public void SetTheme(string themeLetter)
        {
            if (themeLetter == "l") ThemesController.SetTheme(ThemesController.ThemeTypes.Light);
            if (themeLetter == "d") ThemesController.SetTheme(ThemesController.ThemeTypes.Dark);
        }

        public void SelectFolderToBeSearched()
        {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.UseDescriptionForTitle = true;
            fbd.Description = "Select a directory to be searched";
            if (fbd.ShowDialog() == true)
            {
                if (fbd.SelectedPath.IsDirectory())
                    SearchStartFolder = fbd.SelectedPath;
                else
                    MessageBox.Show("Seletced folder does not exist");
            }
        }

        public void ClearSearchedCounters()
        {
            FoldersSearched = 0;
            FilesSearched = 0;
        }

        public void ClearResults()
        {
            FindResults.Clear();
        }

        public void ClearResultsAsync()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                FindResults.Clear();
            });
        }

        public void CancelSearch()
        {
            SetSearchingStatus(false);
            CanSearch = false;
        }

        // This method is absolutely huge ik but i cant
        // be bothered to split it up into smaller ones lol
        public void Find()
        {
            try
            {
                CancelSearch();
                ClearSearchedCounters();

                CanSearch = true;

                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (SearchStartFolder.IsDirectory())
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

                                                ReadAndSearchFileAsync(filename, searchText);
                                                FilesSearched++;
                                            }
                                            FoldersSearched++;

                                            DirSearch(directory);
                                        }
                                    }

                                    DirSearch(SearchStartFolder);

                                    foreach (string filename in Directory.GetFiles(SearchStartFolder))
                                    {
                                        if (!CanSearch) return;

                                        ReadAndSearchFileAsync(filename, searchText);

                                        FilesSearched++;
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

                                                if (GetFileName(filename).Contains(searchText))
                                                    ResultsFoundAsync(filename, searchText);

                                                FilesSearched++;
                                            }
                                            FoldersSearched++;

                                            DirSearch(directory);
                                        }
                                    }

                                    DirSearch(SearchStartFolder);

                                    foreach (string filename in Directory.GetFiles(SearchStartFolder))
                                    {
                                        if (!CanSearch) return;

                                        if (GetFileName(filename).Contains(searchText))
                                            ResultsFoundAsync(filename, searchText);

                                        FilesSearched++;
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
                                            FoldersSearched++;

                                            DirSearch(directory);
                                        }
                                    }

                                    DirSearch(SearchStartFolder);
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
                                                if (GetFileName(filename).Contains(searchText))
                                                {
                                                    ResultsFoundAsync(filename, searchText);
                                                    hasFoundFile = true;
                                                }

                                                if (!hasFoundFile)
                                                {
                                                    ReadAndSearchFileAsync(filename, searchText);
                                                }

                                                FilesSearched++;
                                            }
                                            FoldersSearched++;

                                            DirSearch(directory);
                                        }
                                    }

                                    DirSearch(SearchStartFolder);

                                    foreach (string filename in Directory.GetFiles(SearchStartFolder))
                                    {
                                        if (!CanSearch) return;

                                        bool hasFoundFile = false;
                                        if (GetFileName(filename).Contains(searchText))
                                        {
                                            ResultsFoundAsync(filename, searchText);
                                            hasFoundFile = true;
                                        }

                                        if (!hasFoundFile)
                                        {
                                            ReadAndSearchFileAsync(filename, searchText);
                                        }

                                        FilesSearched++;
                                    }
                                }
                            }

                            else
                            {
                                switch (SearchPreferences)
                                {
                                    case SearchPreference.File:
                                        foreach (string file in Directory.GetFiles(SearchStartFolder))
                                        {
                                            if (!CanSearch) return;

                                            if (GetFileName(file).Contains(searchText))
                                                ResultsFoundAsync(file, searchText);

                                            FilesSearched++;
                                        }
                                        break;

                                    case SearchPreference.Folder:
                                        foreach (string folder in Directory.GetDirectories(SearchStartFolder))
                                        {
                                            if (!CanSearch) return;

                                            string dirName = folder.GetDirectoryName();
                                            if (dirName.Contains(searchText))
                                                ResultsFoundAsync(folder, searchText);

                                            FoldersSearched++;
                                        }
                                        break;

                                    case SearchPreference.FileContents:
                                        foreach (string file in Directory.GetFiles(SearchStartFolder))
                                        {
                                            if (!CanSearch) return;

                                            ReadAndSearchFileAsync(file, searchText);

                                            FilesSearched++;
                                        }
                                        break;

                                    case SearchPreference.All:
                                        List<string> foundFiles = new List<string>();

                                        foreach (string folder in Directory.GetDirectories(SearchStartFolder))
                                        {
                                            if (!CanSearch) return;

                                            string dirName = folder.GetDirectoryName();
                                            if (dirName.Contains(searchText))
                                                ResultsFoundAsync(folder, searchText);

                                            FoldersSearched++;
                                        }

                                        foreach (string file in Directory.GetFiles(SearchStartFolder))
                                        {
                                            if (!CanSearch) return;

                                            bool hasFoundFile = false;
                                            if (GetFileName(file).Contains(searchText))
                                            {
                                                ResultsFoundAsync(file, searchText);
                                                hasFoundFile = true;
                                            }

                                            if (!hasFoundFile)
                                            {
                                                ReadAndSearchFileAsync(file, searchText);
                                            }

                                            FilesSearched++;
                                        }
                                        break;
                                }
                            }

                            SetSearchingStatus(false);
                        });
                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message + " -- Cancelling search."); CancelSearch(); }
        }

        public string GetFileName(string original)
        {
            if (IgnoreExtension)
                return Path.GetFileNameWithoutExtension(original);
            else
                return Path.GetFileName(original);
        }

        public void ReadAndSearchFileAsync(string file, string searchText)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(file))
                {
                    byte[] buffer = new byte[FileMaxReadSize];
                    while (fileStream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        if (!CanSearch) return;
                        string text = Encoding.ASCII.GetString(buffer);
                        if ((IsCaseSensitive ? text : text.ToLower()).Contains(searchText))
                        {
                            ResultsFoundAsync(file, searchText);
                            break;
                        }
                    }
                }
            }
            catch(Exception e) { MessageBox.Show(e.Message + " -- Cancelling search."); CancelSearch(); }
        }

        public void SetSearchingStatus(bool isSearching)
        {
            IsSearching = isSearching;
        }

        public void ResultsFoundAsync(string path, string selectionText)
        {
            SearchResultViewModel searchResult = CreateResultFromPath(path, selectionText);
            if (searchResult != null)
                AddResultAsync(searchResult);
        }

        public SearchResultViewModel CreateResultFromPath(string path, string selectionText)
        {
            if (path.IsFile())
                return Fetcher.CreateResultFromFile(path, selectionText);

            else if (path.IsDirectory())
                return Fetcher.CreateResultFromFolder(path, selectionText);

            else return null;
        }

        public void AddResult(SearchResultViewModel result)
        {
            FindResults.Add(result);
        }

        public void AddResultAsync(SearchResultViewModel result)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                AddResult(result);
            });
        }

        public void RemoveResult(SearchResultViewModel result)
        {
            FindResults.Remove(result);
        }
    }
}

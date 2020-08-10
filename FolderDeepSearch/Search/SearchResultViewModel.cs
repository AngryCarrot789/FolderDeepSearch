using FolderDeepSearch.Files;
using FolderDeepSearch.Utilities;
using System.Drawing;

namespace FolderDeepSearch.Search
{
    public class SearchResultViewModel : BaseViewModel
    {
        private Icon _icon;
        public Icon Icon
        {
            get => _icon;
            set => RaisePropertyChanged(ref _icon, value);
        }

        private string _selectedText;
        public string SelectedText
        {
            get => _selectedText;
            set => RaisePropertyChanged(ref _selectedText, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => RaisePropertyChanged(ref _name, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => RaisePropertyChanged(ref _path, value);
        }

        private FileType _type;
        public FileType Type
        {
            get => _type;
            set => RaisePropertyChanged(ref _type, value);
        }

        private long _sizeBytes;
        public long SizeBytes
        {
            get => _sizeBytes;
            set => RaisePropertyChanged(ref _sizeBytes, value);
        }

        public bool IsFile => Type == FileType.File;
        public bool IsFolder => Type == FileType.Folder;
    }
}

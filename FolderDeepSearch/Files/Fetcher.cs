using FolderDeepSearch.Helpers;
using FolderDeepSearch.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FolderDeepSearch.Files
{
    public static class Fetcher
    {
        public static SearchResultViewModel CreateResultFromFile(string path, string selectionText)
        {
            if (!path.IsFile())
                return null;

            try
            {
                FileInfo fInfo = new FileInfo(path);
                SearchResultViewModel srModel = new SearchResultViewModel()
                {
                    Icon = IconHelper.GetIconOfFile(path, false, false),
                    SelectedText = selectionText,
                    Name = fInfo.Name,
                    Path = fInfo.FullName,
                    Type = FileType.File,
                    SizeBytes = fInfo.Length
                };

                return srModel;
            }
            catch (Exception e)
            {
                //MessageBox.Show($"Failed to create file: {e.Message}", "Error"); 
                return null;
            }
        }

        public static SearchResultViewModel CreateResultFromFolder(string folderPath, string selectionText)
        {
            if (!folderPath.IsDirectory())
                return null;

            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(folderPath);
                SearchResultViewModel dModel = new SearchResultViewModel()
                {
                    Icon = IconHelper.GetIconOfFile(folderPath, false, true),
                    SelectedText = selectionText,
                    Name = dInfo.Name,
                    Path = dInfo.FullName,
                    Type = FileType.Folder,
                    SizeBytes = long.MaxValue
                };

                return dModel;
            }
            catch (Exception e)
            {
                //MessageBox.Show($"Failed to create folder: {e.Message}", "Error");
                return null;
            }
        }
    }
}

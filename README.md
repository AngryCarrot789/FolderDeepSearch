# FolderDeepSearch
A simple program for deep searching through folders, files and file contents to search for a specific string of text. 

Can select between searching only in the selected folder, or search recursively though every single subdirectory.

Searching contents uses a filestream to constantly read a 1kb chunk from the file. this is better than loading the entire file into memory and then searching that.

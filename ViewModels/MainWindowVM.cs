using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using MEdit.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using System;

namespace MEdit.ViewModels
{
    public class FileMetaData : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long Size {get; set;}
        public DateTime DateCreated {get; set;}
        public DateTime DateAccessed {get; set;}
        public DateTime DateModified {get; set;}

        public string DateCreatedString {get; set;}
        public string DateAccessedString {get; set;}
        public string DateModifiedString {get; set;}

        public string ProductName {get; set;}
        public string ProductVersion {get; set;}
        public string CompanyName {get; set;}
        public string Description {get; set;}

        public FileMetaData(string filePath)
        {
            Path = filePath;
            UpdateMetadata();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateMetadata()
        {
            if (!File.Exists(Path))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            var fileInfo = new FileInfo(Path);
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Path);

            Name = fileInfo.Name;
            Type = "Unknown";
            Size = fileInfo.Length;
            DateCreated = fileInfo.CreationTime;
            DateAccessed = fileInfo.LastAccessTime;
            DateModified = fileInfo.LastWriteTime;

            DateCreatedString = fileInfo.CreationTime.ToLongTimeString();
            DateAccessedString = fileInfo.LastAccessTime.ToLongTimeString();
            DateModifiedString = fileInfo.LastWriteTime.ToLongTimeString();

            // Adding version-specific metadata if available
            ProductName = fileVersionInfo.ProductName ?? string.Empty;
            ProductVersion = fileVersionInfo.ProductVersion ?? string.Empty;
            CompanyName = fileVersionInfo.CompanyName ?? string.Empty;
            Description = fileVersionInfo.FileDescription ?? string.Empty;
        }
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
}

    public class MainWindowVM : INotifyPropertyChanged
    {
        public ICommand OnDropCommand { get; private set; }
        public ICommand OnDragOverCommand { get; private set; }
        public ICommand OnBrowseCommand { get; private set; }
        public ICommand SortCommand { get; private set; }
        public ICommand UpdateCommand {get;}
        public ICommand ExitCommand { get; }


        private ObservableCollection<string> _items;

        private bool ascendingSort = false;
        public ObservableCollection<string> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private Dictionary<string, FileMetaData> ItemMetaData = new Dictionary<string, FileMetaData>();

        private string _selectedItem;
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    if (ItemMetaData.ContainsKey(_selectedItem))
                    {
                        SelectedFileMetaData = ItemMetaData[_selectedItem];
                        SelectedFileMetaData.UpdateMetadata();
                    }
                }
            }
        }

        private FileMetaData _selectedFileMetaData;
        public FileMetaData SelectedFileMetaData
        {
            get => _selectedFileMetaData;
            set
            {
                if (_selectedFileMetaData != value)
                {
                    _selectedFileMetaData = value;
                    OnPropertyChanged(nameof(SelectedFileMetaData));
                }
            }
        }

        public MainWindowVM()
        {
            OnBrowseCommand = new RelayCommand(ExecuteBrowse);
            OnDropCommand = new RelayCommand(DropExecute);
            OnDragOverCommand = new RelayCommand(DragOverExecute);
            SortCommand = new RelayCommand(SortItems);
            UpdateCommand = new RelayCommand(UpdateFields);
            ExitCommand = new RelayCommand(ExitApplication);

            Items = new ObservableCollection<string>();
        }

        void AddItemToFiles(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            if (!ItemMetaData.ContainsKey(fileName))
            {
                Items.Add(fileName);
                ItemMetaData.Add(fileName, new FileMetaData(filePath));
            }
        }

        void RemoveItemFromFiles(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            if (ItemMetaData.ContainsKey(fileName))
            {
                ItemMetaData.Remove(filePath);
                Items.Remove(System.IO.Path.GetFileName(filePath));
            }
        }
        private void ExecuteBrowse(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;  // Enable multiple file selection.

            if (openFileDialog.ShowDialog() == true)
            {
                List<string> filePaths = new List<string>(openFileDialog.FileNames);
                List<string> fileNames = new List<string>();

                foreach (string fullPath in filePaths)
                    AddItemToFiles(fullPath);
            }
        }

        private void DropExecute(object parameter)
        {
            string[] files = parameter as string[];
            if (files != null)
            {
                foreach (var file in files)
                    AddItemToFiles(file);
            }
        }

        private void SortItems(object parameter)
        {
            if (ascendingSort)
            {
                var sortedList = Items.OrderBy(item => item).ToList();
                Items = new ObservableCollection<string>(sortedList);
            }
            else
            {
                var sortedList = Items.OrderByDescending(item => item).ToList();
                Items = new ObservableCollection<string>(sortedList);
            }
            ascendingSort = !ascendingSort;
        }

        private void UpdateFields(object parameter)
        {
            Application.Current.Shutdown(); // Exits the application
        }
        private void ExitApplication(object parameter)
        {
            Application.Current.Shutdown(); // Exits the application
        }
        private void DragOverExecute(object parameter)
        {
            var e = parameter as DragEventArgs;
            if (e != null)
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

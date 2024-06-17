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
using System.Windows.Media;

namespace MEdit.ViewModels
{
    public class FileMetaData : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long Size {get; set;}
        public long Id { get; set; }
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
        private bool _ischanged = false;
        public bool IsChanged
        {
            get
            {
                return _ischanged;
            }
            set
            {
                if (value != _ischanged)
                {
                    _ischanged = value;
                }
                OnPropertyChanged(nameof(IsChanged));
            }
        }

        public FileMetaData(string filePath, long id)
        {

            Id = id;
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

    public class MenuItem
    {
        public string Header { get; set; }

        public bool IsCheckable { get; set; }
        public bool IsEnabled { get; set; }
        private bool _ischecked;
        public bool IsChecked
        {
            get
            {
                return _ischecked;
            }
            set
            {
                if (value != _ischecked)
                {
                    _ischecked = value;
                    Command?.Execute(this);
                }
            }
        }

        public ICommand Command { get; set; }

        public MenuItem(string header, ICommand command, bool isEnabled = true, bool isCheckable = true)
        {
            Header = header;
            Command = command;
            IsCheckable = isCheckable;
            IsEnabled = isEnabled;
            IsChecked = isCheckable;
        }
    }
    
    public class MainWindowVM : INotifyPropertyChanged
    {
        public ICommand OnDropCommand { get; private set; }
        public ICommand OnDragOverCommand { get; private set; }
        public ICommand BrowseCommand { get; private set; }
        public ICommand UpdateCommand {get;}
        public ICommand ExitCommand { get; }
        public ICommand UpdateColumnVisibilityCommand { get; }

        private Visibility _sizeVisibility;
        public Visibility SizeVisibility
        {
            get { return _sizeVisibility; }
            set
            {
                _sizeVisibility = value;
                OnPropertyChanged(nameof(SizeVisibility));
            }
        }
        
        private Visibility _dateModifiedVisibility;
        public Visibility DateModifiedVisibility
        {
            get { return _dateModifiedVisibility; }
            set
            {
                _dateModifiedVisibility = value;
                OnPropertyChanged(nameof(DateModifiedVisibility));
            }
        }

        public Brush HeaderColor = Brushes.Pink;
        private ObservableCollection<FileMetaData> _items;

        public ObservableCollection<FileMetaData> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ObservableCollection<MenuItem> MenuItems { get; set; }
        public ObservableCollection<MenuItem> ElementNameMenuItems { get; set; }
        
        public Dictionary<string, FileMetaData> OriginalInfo = new Dictionary<string, FileMetaData>();

        private FileMetaData _selectedItem;
        public FileMetaData SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public MainWindow View;
        public MainWindowVM(MainWindow view)
        {
            View = view;
            BrowseCommand = new RelayCommand(ExecuteBrowse);
            OnDropCommand = new RelayCommand(DropExecute);
            OnDragOverCommand = new RelayCommand(DragOverExecute);
            UpdateCommand = new RelayCommand(UpdateFields);
            ExitCommand = new RelayCommand(ExitApplication);
            UpdateColumnVisibilityCommand = new RelayCommand(UpdateColumnVisibility);

            Items = new ObservableCollection<FileMetaData>();
            SizeVisibility = Visibility.Collapsed;

            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem("Name", null, false, false),
                new MenuItem("Size", UpdateColumnVisibilityCommand),
                new MenuItem("Date Modified", UpdateColumnVisibilityCommand)
            };

            ElementNameMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem("Edit", new RelayCommand(obj => View.DataGrid.BeginEdit())),
                new MenuItem("Remove", new RelayCommand(obj => RemoveFile(SelectedItem.Path)), false, false)
            };
        }

        static long Id = 0;
        void AddItemToFiles(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            
            if (!OriginalInfo.ContainsKey(fileName))
            {
                //Not same instance
                OriginalInfo.Add(fileName, new FileMetaData(filePath, Id));
                Items.Add(new FileMetaData(filePath, Id++));
            }
        }

        void RemoveFile(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);

            if (OriginalInfo.ContainsKey(fileName))
            {
                OriginalInfo.Remove(fileName);
                var item = Items.FirstOrDefault((x) => x.Path == filePath);
                Items.Remove(item);
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
        private void UpdateColumnVisibility(object parameter)
        {
            if (parameter is MenuItem item)
            {
                if (item.Header == "Size")
                    SizeVisibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                if (item.Header == "Date Modified")
                    DateModifiedVisibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
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

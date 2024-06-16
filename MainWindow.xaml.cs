using MEdit.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowVM(this);
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Textblock.Visibility = Visibility.Hidden;
            DataGrid.Opacity = 1;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var viewModel = DataContext as MainWindowVM;
                if (viewModel != null && viewModel.OnDropCommand.CanExecute(files))
                {
                    viewModel.OnDropCommand.Execute(files);
                }
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            Textblock.Visibility = Visibility.Visible;
            DataGrid.Opacity = 0.3;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            Textblock.Visibility = Visibility.Hidden;
            DataGrid.Opacity = 1;
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control && e.Key == System.Windows.Input.Key.O)
            {
                var viewModel = DataContext as MainWindowVM;
                viewModel.BrowseCommand.Execute(null);

                e.Handled = true; // Mark the event as handled
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                var viewModel = DataContext as MainWindowVM;
                viewModel.BrowseCommand.Execute(null);
            }
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingEventArgs is System.Windows.Input.MouseButtonEventArgs)
                DataGrid.CancelEdit();
        }

        private void DataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.None && e.Key == System.Windows.Input.Key.F2)
            {
                DataGrid.BeginEdit();
            }
        }

        private void DataGrid_CurrentCellChanged(object sender, System.EventArgs e)
        {
            var viewModel = DataContext as MainWindowVM;
            var datagrid = sender as DataGrid;
            var selItem = datagrid.SelectedValue as FileMetaData;

            if (selItem == null)
                return;

            foreach (var item in viewModel.Items)
            {
                if (selItem != null && selItem.Id == item.Id)
                {
                    FileMetaData org = null;
                    foreach (var kvp in viewModel.OriginalInfo)
                    {
                        if (kvp.Value.Id == item.Id)
                            org = kvp.Value;
                    }

                    if (selItem.Name == org.Name)
                        item.IsChanged = false;
                    else
                        item.IsChanged = true;
                }
            }
        }
    }
}

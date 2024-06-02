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
            DataContext = new MainWindowVM();
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            //Rect.Visibility = Visibility.Hidden;
            Textblock.Visibility = Visibility.Hidden;
            StackPanelContent.Opacity = 1;
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
            //Rect.Visibility = Visibility.Visible;
            Textblock.Visibility = Visibility.Visible;
            StackPanelContent.Opacity = 0.1;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            //Rect.Visibility = Visibility.Hidden;
            Textblock.Visibility = Visibility.Hidden;
            StackPanelContent.Opacity = 1;
        }
    }
}

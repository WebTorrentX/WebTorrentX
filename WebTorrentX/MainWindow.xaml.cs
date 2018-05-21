using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WebTorrentX.ViewModels;

namespace WebTorrentX
{

    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Page currentPage;
        public Page CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CurrentPage = new DownloadPage();
        }

        private void OpenTorrent()
        {
            if (CurrentPage is DownloadPage)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    (CurrentPage as DownloadPage).LoadTorrent(openFileDialog.FileName);
                }
            }
                
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {            
            //downloadController.Dispose();
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.O:
                        OpenTorrent();
                        break;
                    case Key.U:
                        break;
                    case Key.W:
                        Close();
                        break;
                    default: break;
                }
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenTorrent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}

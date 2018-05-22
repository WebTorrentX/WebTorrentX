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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MainFrame.Content = new DownloadPage();
        }

        private void OpenTorrent()
        {
            if (MainFrame.Content is DownloadPage)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    (MainFrame.Content as DownloadPage).LoadTorrent(openFileDialog.FileName);
                }
            }
                
        }

        private void Fullscreen()
        {
            if (MainFrame.Content is PlayerPage)
                (MainFrame.Content as PlayerPage).Fullscreen();
        }

        private void GoBack()
        {
            if (MainFrame.Content is PlayerPage)
            {
                (MainFrame.Content as PlayerPage).GoBack();
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
            if (e.Key == Key.Escape)
            {
                (MainFrame.Content as PlayerPage).GoBack();
            }
            if (e.Key == Key.F11)
            {
                Fullscreen();
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

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Fullscreen();
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            (MainFrame.Content as PlayerPage).GoBack();
        }
    }
}

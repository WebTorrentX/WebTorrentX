using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
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

        public bool IsDownloadPage
        {
            get
            {
                return MainFrame.Content is DownloadPage;
            }
        }

        public bool IsPlayerPage
        {
            get
            {
                return MainFrame.Content is PlayerPage;
            }
        }

        public bool CanGoBack
        {
            get
            {
                return MainFrame.NavigationService.CanGoBack;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Location == string.Empty)
            {
                Properties.Settings.Default.Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                Properties.Settings.Default.Save();
            }
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
            else if (MainFrame.Content is SettingsPage)
            {
                (MainFrame.Content as SettingsPage).GoBack();
            }
        }

        private void PlayPause()
        {
            if (MainFrame.Content is PlayerPage)
                (MainFrame.Content as PlayerPage).PlayPause();
        }

        private void IncreaseVolume()
        {
            if (MainFrame.Content is PlayerPage)
                (MainFrame.Content as PlayerPage).IncreaseVolume();
        }

        private void DecreaseVolume()
        {
            if (MainFrame.Content is PlayerPage)
                (MainFrame.Content as PlayerPage).DecreaseVolume();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {            
            App.downloadController.Dispose();
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
                    case Key.Up:
                        IncreaseVolume();
                        break;
                    case Key.Down:
                        DecreaseVolume();
                        break;
                    default: break;
                }
            }
            switch (e.Key)
            {
                case Key.Escape:
                    GoBack();
                    break;
                case Key.F11:
                    Fullscreen();
                    break;
                case Key.Space:
                    PlayPause();
                    break;
                default: break;
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

        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is SettingsPage == false)
                MainFrame.NavigationService.Navigate(new SettingsPage());
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Fullscreen();
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        private void IncreaseVolumeButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseVolume();
        }

        private void DecreaseVolumeButton_Click(object sender, RoutedEventArgs e)
        {
            DecreaseVolume();
        }

        private void IncreaseSpeedButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DecreaseSpeedButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSubtitlesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PauseAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DownloadPage)
                (MainFrame.Content as DownloadPage).PauseAll();
        }

        private void ResumeAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is DownloadPage)
                (MainFrame.Content as DownloadPage).ResumeAll();
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            OnPropertyChanged(nameof(IsDownloadPage));
            OnPropertyChanged(nameof(IsPlayerPage));
            OnPropertyChanged(nameof(CanGoBack));
        }
    }
}

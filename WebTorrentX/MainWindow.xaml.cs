using System;
using System.ComponentModel;
using System.IO;
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
            if (Properties.Settings.Default.Location == string.Empty)
            {
                Properties.Settings.Default.Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                Properties.Settings.Default.Save();
            }
            DataContext = this;
            MainFrame.Content = new DownloadPage();
            if (Application.Current.Properties["openfile"] != null)
            {
                (MainFrame.Content as DownloadPage).LoadTorrent(Application.Current.Properties["openfile"].ToString());
            }
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
                GoBack();
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

        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
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

        }

        private void IncreaseVolumeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DecreaseVolumeButton_Click(object sender, RoutedEventArgs e)
        {

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

    }
}

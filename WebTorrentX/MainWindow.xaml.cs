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

        public bool IsAddTorrentLinkPage
        {
            get
            {
                return MainFrame.Content is AddTorrentLinkPage;
            }
        }

        public bool CanGoBack
        {
            get
            {
                return MainFrame.NavigationService.CanGoBack;
            }
        }

        private Visibility hideControls;
        public Visibility HideControls
        {
            get
            {
                return hideControls;
            }
            set
            {
                hideControls = value;
                OnPropertyChanged(nameof(HideControls));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(Properties.Settings.Default.Location))
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

        private void OpenAddress()
        {
            if (MainFrame.Content is DownloadPage)
            {
                (MainFrame.Content as DownloadPage).NavigationService.Navigate(new AddTorrentLinkPage());
            }
        }

        private void Undo()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).Undo();
            }
        }

        private void Redo()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).Redo();
            }
        }

        private void Cut()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).Cut();
            }
        }

        private void Copy()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).Copy();
            }
        }

        private void Paste()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).Paste();
            }
        }

        private void Delete()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).Delete();
            }
        }

        private void SelectAll()
        {
            if (MainFrame.Content is AddTorrentLinkPage)
            {
                (MainFrame.Content as AddTorrentLinkPage).SelectAll();
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

        private void IncreaseSpeed()
        {
            if (MainFrame.Content is PlayerPage)
                (MainFrame.Content as PlayerPage).IncreaseSpeed();
        }

        private void DecreaseSpeed()
        {
            if (MainFrame.Content is PlayerPage)
                (MainFrame.Content as PlayerPage).DecreaseSpeed();
        }

        private void AddSubtitle()
        {
            if (MainFrame.Content is PlayerPage)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    (MainFrame.Content as PlayerPage).LoadSubtitles(openFileDialog.FileName);
                }
            }
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
                        OpenAddress();
                        break;
                    case Key.W:
                        Close();
                        break;
                    case Key.Z:
                        Undo();
                        break;
                    case Key.Y:
                        Redo();
                        break;
                    case Key.X:
                        Cut();
                        break;
                    case Key.C:
                        Copy();
                        break;
                    case Key.V:
                        Paste();
                        break;
                    case Key.A:
                        SelectAll();
                        break;
                    case Key.Up:
                        IncreaseVolume();
                        break;
                    case Key.Down:
                        DecreaseVolume();
                        break;
                    case Key.Left:
                        DecreaseSpeed();
                        break;
                    case Key.Right:
                        IncreaseSpeed();
                        break;
                    default: break;
                }
            }
            switch (e.Key)
            {
                case Key.Escape:
                    if (MainFrame.Content is PlayerPage)
                    {
                        if ((MainFrame.Content as PlayerPage).IsFullscreen)
                            Fullscreen();
                        else GoBack();
                    }
                    else GoBack();
                    break;
                case Key.F11:
                    Fullscreen();
                    break;
                case Key.Space:
                    PlayPause();
                    break;
                case Key.Delete:
                    Delete();
                    break;
                default: break;
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenTorrent();
        }


        private void OpenAddressButton_Click(object sender, RoutedEventArgs e)
        {
            OpenAddress();
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
            IncreaseSpeed();
        }

        private void DecreaseSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            DecreaseSpeed();
        }

        private void AddSubtitlesButton_Click(object sender, RoutedEventArgs e)
        {
            AddSubtitle();
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
            OnPropertyChanged(nameof(IsAddTorrentLinkPage));
            OnPropertyChanged(nameof(CanGoBack));
        }

        private void ContributeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/WebTorrentX/WebTorrentX/");
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/WebTorrentX/WebTorrentX/issues");
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }
    }
}

using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WebTorrentX.Controllers;
using WebTorrentX.Models;

namespace WebTorrentX.ViewModels
{

    public sealed partial class DownloadPage : Page
    {

        private DownloadController downloadController;

        private FileSystemWatcher watcher;

        internal ObservableCollection<Torrent> TorrentSource
        {
            get { return downloadController.Torrents; }
        }

        public DownloadPage()
        {
            InitializeComponent();
            downloadController = new DownloadController();
            downloadController.Error += (sender, message) =>
            {
                MessageBox.Show(message, "WebTorrentX", MessageBoxButton.OK);
            };
            DataContext = this;
            TorrentListView.ItemsSource = TorrentSource;
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.WatchForNewFiles)
            {
                watcher = new FileSystemWatcher();
                watcher.Path = Properties.Settings.Default.WatchFolder;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.torrent";
                watcher.Created += Watcher_Created;
                watcher.EnableRaisingEvents = true;
            }
            else if (watcher != null)
                watcher.Dispose();
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                downloadController.LoadTorrent(e.FullPath);
            });
        }

        public void LoadTorrent(string filename)
        {
            downloadController.LoadTorrent(filename);
        }

        public void PauseAll()
        {
            foreach (var torrent in downloadController.Torrents)
                torrent.IsDownloading = false;
        }

        public void ResumeAll()
        {
            foreach (var torrent in downloadController.Torrents)
                torrent.IsDownloading = true;
        }

        private void RemoveTorrentButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Remove this torrent?", "WebTorrentX", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if ((sender as Button).Tag is Torrent)
                {
                    ((sender as Button).Tag as Torrent).Remove();
                    downloadController.Torrents.Remove((sender as Button).Tag as Torrent);
                }
            }

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag is Torrent)
            {
                Application.Current.Properties["filename"] = Path.Combine(downloadController.DownloadPath, ((sender as Button).Tag as Torrent).Name);
                NavigationService.Navigate(new PlayerPage());
            }
                
        }

        private void TorrentListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for (int i = 0; i < files.Length; i++)
                    LoadTorrent(files[i]);
            }
        }

    }
}

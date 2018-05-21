using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WebTorrentX.Controllers;
using WebTorrentX.Models;

namespace WebTorrentX.ViewModels
{

    public sealed partial class DownloadPage : Page
    {

        private DownloadController downloadController;

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

        public void LoadTorrent(string filename)
        {
            downloadController.LoadTorrent(filename);
        }

        private void RemoveTorrentButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Remove this torrent?", "WebTorrentX", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                int index = int.Parse((sender as Button).Tag.ToString());
                if (index >= 0)
                {
                    downloadController.Torrents[index].Remove();
                    downloadController.Torrents.RemoveAt(index);
                }
            }

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            int index = int.Parse((sender as Button).Tag.ToString());
            if (index >= 0)
            {
                Application.Current.Properties["filename"] = Path.Combine(downloadController.DownloadPath, downloadController.Torrents[index].Name);
                NavigationService.Navigate(new PlayerPage());
            }
                
        }
    }
}

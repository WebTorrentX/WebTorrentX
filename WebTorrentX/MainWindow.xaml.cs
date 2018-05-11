using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WebTorrentX
{

    public sealed partial class MainWindow : Window
    {
        internal Downloader Downloader { get; set; } = new Downloader();

        internal ObservableCollection<Torrent> TorrentSource
        {
            get { return Downloader.Torrents; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            TorrentListView.ItemsSource = TorrentSource;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();            
            if (openFileDialog.ShowDialog() == true)
            {
                Downloader.LoadTorrent(openFileDialog.FileName);
            }
        }

        private void RemoveTorrentButton_Click(object sender, RoutedEventArgs e)
        {
            /*int index = TorrentListView.SelectedIndex;
            if (index >= 0)
            {
                Downloader.Torrents[index].Dispose();
                Downloader.Torrents.RemoveAt(index);
            }*/
        }
    }
}

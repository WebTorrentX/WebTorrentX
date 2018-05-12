using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WebTorrentX.Controllers;
using WebTorrentX.Models;

namespace WebTorrentX
{

    public sealed partial class MainWindow : Window
    {
        private DownloadController downloadController;

        internal ObservableCollection<Torrent> TorrentSource
        {
            get { return downloadController.Torrents; }
        }

        public MainWindow()
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

        private void OpenTorrent()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                downloadController.LoadTorrent(openFileDialog.FileName);
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenTorrent();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            downloadController.Dispose();
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
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ragnar;
using System.Collections.ObjectModel;

namespace WebTorrentX
{
    internal sealed class Downloader
    {

        private int minPort = 2000;
        private int maxPort = 2500;
        public Session session = new Session();

        public ObservableCollection<Torrent> Torrents { get; private set; } = new ObservableCollection<Torrent>();

        public string DownloadPath { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        public void LoadTorrent(string filename)
        {
            var addParams = new AddTorrentParams
            {
                SavePath = DownloadPath,
                TorrentInfo = new TorrentInfo(filename)
            };
            addParams.TorrentInfo = new TorrentInfo(filename);            
            session.ListenOn(minPort, maxPort);
            var torrent = Torrent.Create(addParams, session);          
            Torrents.Add(torrent);
            
        }

    }
}

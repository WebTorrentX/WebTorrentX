using System;
using System.IO;
using System.Linq;
using Ragnar;
using System.Collections.ObjectModel;
using WebTorrentX.Models;

namespace WebTorrentX.Controllers
{
    internal sealed class DownloadController : IDisposable
    {

        private const int minPort = 2000;
        private const int maxPort = 2500;
        private readonly string sesStateFile;
        private readonly string torrentDir;
        private readonly string linksFile;


        private Session session;

        public ObservableCollection<Torrent> Torrents { get; private set; }

        public event EventHandler<string> Error;

        public DownloadController()
        {
            sesStateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".ses_state");
            torrentDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".torrents");
            linksFile = Path.Combine(torrentDir, "download.links");
            session = new Session();
            LoadSessionState();            
            Torrents = new ObservableCollection<Torrent>();
            LoadActiveTorrents();
        }

        private void LoadActiveTorrents()
        {
            if (Directory.Exists(torrentDir))
            {
                var files = Directory.GetFiles(torrentDir);
                foreach (var file in files)
                {
                    if (file.EndsWith(".torrent"))
                    {
                        var addParams = new AddTorrentParams
                        {
                            SavePath = Properties.Settings.Default.Location,
                            TorrentInfo = new TorrentInfo(file)
                        };
                        var torrent = Torrent.Create(addParams, session);
                        Torrents.Add(torrent);
                    }
                }
                if (File.Exists(linksFile))
                {
                    var links = File.ReadLines(linksFile);
                    foreach (var link in links)
                    {
                        var addParams = new AddTorrentParams
                        {
                            SavePath = Properties.Settings.Default.Location,
                            Url = link
                        };
                        var torrent = Torrent.Create(addParams, session);
                        Torrents.Add(torrent);
                    }
                }                
            }
        }

        private void LoadSessionState()
        {
            if (File.Exists(sesStateFile))
            {
                session.LoadState(File.ReadAllBytes(sesStateFile));
            }
            else session.ListenOn(minPort, maxPort);
        }

        private void SaveSessionState()
        {
            if (Torrents.Count > 0) File.WriteAllBytes(sesStateFile, session.SaveState());               
        }

        public void LoadMagnet(string link)
        {
            if (!Directory.Exists(torrentDir))
                Directory.CreateDirectory(torrentDir);
            try
            {
                if (link.StartsWith(@"magnet:?xt=urn:btih:") || link.StartsWith(@"http://") || link.StartsWith(@"https://"))
                {
                    var addParams = new AddTorrentParams
                    {
                        SavePath = Properties.Settings.Default.Location,
                        Url = link
                    };
                    var torrent = Torrent.Create(addParams, session);
                    var result = from t in Torrents where t.InfoHash.ToHex() == torrent.InfoHash.ToHex() select t;
                    if (result == null || result.Count() == 0)
                    {
                        Torrents.Add(torrent);
                        if (File.Exists(linksFile))
                            File.AppendAllText(linksFile, string.Concat(link, Environment.NewLine));
                        else File.WriteAllText(linksFile, string.Concat(link, Environment.NewLine));
                    }
                    else
                    {
                        Error?.Invoke(this, "This torrent already exists");
                    }
                }
                else
                {
                    Error?.Invoke(this, "Wrong link");
                }
            }
            catch
            {
                Error?.Invoke(this, "Error opening magnet link!");
            }
        }

        public void LoadTorrent(string filename)
        {
            if (!Directory.Exists(torrentDir))
                Directory.CreateDirectory(torrentDir);
            try
            {
                TorrentInfo info = new TorrentInfo(filename);
                File.Copy(filename, Path.Combine(torrentDir, info.Name + ".torrent"), true);
                var result = from t in Torrents where t.InfoHash.ToHex() == info.InfoHash select t;
                if (result == null || result.Count() == 0)
                {
                    var addParams = new AddTorrentParams
                    {
                        SavePath = Properties.Settings.Default.Location,
                        TorrentInfo = info
                    };
                    var torrent = Torrent.Create(addParams, session);
                    Torrents.Add(torrent);
                }
                else
                {
                    Error?.Invoke(this, "This torrent already exists");
                }
            }
            catch
            {
                Error?.Invoke(this, "Error parsing file");
            }   
        }

        public void Dispose()
        {
            foreach (var torrent in Torrents)
                torrent.Dispose();
            SaveSessionState();
        }
    }
}

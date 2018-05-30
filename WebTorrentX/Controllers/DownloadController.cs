using System;
using System.IO;
using System.Linq;
using Ragnar;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using WebTorrentX.Models;

namespace WebTorrentX.Controllers
{
    internal sealed class DownloadController : IDisposable
    {

        private const int minPort = 2000;
        private const int maxPort = 2500;
        private readonly string sesStateFile;
        private readonly string torrentDir;
        private readonly string activeTorrentsFile;


        private Session session;

        public ObservableCollection<Torrent> Torrents { get; private set; }

        public event EventHandler<string> Error;

        public DownloadController()
        {
            sesStateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".ses_state");
            torrentDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".torrents");
            activeTorrentsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".download");
            session = new Session();
            LoadSessionState();            
            Torrents = new ObservableCollection<Torrent>();
            LoadActiveTorrents();
        }

        private void LoadActiveTorrents()
        {
            if (File.Exists(activeTorrentsFile))
            {
                var lines = File.ReadAllLines(activeTorrentsFile);
                foreach (var line in lines)
                {
                    dynamic t = JsonConvert.DeserializeObject(line);
                    Torrent torrent = Torrent.CreateFromSavedData(t, session);
                    if (torrent != null)
                        Torrents.Add(torrent);
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
                string torrentFileName = Path.Combine(torrentDir, info.Name + ".torrent");
                File.Copy(filename, torrentFileName, true);
                var result = from t in Torrents where t.InfoHash.ToHex() == info.InfoHash select t;
                if (result == null || result.Count() == 0)
                {
                    var addParams = new AddTorrentParams
                    {
                        SavePath = Properties.Settings.Default.Location,
                        TorrentInfo = info
                    };
                    var torrent = Torrent.Create(addParams, session);
                    torrent.TorrentFileName = torrentFileName;
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
            if (File.Exists(activeTorrentsFile))
                File.Delete(activeTorrentsFile);
            foreach (var torrent in Torrents)
                torrent.Dispose();
            SaveSessionState();
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WebTorrentX.Models;

namespace WebTorrentX.Controllers
{
    internal sealed class DownloadController : IDisposable
    {
        private readonly string torrentDir;
        private readonly string activeTorrentsFile;

        public ObservableCollection<Torrent> Torrents { get; private set; }

        public event EventHandler<string> Error;

        private string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebTorrentX");

        public DownloadController()
        {
            torrentDir = Path.Combine(appDataFolder, ".torrents");
            activeTorrentsFile = Path.Combine(appDataFolder, ".download");
            Torrents = new ObservableCollection<Torrent>();
            LoadActiveTorrents();
        }

        private void AddTorrent(Torrent torrent)
        {
            if (Torrents.Count == 0)
            {
                Torrents.Add(torrent);
                Torrents.Add(Torrent.CreateEmpty());
            }
            else
            {
                Torrents.RemoveAt(Torrents.Count - 1);
                Torrents.Add(torrent);
                Torrents.Add(Torrent.CreateEmpty());
            }
        }

        private async void LoadActiveTorrents()
        {
            if (File.Exists(activeTorrentsFile))
            {
                var lines = File.ReadAllLines(activeTorrentsFile);
                foreach (var line in lines)
                {
                    dynamic t = JsonConvert.DeserializeObject(line);
                    Torrent torrent = await Torrent.CreateFromSavedData(t);
                    if (torrent != null)
                        AddTorrent(torrent);
                }
            }
            else Torrents.Add(Torrent.CreateEmpty());
        }

        public async void LoadMagnet(string link)
        {
            if (!Directory.Exists(torrentDir))
                Directory.CreateDirectory(torrentDir);
            try
            {
                if (link.StartsWith(@"magnet:?xt=urn:btih:") || link.StartsWith(@"http://") || link.StartsWith(@"https://"))
                {
                    var addParams = new AddTorrentParams
                    {
                        SaveFolder = Properties.Settings.Default.Location,
                        Url = link
                    };
                    var torrent = await Torrent.Create(addParams);
                    var result = from t in Torrents where t.InfoHash == torrent.InfoHash select t;
                    if (result == null || result.Count() == 0)
                    {
                        AddTorrent(torrent);
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
            catch (Exception ex)
            {
                Error?.Invoke(this, "Error opening magnet link!");
            }
        }

        public async void LoadTorrent(string filename)
        {
            if (!Directory.Exists(torrentDir))
                Directory.CreateDirectory(torrentDir);
            try
            {
                string torrentFileName = Path.Combine(torrentDir, Path.GetFileName(filename));
                File.Copy(filename, torrentFileName, true);
                var result = from t in Torrents where t.TorrentFileName == filename select t;
                if (result == null || result.Count() == 0)
                {
                    var addParams = new AddTorrentParams
                    {
                        SaveFolder = Properties.Settings.Default.Location,
                        Filename = filename
                    };
                    var torrent = await Torrent.Create(addParams);
                    torrent.TorrentFileName = torrentFileName;
                    AddTorrent(torrent);
                }
                else
                {
                    Error?.Invoke(this, "This torrent already exists");
                }
            }
            catch (Exception ex)
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
        }
    }
}

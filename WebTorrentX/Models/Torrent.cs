using MonoTorrent.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebTorrentX.Models
{
    internal sealed class Torrent : INotifyPropertyChanged, IDisposable
    {
        private bool active;
        private TorrentManager manager;

        private readonly string fastResumeDir;
        private readonly string torrentDir;
        private readonly string activeTorrentsFile;

        public bool IsEmpty { get; private set; } = false;

        public string Name
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return manager.Torrent.Name;
                else return string.Empty;
            }
        }

        public double Size
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return Math.Round((double)manager.Torrent.Size / (1024 * 1024), 2);
                else return 0;
            }
        }

        public double Done
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return Math.Round((double)FilesInfo.Sum(x => x.DownloadedSize) / (1024 * 1024), 2);
                else
                    return 0;
            }
        }

        public double Progress
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return Math.Round(manager.Progress, 2);
                else return 0;
            }
        }

        public int Peers
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return manager.OpenConnections;
                else return 0;
            }
        }

        public double Speed
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return Math.Round(manager.Monitor.DownloadSpeed / 1024f, 2);
                else
                    return 0;
            }
        }

        public string TimeRemaining
        {
            get
            {
                if (manager != null && Speed > 0 && !IsEmpty)
                {
                    int p = (int)((Size - Done) * 1024 / Speed);
                    TimeSpan ts = new TimeSpan(0, 0, p);
                    return ts.ToString() + " remaining";
                }
                else return string.Empty;

            }
        }

        public bool IsDownloading
        {
            get
            {
                if (manager != null && !IsEmpty)
                    return manager.State != TorrentState.Paused;
                else return false;
            }
            set
            {
                if (manager != null && !IsEmpty)
                {
                    if (value && manager.State == TorrentState.Paused)
                    {
                        manager.StartAsync();
                    }
                    else if (!value && manager.State != TorrentState.Paused)
                    {
                        manager.PauseAsync();
                        //torrent.SaveResumeData();
                    }
                    UpdateProperties();
                }
            }
        }

        public string Status
        {
            get
            {
                if (manager != null && !IsEmpty)
                {
                    if (manager.State == TorrentState.Paused)
                        return "Pause";
                    else
                        return manager.State.ToString();
                }
                else return string.Empty;
            }
        }

        public string DownloadFolder { get; private set; }

        public string InfoHash
        {
            get
            {
                if (manager != null && !IsEmpty)
                {
                    return manager.Torrent.InfoHash.ToHex();
                }
                else return string.Empty;
            }
        }

        public IEnumerable<TorrentFileInfo> FilesInfo
        {
            get
            {
                if (manager != null && !IsEmpty)
                {
                    for (int i = 0; i < manager.Torrent.Files.Count(); i++)
                    {
                        TorrentFileInfo tfinfo = new TorrentFileInfo(manager, i);
                        yield return tfinfo;
                    }
                }
            }
        }

        public string Url { get; set; } = string.Empty;
        public string TorrentFileName { get; set; } = string.Empty;

        private static string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebTorrentX");

        private Torrent() { }

        private Torrent(AddTorrentParams addParams)
        {
            TorrentFileName = addParams.Filename;
            Url = addParams.Url;
            DownloadFolder = addParams.SaveFolder;

            fastResumeDir = Path.Combine(appDataFolder, ".resume");
            torrentDir = Path.Combine(appDataFolder, ".torrents");
            activeTorrentsFile = Path.Combine(appDataFolder, ".download");
            LoadTorrentState(ref addParams);

            active = true;
        }

        public async Task StartAsync()
        {
            var engine = new ClientEngine();
            MonoTorrent.Torrent torrent = null;

            if (File.Exists(TorrentFileName))
            {
                torrent = await MonoTorrent.Torrent.LoadAsync(TorrentFileName);
            }
            else if (!Url.StartsWith("magnet"))
            {
                torrent = MonoTorrent.Torrent.Load(new Uri(Url), Path.Combine(torrentDir, Path.GetTempFileName() + ".torrent"));
            }

            manager = torrent is null ? await engine.AddStreamingAsync(MonoTorrent.MagnetLink.Parse(Url), DownloadFolder)
                 : await engine.AddStreamingAsync(torrent, DownloadFolder);
            await manager.StartAsync();

            if (!manager.HasMetadata)
            {
                await manager.WaitForMetadataAsync();
            }
        }

        public static async Task<Torrent> Create(AddTorrentParams addParams)
        {
            Torrent torrent = new Torrent(addParams);
            await torrent.StartAsync();
            Task.Run(delegate
            {
                while (torrent.active)
                {
                    torrent.UpdateProperties();
                    Thread.Sleep(1000);
                }
            });
            return torrent;
        }

        public static async Task<Torrent> CreateFromSavedData(dynamic torrent)
        {
            AddTorrentParams addParams = new AddTorrentParams
            {
                SaveFolder = string.IsNullOrEmpty((string)torrent.Path) ? Properties.Settings.Default.Location : (string)torrent.Path
            };
            if (string.IsNullOrEmpty((string)torrent.TorrentFileName))
            {
                if (string.IsNullOrEmpty((string)torrent.Url))
                {
                    return null;
                }
                else
                {
                    addParams.Url = (string)torrent.Url;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(appDataFolder, ".torrents", (string)torrent.TorrentFileName)))
                    addParams.Filename = Path.Combine(appDataFolder, ".torrents", (string)torrent.TorrentFileName);
                else return null;
            }
            var result = await Create(addParams);

            if (torrent.Status == "Pause")
            {
                //result.manager.Pause();
            }
            return result;
        }

        public static Torrent CreateEmpty()
        {
            Torrent torrent = new Torrent();
            torrent.IsEmpty = true;
            return torrent;
        }

        public void UpdateProperties()
        {
            if (IsEmpty) return;
            OnPropertyChanged(nameof(FilesInfo));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(Done));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Peers));
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(Status));
        }

        private void LoadTorrentState(ref AddTorrentParams p)
        {
            if (IsEmpty || p.Filename == null) return;
            string file = Path.Combine(fastResumeDir, p.Filename + ".fastresume");
            if (File.Exists(file))
            {
                p.ResumeData = File.ReadAllBytes(file);
            }
        }

        private void SaveTorrentState()
        {
            //if (handle.NeedSaveResumeData() && !IsEmpty)
            //{
            //    handle.SaveResumeData();
            //    var savedFastResume = false;
            //    while (!savedFastResume)
            //    {
            //        var alerts = session.Alerts.PopAll();
            //        if (alerts == null || !alerts.Any()) break;
            //        foreach (var alert in alerts)
            //        {
            //            if (alert is SaveResumeDataAlert)
            //            {
            //                var saveResumeAlert = (SaveResumeDataAlert)alert;
            //                if (!Directory.Exists(fastResumeDir)) Directory.CreateDirectory(fastResumeDir);
            //                var status = saveResumeAlert.Handle.QueryStatus();
            //                File.WriteAllBytes(
            //                    Path.Combine(fastResumeDir, status.Name + ".fastresume"),
            //                    saveResumeAlert.ResumeData);
            //                savedFastResume = true;
            //                break;
            //            }
            //        }
            //    }
            //}            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (IsEmpty) return;
            active = false;
            SaveTorrentState();
            dynamic torrent = new
            {
                Status = Status,
                InfoHash = InfoHash,
                Name = Name,
                Path = DownloadFolder,
                Url = Url,
                TorrentFileName = Path.GetFileName(TorrentFileName)
            };
            if (File.Exists(activeTorrentsFile))
                File.AppendAllText(activeTorrentsFile, string.Concat(JsonConvert.SerializeObject(torrent), Environment.NewLine));
            else File.WriteAllText(activeTorrentsFile, string.Concat(JsonConvert.SerializeObject(torrent), Environment.NewLine));
        }

        public void Remove()
        {
            if (IsEmpty || manager is null) return;
            active = false;
            string torrent = Path.Combine(torrentDir, manager.Torrent.Name + ".torrent");
            if (File.Exists(torrent))
                File.Delete(torrent);
            string fastResume = Path.Combine(fastResumeDir, manager.Torrent.Name + ".fastresume");
            if (File.Exists(fastResume))
                File.Delete(fastResume);
        }
    }
}

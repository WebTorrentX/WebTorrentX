﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ragnar;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WebTorrentX.Models
{
    internal sealed class Torrent : INotifyPropertyChanged, IDisposable
    {
        private bool active;
        private Session session;
        private TorrentHandle handle;

        private readonly string fastResumeDir;
        private readonly string torrentDir;
        private readonly string activeTorrentsFile;

        public bool IsEmpty { get; private set; } = false;

        public string Name
        {
            get
            {
                if (handle != null && !IsEmpty)
                    return handle.QueryStatus().Name;
                else return string.Empty;
            }
        }

        public double Size
        {
            get
            {
                if (handle != null && !IsEmpty)
                    return Math.Round((double)handle.QueryStatus().TotalWanted / (1024 * 1024), 2);
                else return 0;
            }
        }

        public double Done
        {
            get
            {
                if (handle != null && !IsEmpty)
                    return Math.Round((double)handle.QueryStatus().TotalWantedDone / (1024 * 1024), 2);
                else return 0;
            }
        }

        public double Progress
        {
            get
            {
                if (handle != null && !IsEmpty)
                    return Math.Round(handle.QueryStatus().Progress * 100f, 2);
                else return 0;
            }
        }

        public int Peers
        {
            get
            {
                if (handle != null && !IsEmpty)
                    return handle.QueryStatus().NumPeers;
                else return 0;
            }
        }        

        public double Speed
        {
            get
            {
                if (handle != null && !IsEmpty)
                    return Math.Round(handle.QueryStatus().DownloadRate / 1024f, 2);
                else return 0;
            }
        }

        public string TimeRemaining
        {
            get
            {
                if (handle != null && Speed > 0 && !IsEmpty)
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
                if (handle != null && !IsEmpty)
                    return !handle.IsPaused;
                else return false;
            }
            set
            {
                if (handle != null && !IsEmpty)
                {
                    if (value && handle.IsPaused)
                    {
                        handle.Resume();
                    }
                    else if (!value && !handle.IsPaused)
                    {
                        handle.Pause();
                        handle.SaveResumeData();
                    }
                    UpdateProperties();
                }                
            }
        }   

        public string Status
        {
            get
            {
                if (handle != null && !IsEmpty)
                {
                    if (handle.IsPaused)
                        return "Pause";
                    else
                        return handle.QueryStatus().State.ToString();
                }
                else return string.Empty;
            }
        }

        public string DownloadPath
        {
            get
            {
                if (handle != null && !IsEmpty)
                {
                    return handle.QueryStatus().SavePath;
                }
                else return string.Empty;
            }
        }

        public string InfoHash
        {
            get
            {
                if (handle != null && !IsEmpty)
                {
                    return handle.InfoHash.ToHex();
                }
                else return string.Empty;
            }
        }
        
        public IEnumerable<TorrentFileInfo> FilesInfo
        {
            get
            {
                if (handle != null && handle.TorrentFile != null && !IsEmpty)
                {
                    for (int i = 0; i < handle.TorrentFile.NumFiles; i++)
                    {
                        TorrentFileInfo tfinfo = new TorrentFileInfo(handle, i);
                        yield return tfinfo;
                    }
                }
            }
        }

        public string Url { get; set; } = string.Empty;
        public string TorrentFileName { get; set; } = string.Empty;

        private static string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebTorrentX");

        private Torrent() { }

        private Torrent(AddTorrentParams addParams, Session session)
        {
            fastResumeDir = Path.Combine(appDataFolder, ".resume");
            torrentDir = Path.Combine(appDataFolder, ".torrents");
            activeTorrentsFile = Path.Combine(appDataFolder, ".download");
            LoadTorrentState(ref addParams);
            handle = session.AddTorrent(addParams);
            handle.SequentialDownload = true;
            handle.AutoManaged = false;
            handle.FlushCache();
            this.session = session;
            Url = addParams.Url;
            active = true;
        }

        public static Torrent Create(AddTorrentParams addParams, Session session)
        {

            Torrent torrent = new Torrent(addParams, session);   
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

        public static Torrent CreateFromSavedData(dynamic torrent, Session session)
        {
            AddTorrentParams addParams = new AddTorrentParams
            {
                SavePath = string.IsNullOrEmpty((string)torrent.Path) ? Properties.Settings.Default.Location : (string)torrent.Path
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
                    addParams.TorrentInfo = new TorrentInfo(Path.Combine(appDataFolder, ".torrents", (string)torrent.TorrentFileName));
                else return null;
            }
            var result =  Create(addParams, session);
            result.Url = (string)torrent.Url;
            result.TorrentFileName = Path.Combine(appDataFolder, ".torrents", (string)torrent.TorrentFileName);
            if (torrent.Status == "Pause")
            {
                result.handle.Pause();
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
            if (IsEmpty || p.TorrentInfo == null) return;
            string file = Path.Combine(fastResumeDir, p.TorrentInfo.Name + ".fastresume");
            if (File.Exists(file))
            {
                p.ResumeData = File.ReadAllBytes(file);
            }
        }

        private void SaveTorrentState()
        {
            if (handle.NeedSaveResumeData() && !IsEmpty)
            {
                handle.SaveResumeData();
                var savedFastResume = false;
                while (!savedFastResume)
                {
                    var alerts = session.Alerts.PopAll();
                    if (alerts == null || !alerts.Any()) break;
                    foreach (var alert in alerts)
                    {
                        if (alert is SaveResumeDataAlert)
                        {
                            var saveResumeAlert = (SaveResumeDataAlert)alert;
                            if (!Directory.Exists(fastResumeDir)) Directory.CreateDirectory(fastResumeDir);
                            var status = saveResumeAlert.Handle.QueryStatus();
                            File.WriteAllBytes(
                                Path.Combine(fastResumeDir, status.Name + ".fastresume"),
                                saveResumeAlert.ResumeData);
                            savedFastResume = true;
                            break;
                        }
                    }
                }
            }            
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
                Path = DownloadPath,
                Url = Url,
                TorrentFileName = Path.GetFileName(TorrentFileName)
            };
            if (File.Exists(activeTorrentsFile))
                File.AppendAllText(activeTorrentsFile, string.Concat(JsonConvert.SerializeObject(torrent), Environment.NewLine));
            else File.WriteAllText(activeTorrentsFile, string.Concat(JsonConvert.SerializeObject(torrent), Environment.NewLine));
        }

        public void Remove()
        {
            if (IsEmpty) return;
            active = false;
            string torrent = Path.Combine(torrentDir, handle.QueryStatus().Name + ".torrent");
            if (File.Exists(torrent))
                File.Delete(torrent);
            string fastResume = Path.Combine(fastResumeDir, handle.QueryStatus().Name + ".fastresume");
            if (File.Exists(fastResume))
                File.Delete(fastResume);
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ragnar;

namespace WebTorrentX.Models
{
    internal sealed class Torrent : INotifyPropertyChanged, IDisposable
    {
        private bool active;
        private Session session;
        private TorrentHandle handle;

        private const string fastResumeDir = ".resume";
        private const string torrentDir = ".torrents";

        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            private set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }                
            }
        }

        public double Size
        {
            get
            {
                if (handle != null)
                    return Math.Round((double)handle.QueryStatus().TotalWanted / (1024 * 1024), 2);
                else return 0;
            }
        }

        public double Done
        {
            get
            {
                if (handle != null)
                    return Math.Round((double)handle.QueryStatus().TotalWantedDone / (1024 * 1024), 2);
                else return 0;
            }
        }

        public double Progress
        {
            get
            {
                if (handle != null)
                    return Math.Round(handle.QueryStatus().Progress * 100f, 2);
                else return 0;
            }
        }

        public int Peers
        {
            get
            {
                if (handle != null)
                    return handle.QueryStatus().NumPeers;
                else return 0;
            }
        }        

        public double Speed
        {
            get
            {
                if (handle != null)
                    return Math.Round(handle.QueryStatus().DownloadRate / 1024f, 2);
                else return 0;
            }
        }

        public string TimeRemaining
        {
            get
            {
                if (handle != null && Speed > 0)
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
                if (handle != null)
                    return !handle.IsPaused;
                else return false;
            }
            set
            {
                if (handle != null)
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
                if (handle != null)
                {
                    if (handle.IsPaused)
                        return "Pause";
                    else return handle.QueryStatus().State.ToString();
                }
                else return string.Empty;
            }
        }

        public SHA1Hash InfoHash
        {
            get
            {
                return handle.InfoHash;
            }
        }

        private Torrent(AddTorrentParams addParams, Session session)
        {
            LoadTorrentState(ref addParams);
            handle = session.AddTorrent(addParams);
            handle.SequentialDownload = true;
            handle.AutoManaged = false;
            this.session = session;
            name = handle.QueryStatus().Name;
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

        public void UpdateProperties()
        {
            GetHashCode();
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(Peers));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(Done));
            OnPropertyChanged(nameof(Progress));
        }

        private void LoadTorrentState(ref AddTorrentParams p)
        {
            if (p.TorrentInfo == null) return;
            string file = Path.Combine(fastResumeDir, p.TorrentInfo.Name + ".fastresume");
            if (File.Exists(file))
            {
                p.ResumeData = File.ReadAllBytes(file);
            }
        }

        private void SaveTorrentState()
        {
            if (handle.NeedSaveResumeData())
            {
                handle.SaveResumeData();
                var savedFastResume = false;
                while (!savedFastResume)
                {
                    var alerts = session.Alerts.PopAll();
                    if (alerts == null || !alerts.Any()) continue;
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
            active = false;
            SaveTorrentState();
        }

        public void Remove()
        {
            active = false;
            File.Delete(Path.Combine(torrentDir, handle.TorrentFile.Name + ".torrent"));
            File.Delete(Path.Combine(fastResumeDir, handle.TorrentFile.Name + ".fastresume"));
        }
    }
}

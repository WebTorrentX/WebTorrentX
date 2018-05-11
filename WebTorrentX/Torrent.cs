using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ragnar;

namespace WebTorrentX
{
    internal sealed class Torrent : INotifyPropertyChanged, IDisposable
    {
        private bool active;
        private Session session;
        private TorrentHandle handle;

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
                    if (handle.IsPaused)
                    {
                        handle.Resume();
                    }
                    else
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
                    var status = torrent.handle.QueryStatus();
                    if (status.IsSeeding)
                    {
                        //break;
                    }
                    torrent.UpdateProperties();
                    Thread.Sleep(1000);
                }
            });
            return torrent;
        }

        public void UpdateProperties()
        {
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(Peers));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(Done));
            OnPropertyChanged(nameof(Progress));
        }

        private static void LoadTorrentState(ref AddTorrentParams p)
        {
            if (File.Exists(Path.Combine(".resume", p.TorrentInfo.Name + ".fastresume")))
            {
                p.ResumeData = File.ReadAllBytes(Path.Combine(".resume", p.TorrentInfo.Name + ".fastresume"));
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
                            if (!Directory.Exists(".resume")) Directory.CreateDirectory(".resume");
                            var status = saveResumeAlert.Handle.QueryStatus();
                            File.WriteAllBytes(
                                Path.Combine(".resume", status.Name + ".fastresume"),
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
        }
    }
}

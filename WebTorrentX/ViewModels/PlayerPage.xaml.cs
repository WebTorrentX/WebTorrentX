using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using FontAwesome;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using WebTorrentX.Models;
using System.IO;
using System.Linq;

namespace WebTorrentX.ViewModels
{

    public sealed partial class PlayerPage : Page, INotifyPropertyChanged
    {

        private Torrent torrent;
        private string filename = string.Empty;
        private int fileindex = 0;
        private int bufferingStart = 3;

        private Visibility loading = Visibility.Visible;
        public Visibility Loading
        {
            get
            {
                return loading;
            }
            set
            {
                loading = value;
                OnPropertyChanged(nameof(Loading));
            }
        }

        private bool IsBuffering
        {
            get
            {
                return loading == Visibility.Visible;
            }
            set
            {
                if (value)
                    loading = Visibility.Visible;
                else loading = Visibility.Collapsed;
                OnPropertyChanged(nameof(Loading));
            }
        }

        public double Progress
        {
            get
            {
                return Player.Time.TotalSeconds;
            }
        }

        private TimeSpan Length
        {
            get
            {
                if (Player.VlcMediaPlayer.Media != null)
                {
                    if (Player.VlcMediaPlayer.Media.Duration != TimeSpan.Zero)
                        return Player.VlcMediaPlayer.Media.Duration;
                    else
                    {
                        try
                        {
                            var shell = ShellObject.FromParsingName(filename);
                            IShellProperty prop = shell.Properties.System.Media.Duration;
                            var t = (ulong)prop.ValueAsObject;
                            return TimeSpan.FromTicks((long)t);
                        }
                        catch
                        {
                            return TimeSpan.Zero;
                        }
                    }
                }
                else return TimeSpan.Zero;                
            }
        }

        public double Maximum
        {
            get
            {
                if (Length != null)
                    return Length.TotalSeconds;
                else return 0;
            }
        }

        private int volumeBackup = 0;
        public int Volume
        {
            get
            {
                return Player.Volume;
            }
            set
            {
                if (value < 0 || value > 100) return;
                Player.Volume = value;
                if (Player.Volume == 0)
                    VolumeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.VolumeOff;
                else if (Player.Volume > 0 && Player.Volume < 50)
                    VolumeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.VolumeDown;
                else VolumeIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.VolumeUp;
                OnPropertyChanged(nameof(Volume));
            }
        }

        public string Time
        {
            get
            {
                string time = Player.Time.ToString(@"hh\:mm\:ss");
                string length = Length != null ? Length.ToString(@"hh\:mm\:ss") : string.Empty;
                length = " / " + length;
                return time + length;
            }
        }

        public double Buffering
        {
            get
            {
                if (torrent != null)
                {
                    //double buffer = 0;
                    //buffer = torrent.FilesInfo[fileindex].Item1.Length * 100 / torrent.FilesInfo[fileindex].Item2;
                    return torrent.FileDownloadedPercent.ElementAt(fileindex);
                }
                else return 0;
            }
        }

        

        public PlayerPage()
        {
            InitializeComponent();
            DataContext = this;
            Player.VlcMediaPlayer.MediaChanged += (object sender, Meta.Vlc.MediaPlayerMediaChangedEventArgs e) => 
            {
                OnPropertyChanged(nameof(Maximum));
            };
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            torrent = Application.Current.Properties["torrent"] as Torrent;
            if (torrent != null)
            {
                if (torrent.Files.Count() == 1)
                {
                    filename = Path.Combine(torrent.DownloadPath, torrent.Files.ElementAt(0));
                    fileindex = 0;
                    try
                    {
                        Player.LoadMedia(filename);                      
                    }
                    catch
                    {
                        string message = string.Format("Couldn't open file {0}", filename);
                        MessageBox.Show(message, "WebTorrentX", MessageBoxButton.OK);
                        GoBack();
                    }
                    Thread t = new Thread(() =>
                    {
                        while (Player.VlcMediaPlayer.State != Meta.Vlc.Interop.Media.MediaState.Stopped)
                        {
                            ThreadTask();
                            Thread.Sleep(500);
                        }                        
                    });
                    t.Start();
                }
                else
                {
                    string path = Path.Combine(torrent.DownloadPath, torrent.Name);
                    if (Directory.Exists(path))
                    {
                        string[] extensions = { ".mkv", ".flv", ".f4p", ".f4a", ".f4v", "f4b", ".avi", ".wmv", ".mp4", ".m4p", ".m4v", ".mpg", ".mpeg", ".m2v", ".3gp" };
                        foreach (string file in Directory.GetFiles(path))
                        {
                            if (extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                            {
                                for (int i = 0; i < torrent.Files.Count(); i++)
                                {
                                    string name = Path.Combine(torrent.DownloadPath, torrent.Files.ElementAt(i));
                                    if (name.Equals(file))
                                    {
                                        filename = name;
                                        fileindex = i;
                                        try
                                        {
                                            Player.LoadMedia(filename);
                                            Thread t = new Thread(() =>
                                            {
                                                while (Player.VlcMediaPlayer.State != Meta.Vlc.Interop.Media.MediaState.Stopped)
                                                {
                                                    ThreadTask();
                                                    Thread.Sleep(500);
                                                }
                                            });
                                            t.Start();
                                            return;
                                        }
                                        catch
                                        {
                                            string message = string.Format("Couldn't open file {0}", filename);
                                            MessageBox.Show(message, "WebTorrentX", MessageBoxButton.OK);
                                            GoBack();
                                        }
                                        
                                    }
                                }   
                            }
                        }
                    }
                    else
                    {
                        GoBack();
                    }                    
                }
            }
            else
            {
                GoBack();
            }
        }

        private void ThreadTask()
        {
            if (Length > TimeSpan.Zero)
            {
                double percentPlayed = Progress * 100 / Length.TotalSeconds;
                if (Buffering - percentPlayed <= bufferingStart && Player.VlcMediaPlayer.IsPlaying)
                {
                    Dispatcher.Invoke(delegate
                    {
                        IsBuffering = true;
                        Pause();
                    });
                }
                else if (Buffering - percentPlayed > bufferingStart && IsBuffering)
                {
                    Dispatcher.Invoke(delegate
                    {
                        IsBuffering = false;
                        Play();
                    });
                }
            }
            else
            {
                Dispatcher.Invoke(delegate
                {
                    IsBuffering = false;
                    Play();
                });
            }
            Dispatcher.Invoke(delegate 
            {
                OnPropertyChanged(nameof(Buffering));
                OnPropertyChanged(nameof(Length));
                OnPropertyChanged(nameof(Maximum));
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(Time));
            });
            
        }

        private void Player_StateChanged(object sender, Meta.Vlc.ObjectEventArgs<Meta.Vlc.Interop.Media.MediaState> e)
        {
            switch (e.Value)
            {
                case Meta.Vlc.Interop.Media.MediaState.Opening:
                    Loading = Visibility.Visible;
                    break;
                case Meta.Vlc.Interop.Media.MediaState.Playing:
                    Loading = Visibility.Collapsed;
                    break;
                case Meta.Vlc.Interop.Media.MediaState.Error:
                    string message = string.Format("Couldn't open file {0}", filename);
                    MessageBox.Show(message, "WebTorrentX", MessageBoxButton.OK);
                    GoBack();
                    break;
                case Meta.Vlc.Interop.Media.MediaState.Ended:                    
                    break;
                default: break;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Player.Stop();
        }

        private void progressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Length > TimeSpan.Zero)
            {
                double time = (sender as Slider).Value;
                double value = time * 100 / Length.TotalSeconds;
                if (Buffering - value > 1)
                {
                    Player.Time = new TimeSpan(0, 0, (int)time);
                }                
            }                
        }

        private void SoundSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Volume = (int)(sender as Slider).Value;
            volumeBackup = 0;
        }

        private void SoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (volumeBackup == 0)
            {
                volumeBackup = Volume;
                Volume = 0;
            }
            else
            {
                Volume = volumeBackup;
                volumeBackup = 0;                
            }
        }

        public void GoBack()
        {
            if (NavigationService.CanGoBack)
            {
                Player.Stop();
                if (Window.GetWindow(this).WindowState == WindowState.Maximized)
                {
                    Window.GetWindow(this).WindowState = WindowState.Normal;
                    Window.GetWindow(this).WindowStyle = WindowStyle.SingleBorderWindow;
                }
                NavigationService.GoBack();
            }                
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        public void Fullscreen()
        {
            if (Window.GetWindow(this).WindowState == WindowState.Normal)
            {
                Window.GetWindow(this).WindowStyle = WindowStyle.None;
                Window.GetWindow(this).WindowState = WindowState.Maximized;
            }
            else
            {
                Window.GetWindow(this).WindowState = WindowState.Normal;
                Window.GetWindow(this).WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Fullscreen();
        }

        private void Stop()
        {
            Player.Stop();
            PlayPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Play;
        }

        public void PlayPause()
        {
            if (IsBuffering == false)
            {
                if (Player.VlcMediaPlayer.IsPlaying)
                {
                    Pause();
                }
                else
                {
                    Play();
                }
            }            
        }

        private void Play()
        {
            Player.Play();
            PlayPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Pause;
        }

        private void Pause()
        {
            Player.Pause();
            PlayPauseIcon.Icon = FontAwesome.WPF.FontAwesomeIcon.Play;
        }

        public void IncreaseVolume()
        {
            Volume++;
        }

        public void DecreaseVolume()
        {
            Volume--;
        }

        public void IncreaseSpeed()
        {
            Player.Rate ++;
        }

        public void DecreaseSpeed()
        {
            Player.Rate --;
        }

        public void LoadSubtitles(string path)
        {
            bool b = Player.VlcMediaPlayer.SetSubtitleFile(path);
        }
    }
}

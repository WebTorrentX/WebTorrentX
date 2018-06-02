using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MaterialIcons;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;


namespace WebTorrentX.ViewModels
{

    public sealed partial class PlayerPage : Page, INotifyPropertyChanged
    {

        private string filename = string.Empty;

        private Visibility loading = Visibility.Collapsed;
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
                    VolumeIcon.Icon = MaterialIconType.ic_volume_mute;
                else if (Player.Volume > 0 && Player.Volume < 50)
                    VolumeIcon.Icon = MaterialIconType.ic_volume_down;
                else VolumeIcon.Icon = MaterialIconType.ic_volume_up;
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

        

        public PlayerPage()
        {
            InitializeComponent();
            DataContext = this;
            Player.VlcMediaPlayer.MediaChanged += (object sender, Meta.Vlc.MediaPlayerMediaChangedEventArgs e) => 
            {
                OnPropertyChanged(nameof(Maximum));
            };
        }

        private void Player_TimeChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(Maximum));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Time));
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

        private void UpdateMedia()
        {
            Player.RebuildPlayer();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            filename = Application.Current.Properties["filename"].ToString();
            try
            {                
                Player.LoadMedia(filename);
                Player.Play();
            }
            catch
            {
                string message = string.Format("Couldn't open file {0}", filename);
                MessageBox.Show(message, "WebTorrentX", MessageBoxButton.OK);
                GoBack();
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
            Player.Time = new TimeSpan(0, 0, (int)(sender as Slider).Value);
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
                if (Window.GetWindow(this).WindowState == WindowState.Maximized)
                {
                    Window.GetWindow(this).WindowState = WindowState.Normal;
                    Window.GetWindow(this).WindowStyle = WindowStyle.SingleBorderWindow;
                    FullscreenIcon.Icon = MaterialIconType.ic_fullscreen;
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
                FullscreenIcon.Icon = MaterialIconType.ic_fullscreen_exit;
            }
            else
            {
                Window.GetWindow(this).WindowState = WindowState.Normal;
                Window.GetWindow(this).WindowStyle = WindowStyle.SingleBorderWindow;
                FullscreenIcon.Icon = MaterialIconType.ic_fullscreen;
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Fullscreen();
        }

        private void Stop()
        {
            Player.Stop();
            PlayPauseIcon.Icon = MaterialIconType.ic_play_arrow;
        }

        public void PlayPause()
        {
            if (Player.VlcMediaPlayer.IsPlaying)
            {
                Player.Pause();
                PlayPauseIcon.Icon = MaterialIconType.ic_play_arrow;
            }
            else
            {
                Player.Play();
                PlayPauseIcon.Icon = MaterialIconType.ic_pause;
            }
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

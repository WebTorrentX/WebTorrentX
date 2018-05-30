using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MaterialIcons;

namespace WebTorrentX.ViewModels
{

    public sealed partial class PlayerPage : Page, INotifyPropertyChanged
    {
        private bool subtitleEnabled = false;
        public bool SubtitleEnabled
        {
            get
            {
                return subtitleEnabled;
            }
            set
            {
                subtitleEnabled = value;
                OnPropertyChanged(nameof(SubtitleEnabled));
            }
        }

        public string Subtitle
        {
            get
            {
                return "";
            }
        }

        public double Progress
        {
            get
            {
                return vlcPlayer.Time.TotalSeconds;
            }
        }

        public double Maximum
        {
            get
            {
                return vlcPlayer.Length.TotalSeconds;
            }
        }

        private int volumeBackup = 0;
        public int Volume
        {
            get
            {
                return vlcPlayer.Volume;
            }
            set
            {
                if (value < 0 || value > 100) return;
                vlcPlayer.Volume = value;
                if (vlcPlayer.Volume == 0)
                    VolumeIcon.Icon = MaterialIconType.ic_volume_mute;
                else if (vlcPlayer.Volume > 0 && vlcPlayer.Volume < 50)
                    VolumeIcon.Icon = MaterialIconType.ic_volume_down;
                else VolumeIcon.Icon = MaterialIconType.ic_volume_up;
                OnPropertyChanged(nameof(Volume));
            }
        }

        public string Time
        {
            get
            {
                return vlcPlayer.Time.ToString(@"hh\:mm\:ss") + " / " + vlcPlayer.Length.ToString(@"hh\:mm\:ss");
            }
        }

        

        public PlayerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void vlcPlayer_TimeChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Maximum));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Time));
        }

        private void vlcPlayer_StateChanged(object sender, Meta.Vlc.ObjectEventArgs<Meta.Vlc.Interop.Media.MediaState> e)
        {
            if (e.Value == Meta.Vlc.Interop.Media.MediaState.Error)
            {
                string message = string.Format("Couldn't open file {0}", Application.Current.Properties["filename"].ToString());
                MessageBox.Show(message, "WebTorrentX", MessageBoxButton.OK);
                GoBack();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = Application.Current.Properties["filename"].ToString();
            try
            {
                
                vlcPlayer.LoadMedia(filename);
                vlcPlayer.Play();
            }
            catch
            {
                string message = string.Format("Couldn't open file {0}", Application.Current.Properties["filename"].ToString());
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
            vlcPlayer.Stop();
            PlayPauseIcon.Icon = MaterialIconType.ic_play_arrow;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            vlcPlayer.Stop();
        }

        private void progressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            vlcPlayer.Time = new TimeSpan(0, 0, (int)(sender as Slider).Value);
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

        public void PlayPause()
        {
            if (vlcPlayer.VlcMediaPlayer.IsPlaying)
            {
                vlcPlayer.Pause();
                PlayPauseIcon.Icon = MaterialIconType.ic_play_arrow;
            }
            else
            {
                vlcPlayer.Play();
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
            vlcPlayer.Rate ++;
        }

        public void DecreaseSpeed()
        {
            vlcPlayer.Rate --;
        }

        public void LoadSubtitles(string path)
        {
            bool b = vlcPlayer.VlcMediaPlayer.SetSubtitleFile(path);
            SubtitleEnabled = true;
        }

    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MaterialIcons;

namespace WebTorrentX.ViewModels
{

    public sealed partial class PlayerPage : Page, INotifyPropertyChanged
    {

        private double progress;
        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        public long Maximum
        {
            get
            {
                return vlcControl.MediaPlayer.Length;
            }
        }

        

        public PlayerPage()
        {
            InitializeComponent();
            DataContext = this;
            vlcControl.MediaPlayer.VlcLibDirectoryNeeded += OnVlcControlNeedsLibDirectory;
            vlcControl.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            vlcControl.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            vlcControl.MediaPlayer.EndInit();
        }

        private void MediaPlayer_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Maximum));
        }

        private void MediaPlayer_TimeChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTimeChangedEventArgs e)
        {
            Progress = vlcControl.MediaPlayer.Time;
        }


        private void OnVlcControlNeedsLibDirectory(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;
            if (IntPtr.Size == 4)
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"libvlc\win-x86\"));
            else
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"libvlc\win-x64\"));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string filename = Application.Current.Properties["filename"].ToString();
            vlcControl.MediaPlayer.Play(new Uri(filename));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (vlcControl.MediaPlayer.IsPlaying)
            {
                vlcControl.MediaPlayer.Pause();
                PlayPauseIcon.Icon = MaterialIconType.ic_play_arrow;
            }
            else
            {
                vlcControl.MediaPlayer.Play();
                PlayPauseIcon.Icon = MaterialIconType.ic_pause; 
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            vlcControl.MediaPlayer.Stop();
            Progress = 0;
            PlayPauseIcon.Icon = MaterialIconType.ic_play_arrow;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            vlcControl.MediaPlayer.Stop();
        }

        private void progressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            vlcControl.MediaPlayer.Time = (long)(sender as Slider).Value;
        }
    }
}

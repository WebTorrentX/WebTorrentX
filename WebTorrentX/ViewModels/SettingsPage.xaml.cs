using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WebTorrentX.ViewModels
{

    public partial class SettingsPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Location
        {
            get
            {
                return Properties.Settings.Default.Location;
            }
            set
            {
                Properties.Settings.Default.Location = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(Location));
            }
        }

        public string WatchFolder
        {
            get
            {
                return Properties.Settings.Default.WatchFolder;
            }
            set
            {
                Properties.Settings.Default.WatchFolder = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(WatchFolder));
            }
        }

        public bool WatchForNewFiles
        {
            get
            {
                return Properties.Settings.Default.WatchForNewFiles;
            }
            set
            {
                if (value && string.IsNullOrEmpty(WatchFolder))
                {
                    var result = System.Windows.MessageBox.Show("Select the watch folder first!", "WebTorrentX", MessageBoxButton.OK);
                    value = false;
                }
                Properties.Settings.Default.WatchForNewFiles = value;
                Properties.Settings.Default.Save();                
                OnPropertyChanged(nameof(WatchForNewFiles));
            }
        }

        public bool OpenOnStartup
        {
            get
            {
                return Properties.Settings.Default.OpenOnStartup;
            }
            set
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (value)
                {
                    key.SetValue(AppDomain.CurrentDomain.FriendlyName, System.Reflection.Assembly.GetEntryAssembly().Location);
                }
                else
                {
                    key.DeleteValue(AppDomain.CurrentDomain.FriendlyName, false);
                }
                Properties.Settings.Default.OpenOnStartup = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged(nameof(OpenOnStartup));
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void GoBack()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void ChangeLocationButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Location = dialog.SelectedPath;
                }
            }
        }

        private void ChangeWatchFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    WatchFolder = dialog.SelectedPath;
                }
            }
        }

        private void MakeDefaultAppButton_Click(object sender, RoutedEventArgs e)
        {
            Associate(".torrent", "WebTorrentX", string.Empty, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"), System.Reflection.Assembly.GetEntryAssembly().Location);
        }


        private void Associate(string extension, string progID, string description, string icon, string application)
        {
            var key = Registry.CurrentUser.OpenSubKey("Software\\Classes", true);
            key.CreateSubKey(extension, true).SetValue("", progID);
            if (progID != null && progID.Length > 0)
            {
                using (RegistryKey subkey = key.CreateSubKey(progID))
                {
                    if (description != null) subkey.SetValue("", description);
                    if (icon != null) subkey.CreateSubKey("DefaultIcon").SetValue("", icon);
                    if (application != null) subkey.CreateSubKey(@"Shell\Open\Command").SetValue("", application + " \"%1\"");
                }
            }
                
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

    }
}

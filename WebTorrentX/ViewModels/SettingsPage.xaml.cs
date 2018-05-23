using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WebTorrentX.ViewModels
{

    public partial class SettingsPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string location;
        public string Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        private string watchFolder;
        public string WatchFolder
        {
            get
            {
                return watchFolder;
            }
            set
            {
                watchFolder = value;
                OnPropertyChanged(nameof(WatchFolder));
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
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

        }

        private void ChangeWatchFolderButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MakeDefaultAppButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WebTorrentX.Controllers;
using WebTorrentX.Models;
using WebTorrentX.ViewModels;

namespace WebTorrentX
{

    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Page currentPage;
        public Page CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CurrentPage = new DownloadPage();
        }        

        private void Window_Closing(object sender, CancelEventArgs e)
        {            
            //downloadController.Dispose();
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.O:
                        //OpenTorrent();
                        break;
                    case Key.U:
                        break;
                    case Key.W:
                        Close();
                        break;
                    default: break;
                }
            }
        }
    }
}

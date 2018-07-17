using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WebTorrentX.ViewModels
{

    public partial class AddTorrentLinkPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string link = string.Empty;
        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                link = value;
                OnPropertyChanged(nameof(Link));
            }
        }

        public AddTorrentLinkPage()
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

        private void AddTorrentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(link))
            {
                App.downloadController.LoadMagnet(link);
                GoBack();
            }            
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        public void Undo()
        {
            LinkTextBox.Undo();
        }

        public void Redo()
        {
            LinkTextBox.Redo();
        }

        public void Cut()
        {
            LinkTextBox.Cut();
        }

        public void Copy()
        {
            LinkTextBox.Copy();
        }

        public void Paste()
        {
            LinkTextBox.Paste();
        }

        public void Delete()
        {
            int length = LinkTextBox.SelectionLength;
            Link = LinkTextBox.Text.Remove(LinkTextBox.SelectionStart, length);
        }

        public void SelectAll()
        {
            LinkTextBox.SelectAll();
        }
    }
}

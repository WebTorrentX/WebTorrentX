using System.Windows;
using System.Reflection;

namespace WebTorrentX.ViewModels
{

    public partial class AboutWindow : Window
    {

        public string Version { get; set; }

        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;
            Version = string.Concat("Version: ", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}

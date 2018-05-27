using System.IO;
using System.Windows;
using WebTorrentX.Controllers;

namespace WebTorrentX
{

    public partial class App : Application
    {

        internal static DownloadController downloadController;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                Application.Current.Properties["openfile"] = e.Args[0];
            }
        }
    }
}

using System.IO;
using System.Windows;

namespace WebTorrentX
{

    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                Current.Properties["openfile"] = e.Args[0];
            }
        }
    }
}

using System.Windows;

namespace WebTorrentX
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                Application.Current.Properties["openfile"] = e.Args[0];
            }
        }
    }
}

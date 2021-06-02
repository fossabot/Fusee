using Fusee.Examples.Integrations.Wpf.Frontend.View;
using System.Windows;

namespace Fusee.Examples.Integrations.Wpf.Frontend
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
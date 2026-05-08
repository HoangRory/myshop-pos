using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MyShop.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DIContainer.ConfigureServices();
            var nav = DIContainer.ServiceProvider.GetRequiredService<Services.INavigationService>();
            nav.NavigateTo("Main");
            var mainWindow = DIContainer.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}

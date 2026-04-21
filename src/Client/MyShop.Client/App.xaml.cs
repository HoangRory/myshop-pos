using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DIContainer.ConfigureServices();
            var nav = DIContainer.ServiceProvider.GetRequiredService<Services.INavigationService>();
            nav.NavigateTo<ViewModels.MainViewModel>();
            var mainWindow = DIContainer.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}

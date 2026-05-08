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

            var mainWindow = DIContainer.ServiceProvider.GetRequiredService<MainWindow>();
            var mainVM = DIContainer.ServiceProvider.GetRequiredService<ViewModels.MainViewModel>();
            mainWindow.DataContext = mainVM;

            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }

}

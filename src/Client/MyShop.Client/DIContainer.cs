using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client
{
    public static class DIContainer
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register ViewModels
            services.AddSingleton<ViewModels.MainViewModel>();
            services.AddSingleton<ViewModels.ProductsViewModel>();
            services.AddSingleton<ViewModels.OrdersViewModel>();
            services.AddSingleton<ViewModels.ReportsViewModel>();
            services.AddSingleton<ViewModels.SettingsViewModel>();
            services.AddSingleton<ViewModels.DashboardViewModel>();
            // Đăng ký các ViewModel khác nếu có


            // Register NavigationService as INavigationService
            services.AddSingleton<Services.INavigationService, Services.NavigationService>();

            // Register ProductApiClient
            services.AddSingleton<Services.Interfaces.IProductApiClient, Services.Mock.MockProductApiClient>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

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
            services.AddSingleton<ViewModels.ProductViewModel>();
            // Đăng ký các ViewModel khác nếu có


            // Register NavigationService as INavigationService
            services.AddSingleton<Services.INavigationService, Services.NavigationService>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

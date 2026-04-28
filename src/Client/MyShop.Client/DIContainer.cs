using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.Services;
using MyShop.Client.Services.Interfaces;

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
            services.AddScoped<Services.Interfaces.IProductService, Services.ProductService>();
            services.AddSingleton<Services.Interfaces.IDialogService, Services.DialogService>();
            services.AddSingleton<ViewModels.OrdersViewModel>();
            services.AddSingleton<ViewModels.ReportsViewModel>();
            services.AddSingleton<ViewModels.SettingsViewModel>();
            services.AddSingleton<ViewModels.DashboardViewModel>();
            // Đăng ký các ViewModel khác nếu có


            // Register NavigationService as INavigationService
            services.AddSingleton<Services.INavigationService, Services.NavigationService>();
            services.AddScoped<IProductService, ProductService>();


            // Register shared named HttpClient for all API clients
            services.AddHttpClient("MyShopAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:8443/");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });
            services.AddSingleton(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("MyShopAPI");
            });
            services.AddScoped<IProductService, ProductService>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

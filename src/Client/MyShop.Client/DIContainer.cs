using LuciferCore.Attributes;
using LuciferCore.Main;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.Services;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;
using System.Reflection;

namespace MyShop.Client
{
    public static class DIContainer
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        public static IEnumerable<Type> Plugins { get; private set; } = Lucifer.GetTypesWithAttribute<PluginAttribute>();

        public static Dictionary<string, Type> ViewModels { get; private set; } = [];

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register ViewModels
            foreach (var plugin in Plugins)
            {
                var attr = plugin.GetCustomAttribute<PluginAttribute>();
                if (attr != null && Lucifer.Equals<char>(attr.PluginType, "ViewModel"))
                {
                    ViewModels[attr.Name] = plugin;
                    services.AddSingleton(plugin);
                }
            }

            //services.AddSingleton<ViewModels.MainViewModel>();
            //services.AddSingleton<ViewModels.ProductsViewModel>();

            services.AddSingleton<Services.Interfaces.IDialogService, Services.DialogService>();
            //services.AddSingleton<ViewModels.OrdersViewModel>();
            //services.AddSingleton<ViewModels.ReportsViewModel>();
            //services.AddSingleton<ViewModels.SettingsViewModel>();
            //services.AddSingleton<ViewModels.DashboardViewModel>();
            // Đăng ký các ViewModel khác nếu có


            // Register NavigationService as INavigationService
            services.AddSingleton<Services.INavigationService, Services.NavigationService>();
            services.AddScoped<IProductService, ProductService>();

            // Register CategoryService
            services.AddScoped<ICategoryService, CategoryService>();

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
            services.AddScoped<ICategoryService, CategoryService>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

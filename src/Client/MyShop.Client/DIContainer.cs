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
        public static Dictionary<Type, Type> ViewMapping { get; private set; } = [];

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();

            // Register ViewModels
            foreach (var plugin in Plugins)
            {
                var attr = plugin.GetCustomAttribute<PluginAttribute>();
                if (attr != null && Lucifer.Equals<char>(attr.PluginType, "ViewModel"))
                {
                    ViewModels[attr.Name] = plugin;
                    services.AddSingleton(plugin);

                    if (plugin.Name == "MainViewModel") continue;

                    var viewName = plugin.Name.Replace("ViewModel", "View");
                    var viewType = allTypes.FirstOrDefault(t => t.Name == viewName);
                    if (viewType != null)
                    {
                        ViewMapping[plugin] = viewType;
                    }
                }
            }

            services.AddSingleton<Services.Interfaces.IDialogService, Services.DialogService>();

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
            services.AddScoped<IBRService, BackupService>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

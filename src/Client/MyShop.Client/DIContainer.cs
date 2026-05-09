using LuciferCore.Attributes;
using LuciferCore.Main;
using Microsoft.Extensions.DependencyInjection;
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

                    if (plugin.Name == "MainViewModel" || plugin.Name == "AuthViewModel") continue;

                    var viewName = plugin.Name.Replace("ViewModel", "View");
                    var viewType = allTypes.FirstOrDefault(t => t.Name == viewName);
                    if (viewType != null)
                    {
                        ViewMapping[plugin] = viewType;
                    }
                }
            }


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


            // Register Plugins as Services
            foreach (var plugin in Plugins)
            {
                var attr = plugin.GetCustomAttribute<PluginAttribute>();
                if (attr != null && Lucifer.Equals<char>(attr.PluginType, "Services"))
                {
                    // Tìm Interface tương ứng (Contract)
                    // Ví dụ: AuthService triển khai IAuthService
                    var interfaceType = plugin.GetInterfaces().FirstOrDefault(i => i.Name.StartsWith($"I{plugin.Name}"));

                    if (interfaceType != null)
                    {
                        // Đăng ký dạng Scoped hoặc Singleton tùy nhu cầu
                        services.AddSingleton(interfaceType, plugin);
                    }
                    else
                    {
                        // Nếu không tìm thấy Interface theo chuẩn đặt tên, đăng ký chính nó
                        services.AddSingleton(plugin);
                    }
                }
            }


            // Register Plugins as Service
            foreach (var plugin in Plugins)
            {
                var attr = plugin.GetCustomAttribute<PluginAttribute>();
                if (attr != null && Lucifer.Equals<char>(attr.PluginType, "Service"))
                {
                    // Tìm Interface tương ứng (Contract)
                    // Ví dụ: AuthService triển khai IAuthService
                    var interfaceType = plugin.GetInterfaces().FirstOrDefault(i => i.Name.StartsWith($"I{plugin.Name}"));

                    if (interfaceType != null)
                    {
                        // Đăng ký dạng Scoped hoặc Singleton tùy nhu cầu
                        services.AddScoped(interfaceType, plugin);
                    }
                    else
                    {
                        // Nếu không tìm thấy Interface theo chuẩn đặt tên, đăng ký chính nó
                        services.AddScoped(plugin);
                    }
                }
            }

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.Helpers;
using MyShop.Client.Services.Interfaces;
using System.IO;
using System.Windows;

namespace MyShop.Client
{
    public partial class App : Application
    {
        private ITemporaryDataService? _tempDataService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DIContainer.ConfigureServices();

            // Khởi tạo dịch vụ auto-save
            _tempDataService = DIContainer.ServiceProvider.GetRequiredService<ITemporaryDataService>();
            
            // Bắt đầu dịch vụ auto-save (lưu mỗi 30 giây)
            _tempDataService.Start(saveIntervalMilliseconds: 30000);

            var mainWindow = DIContainer.ServiceProvider.GetRequiredService<MainWindow>();
            //var mainVM = DIContainer.ServiceProvider.GetRequiredService<ViewModels.MainViewModel>();
            //mainWindow.DataContext = mainVM;

            var authVM = DIContainer.ServiceProvider.GetRequiredService<ViewModels.AuthViewModel>();
            mainWindow.DataContext = authVM; // Bây giờ AuthView sẽ tìm thấy các property của nó

            this.MainWindow = mainWindow;
            mainWindow.Show();

            // Đánh dấu là ứng dụng đang chạy
            MarkApplicationRunning();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dừng dịch vụ auto-save
            _tempDataService?.Stop();

            // Xóa dấu "ứng dụng đang chạy" để biết tắt bình thường
            ClearApplicationRunningFlag();

            base.OnExit(e);
        }

        private void MarkApplicationRunning()
        {
            try
            {
                var flagPath = GetApplicationRunningFlagPath();
                File.WriteAllText(flagPath, DateTime.Now.ToString("O"));
            }
            catch
            {
                // Nếu không thể tạo flag, bỏ qua
            }
        }

        private void ClearApplicationRunningFlag()
        {
            try
            {
                var flagPath = GetApplicationRunningFlagPath();
                if (File.Exists(flagPath))
                    File.Delete(flagPath);
            }
            catch
            {
                // Nếu không thể xóa flag, bỏ qua
            }
        }

        private string GetApplicationRunningFlagPath()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyShop"
            );

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return Path.Combine(appDataPath, ".running");
        }
    }

}

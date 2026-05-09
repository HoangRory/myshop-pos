using LuciferCore.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.Helpers;
using MyShop.Client.Services.Interfaces;
using MyShop.Client.Views;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Auth")]
    public class AuthViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;

        public event PropertyChangedEventHandler? PropertyChanged;

        // --- States ---
        private bool _isLoginMode = true;
        public bool IsLoginMode { get => _isLoginMode; set { _isLoginMode = value; OnPropertyChanged(nameof(IsLoginMode)); } }

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public bool RememberMe { get; set; }

        // --- Server Config ---
        public string ServerIP { get; set; } = "localhost";
        public string ServerPort { get; set; } = "8443";
        private bool _showConfig;
        public bool ShowConfig { get => _showConfig; set { _showConfig = value; OnPropertyChanged(nameof(ShowConfig)); } }

        public ICommand SubmitCommand { get; }
        public ICommand ToggleModeCommand { get; }
        public ICommand ToggleConfigCommand { get; }

        public AuthViewModel(IAuthService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;

            SubmitCommand = new RelayCommand(async parameter => await ExecuteSubmit(parameter));
            ToggleModeCommand = new RelayCommand(_ => IsLoginMode = !IsLoginMode);
            ToggleConfigCommand = new RelayCommand(_ => ShowConfig = !ShowConfig);

            // Kiểm tra tự động login nếu có RememberMe
            _ = TryAutoLogin();
        }

        private async Task TryAutoLogin()
        {
            // Logic: Đọc file/registry để lấy account đã lưu
            // Nếu có và RememberMe = true -> gọi _authService.LoginAsync(...)
        }

        // Sửa lại hàm thực thi Command để nhận parameter
        private async Task ExecuteSubmit(object? parameter)
        {
            var passBox = parameter as PasswordBox;
            string actualPassword = passBox?.Password ?? "";

            // Kiểm tra dữ liệu đầu vào cơ bản
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(actualPassword))
            {
                _dialogService.Error("Thông báo", "Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (IsLoginMode)
            {
                // ĐĂNG NHẬP
                if (await _authService.LoginAsync(Username, actualPassword))
                {
                    _dialogService.Success("Thành công", "Chào mừng quay trở lại!");
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        var mainVM = DIContainer.ServiceProvider.GetRequiredService<MainViewModel>();
                        App.Current.MainWindow.DataContext = mainVM;
                    });
                }
                else
                {
                    _dialogService.Error("Thất bại", "Sai tài khoản hoặc mật khẩu.");
                }
            }
            else
            {
                // ĐĂNG KÝ
                // Lưu ý: Để demo nhanh, ta coi như pass khớp. 
                // Nếu muốn check kỹ bạn cần truyền thêm ConfirmPassBox qua MultiBinding
                if (await _authService.SignUpAsync(Username, actualPassword))
                {
                    _dialogService.Success("Thành công", "Đăng ký hoàn tất, mời đăng nhập.");

                    // Chuyển về chế độ đăng nhập
                    IsLoginMode = true;
                    // Gọi VisualStateManager để cập nhật UI về Login
                    if (Application.Current.MainWindow is MainWindow mw && mw.Content is AuthView av)
                    {
                        VisualStateManager.GoToState(av, "LoginState", true);
                    }
                }
                else
                {
                    _dialogService.Error("Lỗi", "Không thể đăng ký tài khoản.");
                }
            }
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
using LuciferCore.Attributes;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using MyShop.Client.Services;
using MyShop.Client.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Main")]
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _pageTitle = "";
        public string PageTitle
        {
            get => _pageTitle;
            set { _pageTitle = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageTitle))); }
        }

        private readonly INavigationService _navigationService;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<PluginMetadata> MenuItems { get; } = new();

        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel))); }
        }

        private string _selectedSection = "Dashboard";
        public string SelectedSection
        {
            get => _selectedSection;
            set { _selectedSection = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSection))); }
        }

        public ICommand NavigateCommand { get; }
        public ICommand ToggleSidebarCommand { get; }
        public ICommand CloseSidebarCommand { get; }
        public ICommand LogoutCommand { get; }

        private bool _isSidebarOpen;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set { _isSidebarOpen = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSidebarOpen))); }
        }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            if (_navigationService is INotifyPropertyChanged navService)
            {
                navService.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(_navigationService.CurrentViewModel))
                    {
                        CurrentViewModel = _navigationService.CurrentViewModel;
                        TryAutoLoad(CurrentViewModel);
                        UpdatePageTitleFromChild(CurrentViewModel);
                    }
                };
            }

            // Lấy toàn bộ Plugin là ViewModel từ DIContainer để tạo Menu
            foreach (var vm in DIContainer.ViewModels)
            {
                // Loại bỏ "Main" vì Main không thể điều hướng đến chính nó
                if (vm.Key == "Main" || vm.Key == "Auth") continue;

                MenuItems.Add(new PluginMetadata
                {
                    Name = vm.Key,
                });
            }

            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));

            ToggleSidebarCommand = new RelayCommand(_ => IsSidebarOpen = !IsSidebarOpen);
            CloseSidebarCommand = new RelayCommand(_ => IsSidebarOpen = false);
            LogoutCommand = new AsyncRelayCommand(_ => ExecuteLogoutAsync());

            ResetToStartup();

        }

        public void ResetToStartup()
        {
            var config = AppConfig.Load();
            var startupSection = config.RememberLastScreen ? config.LastViewModel : config.StartupScreen;

            if (string.IsNullOrWhiteSpace(startupSection))
            {
                startupSection = "Settings";
            }

            if (!MenuItems.Any(item => item.Name.Equals(startupSection, StringComparison.OrdinalIgnoreCase)))
            {
                startupSection = "Settings";
            }

            _selectedSection = startupSection;

            Navigate(startupSection, persistLastView: false);
        }

        private async Task ExecuteLogoutAsync()
        {
            try
            {
                var authService = DIContainer.ServiceProvider.GetRequiredService<IAuthService>();
                await authService.LogoutAsync();
            }
            catch
            {
                // Nếu server không phản hồi thì vẫn cho phép logout cục bộ.
            }

            ClearRememberedAccount();
            ResetToStartup();

            App.Current.MainWindow.DataContext = DIContainer.ServiceProvider.GetRequiredService<AuthViewModel>();
        }

        private static void ClearRememberedAccount()
        {
            var candidates = new[]
            {
                "user_state.json",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_state.json")
            };

            foreach (var filePath in candidates)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch
                {
                    // Bỏ qua nếu file đang bị khóa hoặc không thể xóa.
                }
            }
        }

        private void Navigate(string? viewName, bool persistLastView = true)
        {
            if (string.IsNullOrEmpty(viewName)) return;

            _navigationService.NavigateTo(viewName);
            IsSidebarOpen = false;
            UpdateSelection(viewName);

            // Persist the last opened screen for all navigations, including Settings.
            if (persistLastView)
            {
                var config = AppConfig.Load();
                config.LastViewModel = viewName;
                config.Save();
            }
        }

        private void TryAutoLoad(object? viewModel)
        {
            // Dùng Reflection hoặc Dynamic để gọi hàm Load nếu tồn tại (duck typing)
            var loadMethod = viewModel?.GetType().GetMethod("LoadData");
            loadMethod?.Invoke(viewModel, null);
        }

        private void UpdatePageTitleFromChild(object? viewModel)
        {
            if (viewModel == null)
            {
                PageTitle = string.Empty;
                return;
            }

            var prop = viewModel.GetType().GetProperty("PageTitle");
            if (prop != null)
            {
                var val = prop.GetValue(viewModel) as string;
                if (!string.IsNullOrEmpty(val))
                {
                    PageTitle = val;
                    return;
                }
            }

            var alt = viewModel.GetType().GetProperty("Title") ?? viewModel.GetType().GetProperty("Name");
            if (alt != null)
            {
                PageTitle = alt.GetValue(viewModel)?.ToString() ?? string.Empty;
                return;
            }

            PageTitle = SelectedSection;
        }

        private void UpdateSelection(string viewName)
        {
            SelectedSection = viewName;
            // Cập nhật trạng thái cho từng Item để XAML tự sáng đèn
            foreach (var item in MenuItems)
            {
                item.IsSelected = item.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private const double CompactThresholdForHost = 800.0;

        // Called by the view when the host/control width changes.
        public void HostWidthChanged(double width)
        {
            if (width <= CompactThresholdForHost)
            {
                IsSidebarOpen = false;
            }
        }
    }
}

public class PluginMetadata : INotifyPropertyChanged
{
    public string Name { get; set; } = "";

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

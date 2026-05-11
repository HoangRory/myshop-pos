using LuciferCore.Attributes;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using MyShop.Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

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

            var config = AppConfig.Load();
            _selectedSection = config.LastViewModel;

            // Set default view
            Navigate(SelectedSection);

        }

        private void Navigate(string? viewName)
        {
            if (string.IsNullOrEmpty(viewName)) return;

            _navigationService.NavigateTo(viewName);
            IsSidebarOpen = false;
            UpdateSelection(viewName);

            var config = AppConfig.Load(); // Load để giữ lại IP/Port cũ
            config.LastViewModel = viewName;
            config.Save();
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

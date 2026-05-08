using LuciferCore.Attributes;
using MyShop.Client.Helpers;
using MyShop.Client.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Main")]
    public class MainViewModel : INotifyPropertyChanged
    {
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
                    }
                };
            }

            // Lấy toàn bộ Plugin là ViewModel từ DIContainer để tạo Menu
            foreach (var vm in DIContainer.ViewModels)
            {
                // Loại bỏ "Main" vì Main không thể điều hướng đến chính nó
                if (vm.Key == "Main") continue;

                MenuItems.Add(new PluginMetadata
                {
                    Name = vm.Key,
                });
            }

            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));

            // Set default view
            Navigate(SelectedSection);

        }

        private void Navigate(string? viewName)
        {
            if (string.IsNullOrEmpty(viewName)) return;

            _navigationService.NavigateTo(viewName);
            UpdateSelection(viewName);
        }

        private void TryAutoLoad(object? viewModel)
        {
            // Dùng Reflection hoặc Dynamic để gọi hàm Load nếu tồn tại (duck typing)
            var loadMethod = viewModel?.GetType().GetMethod("LoadData");
            loadMethod?.Invoke(viewModel, null);
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

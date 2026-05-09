using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace MyShop.Client.Services
{
    public class NavigationService : INotifyPropertyChanged, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;

        public event PropertyChangedEventHandler? PropertyChanged;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel == value) return;
                _currentViewModel = value;
                // Phát sự kiện để UI (MainWindow) cập nhật View mới
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
            }
        }

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo(string viewName)
        {
            if (DIContainer.ViewModels.TryGetValue(viewName, out var vmType))
            {
                // DI sẽ tự Resolve instance cùng các Dependency của nó
                CurrentViewModel = _serviceProvider.GetRequiredService(vmType);
            }
        }
    }
}

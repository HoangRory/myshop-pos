using LuciferCore.Attributes;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using MyShop.Client.Services;
using MyShop.Client.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Settings")]
    public class SettingsViewModel : BaseViewModel
    {
        public string PageTitle { get; } = "Cài đặt";

        private readonly IDialogService _dialogService;
        private bool _isLoading;

        private int _savedItemsPerPage;
        private string _savedStartupScreen = "Settings";
        private string _savedServerIP = "localhost";
        private string _savedServerPort = "8443";
        private bool _savedRememberLastScreen;

        private RelayCommand? _saveSettingsCommand;

        private int _itemsPerPage = 10;
        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                if (SetProperty(ref _itemsPerPage, value))
                {
                    UpdateDirtyState();
                }
            }
        }

        private string _lastOpenedScreen = "Dashboard";
        public string LastOpenedScreen
        {
            get => _lastOpenedScreen;
            set => SetProperty(ref _lastOpenedScreen, value);
        }

        private string _startupScreen = "Settings";
        public string StartupScreen
        {
            get => _startupScreen;
            set
            {
                if (SetProperty(ref _startupScreen, value))
                {
                    UpdateDirtyState();
                }
            }
        }

        private bool _rememberLastScreen = false;
        public bool RememberLastScreen
        {
            get => _rememberLastScreen;
            set
            {
                if (SetProperty(ref _rememberLastScreen, value))
                {
                    OnPropertyChanged(nameof(ShowStartupScreenSelection));
                    UpdateDirtyState();
                }
            }
        }

        private string _serverIP = "localhost";
        public string ServerIP
        {
            get => _serverIP;
            set
            {
                if (SetProperty(ref _serverIP, value))
                {
                    UpdateDirtyState();
                }
            }
        }

        private string _serverPort = "8443";
        public string ServerPort
        {
            get => _serverPort;
            set
            {
                if (SetProperty(ref _serverPort, value))
                {
                    UpdateDirtyState();
                }
            }
        }

        private bool _hasUnsavedChanges;
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            private set
            {
                if (SetProperty(ref _hasUnsavedChanges, value))
                {
                    OnPropertyChanged(nameof(SaveButtonText));
                    _saveSettingsCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ShowStartupScreenSelection => !RememberLastScreen;

        public string SaveButtonText => HasUnsavedChanges ? "Lưu thay đổi" : "Lưu cài đặt";

        public ObservableCollection<int> ItemsPerPageOptions { get; }
        public ObservableCollection<string> StartupScreenOptions { get; }
        public ICommand SaveSettingsCommand => _saveSettingsCommand ??= new RelayCommand(_ => SaveSettings(), _ => HasUnsavedChanges);

        public SettingsViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            ItemsPerPageOptions = new ObservableCollection<int> { 5, 10, 15, 20 };
            StartupScreenOptions = new ObservableCollection<string>(BuildStartupScreenOptions());
            LoadSettings();
        }

        private void LoadSettings()
        {
            _isLoading = true;
            var config = AppConfig.Load();
            ItemsPerPage = config.ItemsPerPage;
            LastOpenedScreen = config.LastViewModel;
            RememberLastScreen = config.RememberLastScreen;
            StartupScreen = NormalizeStartupScreen(config.StartupScreen);
            ServerIP = config.ServerIP;
            ServerPort = config.ServerPort;

            _savedItemsPerPage = ItemsPerPage;
            _savedStartupScreen = StartupScreen;
            _savedServerIP = ServerIP;
            _savedServerPort = ServerPort;
            _savedRememberLastScreen = RememberLastScreen;
            HasUnsavedChanges = false;
            _isLoading = false;

            OnPropertyChanged(nameof(ShowStartupScreenSelection));
            OnPropertyChanged(nameof(SaveButtonText));
            _saveSettingsCommand?.RaiseCanExecuteChanged();
        }

        private void SaveSettings()
        {
            try
            {
                var config = AppConfig.Load();
                config.ItemsPerPage = ItemsPerPage;
                config.RememberLastScreen = RememberLastScreen;
                config.StartupScreen = StartupScreen;
                config.ServerIP = ServerIP;
                config.ServerPort = ServerPort;
                config.Save();

                _savedItemsPerPage = ItemsPerPage;
                _savedStartupScreen = StartupScreen;
                _savedServerIP = ServerIP;
                _savedServerPort = ServerPort;
                _savedRememberLastScreen = RememberLastScreen;
                HasUnsavedChanges = false;

                // Notify other ViewModels about settings change
                AppSettingsService.NotifyItemsPerPageChanged();

                _dialogService.Success("Thành công", "Cài đặt đã được lưu.");
            }
            catch
            {
                _dialogService.Error("Lỗi", "Không thể lưu cài đặt.");
            }
        }

        private void UpdateDirtyState()
        {
            if (_isLoading)
            {
                return;
            }

            HasUnsavedChanges = ItemsPerPage != _savedItemsPerPage
                || !string.Equals(StartupScreen, _savedStartupScreen, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(ServerIP, _savedServerIP, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(ServerPort, _savedServerPort, StringComparison.OrdinalIgnoreCase)
                || RememberLastScreen != _savedRememberLastScreen;
        }

        private IEnumerable<string> BuildStartupScreenOptions()
        {
            var configuredScreens = DIContainer.ViewModels.Keys
                .Where(name => !name.Equals("Main", StringComparison.OrdinalIgnoreCase)
                    && !name.Equals("Auth", StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name)
                .ToList();

            if (configuredScreens.Count > 0)
            {
                return configuredScreens;
            }

            return new[] { "Dashboard", "Orders", "Products", "Reports", "Settings", "BR" };
        }

        private string NormalizeStartupScreen(string startupScreen)
        {
            if (StartupScreenOptions.Any(option => option.Equals(startupScreen, StringComparison.OrdinalIgnoreCase)))
            {
                return StartupScreenOptions.First(option => option.Equals(startupScreen, StringComparison.OrdinalIgnoreCase));
            }

            return StartupScreenOptions.FirstOrDefault(option => option.Equals("Settings", StringComparison.OrdinalIgnoreCase))
                ?? StartupScreenOptions.FirstOrDefault()
                ?? "Settings";
        }
    }
}

using System;
using System.Windows.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Services;
using MyShop.Client.ViewModels;

namespace MyShop.Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigateProductsCommand { get; }
        public ICommand NavigateOrdersCommand { get; }
        public ICommand NavigateReportsCommand { get; }
        public ICommand NavigateSettingsCommand { get; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            NavigateDashboardCommand = new RelayCommand(_ => NavigateTo<DashboardViewModel>());
            NavigateProductsCommand = new RelayCommand(_ => NavigateTo<ProductViewModel>());
            NavigateOrdersCommand = new RelayCommand(_ => NavigateTo<OrdersViewModel>());
            NavigateReportsCommand = new RelayCommand(_ => NavigateTo<ReportsViewModel>());
            NavigateSettingsCommand = new RelayCommand(_ => NavigateTo<SettingsViewModel>());

            // Listen for navigation changes
            if (_navigationService is BaseViewModel navVm)
            {
                navVm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(_navigationService.CurrentViewModel))
                    {
                        CurrentViewModel = _navigationService.CurrentViewModel;
                    }
                };
            }

            // Set default view
            NavigateTo<DashboardViewModel>();
        }

        private void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            _navigationService.NavigateTo<TViewModel>();
            CurrentViewModel = _navigationService.CurrentViewModel;
        }
    }
}

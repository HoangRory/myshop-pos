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

        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
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
            NavigateProductsCommand = new RelayCommand(_ => NavigateTo<ProductsViewModel>());
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
                        TryLoadProductsViewModel();
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
            TryLoadProductsViewModel();
            UpdateSelectedSection<TViewModel>();
        }

        private void UpdateSelectedSection<TViewModel>() where TViewModel : BaseViewModel
        {
            // Reset all
            IsDashboardSelected = false;
            IsProductsSelected = false;
            IsOrdersSelected = false;
            IsReportsSelected = false;
            IsSettingsSelected = false;

            var t = typeof(TViewModel);
            if (t == typeof(DashboardViewModel))
                IsDashboardSelected = true;
            else if (t == typeof(ProductsViewModel))
                IsProductsSelected = true;
            else if (t == typeof(OrdersViewModel))
                IsOrdersSelected = true;
            else if (t == typeof(ReportsViewModel))
                IsReportsSelected = true;
            else if (t == typeof(SettingsViewModel))
                IsSettingsSelected = true;
        }

        private bool _isDashboardSelected;
        public bool IsDashboardSelected { get => _isDashboardSelected; set => SetProperty(ref _isDashboardSelected, value); }

        private bool _isProductsSelected;
        public bool IsProductsSelected { get => _isProductsSelected; set => SetProperty(ref _isProductsSelected, value); }

        private bool _isOrdersSelected;
        public bool IsOrdersSelected { get => _isOrdersSelected; set => SetProperty(ref _isOrdersSelected, value); }

        private bool _isReportsSelected;
        public bool IsReportsSelected { get => _isReportsSelected; set => SetProperty(ref _isReportsSelected, value); }

        private bool _isSettingsSelected;
        public bool IsSettingsSelected { get => _isSettingsSelected; set => SetProperty(ref _isSettingsSelected, value); }

        private void TryLoadProductsViewModel()
        {
            if (CurrentViewModel is ProductsViewModel productsVm && !productsVm.IsLoaded)
            {
                if (productsVm.LoadProductsCommand.CanExecute(null))
                    productsVm.LoadProductsCommand.Execute(null);
            }
        }
    }
}


using System;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Services
{
    public class NavigationService : BaseViewModel, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            CurrentViewModel = viewModel;
        }
    }
}

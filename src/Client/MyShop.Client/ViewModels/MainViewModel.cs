using MyShop.Client.ViewModels;

namespace MyShop.Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly Services.INavigationService _navigationService;

        public Services.INavigationService NavigationService => _navigationService;

        public BaseViewModel CurrentViewModel => _navigationService.CurrentViewModel;

        public MainViewModel(Services.INavigationService navigationService)
        {
            _navigationService = navigationService;
            if (_navigationService is BaseViewModel navVm)
            {
                navVm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(_navigationService.CurrentViewModel))
                    {
                        OnPropertyChanged(nameof(CurrentViewModel));
                    }
                };
            }
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            _navigationService.NavigateTo<TViewModel>();
        }
    }
}

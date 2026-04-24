using MyShop.Client.ViewModels;

namespace MyShop.Client.Services
{
    public interface INavigationService
    {
        BaseViewModel CurrentViewModel { get; }
        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
    }
}

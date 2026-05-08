namespace MyShop.Client.Services
{
    public interface INavigationService
    {
        object? CurrentViewModel { get; }
        void NavigateTo(string viewName);
    }
}

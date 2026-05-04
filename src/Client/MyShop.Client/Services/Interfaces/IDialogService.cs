namespace MyShop.Client.Services.Interfaces
{
    public interface IDialogService
    {
        bool Confirm(string title, string message);
        void Success(string title, string message);
    }
}

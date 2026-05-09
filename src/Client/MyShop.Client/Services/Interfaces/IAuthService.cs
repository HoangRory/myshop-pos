namespace MyShop.Client.Services.Interfaces
{
    public interface IAuthService : IAPI
    {
        Task<bool> LoginAsync(string username, string password, bool isHashed = false);
        Task<bool> LogoutAsync();
        Task<bool> SignUpAsync(string username, string password);
    }
}

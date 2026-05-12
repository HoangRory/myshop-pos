namespace MyShop.Client.Services.Interfaces
{
    public interface IAuthService : IAPI
    {
        string AccountId { get; }
        event Action<List<string>>? OnRecoveryRequested;
        Task RequestRecoveryAsync(List<string> viewModels);
        Task<bool> LoginAsync(string username, string password, bool isHashed = false);
        Task<bool> LogoutAsync();
        Task<bool> SignUpAsync(string username, string password);
    }
}

using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Server.Contract;
using Server.Models;
using System.Linq.Expressions;

namespace Server.Handler.Auth;

public class AuthService
{
    public async Task<ResponseModel> Login(Account? account)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (account == null || string.IsNullOrEmpty(account.Username) || string.IsNullOrEmpty(account.PasswordHash))
        {
            response.MakeCustomResponse<byte, char, byte>(400, StorageData.Http11Protocol, "Bad request.", StorageData.TextPlainCharset);
            return response;
        }

        var repo = Lucifer.GetModelT<IRepository<Account>>();

        Expression<Func<Account, bool>> filter = a => a.Username == account.Username;

        var accounts = await repo.GetAsync(filter);
        var accountFromDb = accounts.FirstOrDefault();

        if (accountFromDb == null || accountFromDb.PasswordHash != account.PasswordHash)
        {
            response.MakeCustomResponse<byte, char, byte>(401, StorageData.Http11Protocol, "Invalid username or password.", StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, "Login successful.", StorageData.TextPlainCharset);

        return response;
    }

    public async Task<ResponseModel> Signup(Account? account)
    {
        var error = ValidateAccount(account);
        var response = Lucifer.Rent<ResponseModel>();

        if (error != null)
        {
            response.MakeCustomResponse<byte, char, byte>(400, StorageData.Http11Protocol, error, StorageData.TextPlainCharset);
            return response;
        }

        if (account == null || string.IsNullOrEmpty(account.Username) || string.IsNullOrEmpty(account.PasswordHash))
        {
            response.MakeCustomResponse<byte, char, byte>(400, StorageData.Http11Protocol, "Invalid username or password.", StorageData.TextPlainCharset);
            return response;
        }

        var repo = Lucifer.GetModelT<IRepository<Account>>();
        Expression<Func<Account, bool>> filter = a => a.Username == account.Username;

        var accounts = await repo.GetAsync(filter);
        if (accounts.Any())
        {
            response.MakeCustomResponse<byte, char, byte>(409, StorageData.Http11Protocol, "Username already exists.", StorageData.TextPlainCharset);
            return response;
        }

        account.Role = "Staff";
        account.IsActive = true;
        account.CreatedAt = DateTime.Now;

        var result = await repo.AddAsync(account);

        if (result != 1)
        {
            response.MakeCustomResponse<byte, char, byte>(500, StorageData.Http11Protocol, "Failed to create account.", StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(201, StorageData.Http11Protocol, "Signup successful.", StorageData.TextPlainCharset);
        return response;
    }

    private string? ValidateAccount(Account? acc)
    {
        if (acc == null) return "Account must not be null.";
        if (acc.Username.Length < 5) return "Username must be at least 5 characters.";
        if (acc.PasswordHash.Length < 8) return "Password too short.";
        if (!string.IsNullOrEmpty(acc.Email) && !acc.Email.Contains("@")) return "Invalid email.";
        return null; // OK
    }
}

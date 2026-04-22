using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Service;
using LuciferCore.Storage;
using Server.Core;
using Server.Models;

namespace Server.Handler.Auth;

[Handler("v1", "/api/auth")]
public class AuthHandler : RouteHandler
{
    private readonly AuthService AuthService = new AuthService();

    [RateLimiter(100, 60)]
    [Authorize(UserRole.Guest)]
    [HttpPost("/login")]
    private async Task Login([Session] AppSession session, [Data] RequestModel request)
    {
        ResponseModel response;
        if (Lucifer.Authorization(session, out var _, out var _))
        {
            response = Lucifer.Rent<ResponseModel>();
            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Already logged in"u8, StorageData.TextPlainCharset);
            session.SendResponseAsync(response);
            return;
        }

        var account = request.BodySpan.FromJson<Account>();
        using var _ = response = await AuthService.Login(account);

        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.Guest)]
    [HttpPost("/signup")]
    private async Task Signup([Session] AppSession session, [Data] RequestModel request)
    {
        ResponseModel response;

        if (Lucifer.Authorization(session, out var _, out var _))
        {
            response = Lucifer.Rent<ResponseModel>();
            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Already logged in"u8, StorageData.TextPlainCharset);
            session.SendResponseAsync(response);
            return;
        }

        var account = request.BodySpan.FromJson<Account>();
        using var _ = response = await AuthService.Signup(account);

        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("/logout")]
    private void Logout([Session] AppSession session, [Data] RequestModel request)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (Lucifer.Deauthorize(session))
        {
            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Logged out successfully"u8, StorageData.TextPlainCharset);
            session.SendResponseAsync(response);
            return;
        }

        response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Logout failed"u8, StorageData.TextPlainCharset);
        session.SendResponseAsync(response);
    }

}

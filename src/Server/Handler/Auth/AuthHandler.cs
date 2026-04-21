using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Auth;

[Handler("v1", "/api/auth")]
public class AuthHandler : RouteHandler
{
    [RateLimiter(100, 60)]
    [Authorize(UserRole.Guest)]
    [HttpPost("/login")]
    private void Login([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.Guest)]
    [HttpPost("/signup")]
    private void Signup([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("/logout")]
    private void Logout([Session] AppSession session, [Data] RequestModel request)
    {

    }

}

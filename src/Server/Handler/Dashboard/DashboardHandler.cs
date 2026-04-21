using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Dashboard;

[Handler("v1", "/api/dashboard")]
public class DashboardHandler : RouteHandler
{
    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("")]
    private void GetDashboard([Session] AppSession session, [Data] RequestModel request)
    {

    }
}

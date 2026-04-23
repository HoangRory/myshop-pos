using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Dashboard;

[Handler("v1", "/api/dashboard")]
public class DashboardHandler : RouteHandler
{
    private readonly DashboardService _dashboardService = new();

#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpGet("")]
    private async Task GetDashboard([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _dashboardService.GetDashboardData();
        session.SendResponseAsync(response);
    }
}

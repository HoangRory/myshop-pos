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

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpGet("")]
    private async Task GetDashboard([Session] AppSession session, [Data] RequestModel request)
    {
        var response = await _dashboardService.GetDashboardData();
        session.SendResponseAsync(response);
    }
}

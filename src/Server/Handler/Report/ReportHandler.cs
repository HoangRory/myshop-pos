using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Report;

[Handler("v1", "/api/report")]
public class ReportHandler : RouteHandler
{
    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("/product")]
    private void GetProductReport([Session] AppSession session, [Data] RequestModel request)
    {
    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("/revenue")]
    private void GetRevenueReport([Session] AppSession session, [Data] RequestModel request)
    {
    }
}

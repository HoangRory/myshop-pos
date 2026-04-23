using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Report;

[Handler("v1", "/api/report")]
public class ReportHandler : RouteHandler
{
    private readonly ReportService _reportService = new();

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpPost("/product")]
    private async Task GetProductReport([Session] AppSession session, [Data] RequestModel request)
    {
        var filter = request.BodySpan.FromJson<ReportFilter>();
        using var response = await _reportService.GetProductReport(filter);
        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpPost("/revenue")]
    private async Task GetRevenueReport([Session] AppSession session, [Data] RequestModel request)
    {
        var filter = request.BodySpan.FromJson<ReportFilter>();
        using var response = await _reportService.GetRevenueReport(filter);
        session.SendResponseAsync(response);
    }
}

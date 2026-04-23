using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Order;

[Handler("v1", "/api/order")]
public class OrderHandler : RouteHandler
{
    private readonly OrderService _service = new();

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpGet("")]
    private async Task GetOrders([Session] AppSession session, [Data] RequestModel request)
    {
        var response = await _service.GetOrders();
        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpGet("/id")]
    private async Task GetOrderDetail([Session] AppSession session, [Data] RequestModel request)
    {
        var order = request.BodySpan.FromJson<Models.Order>();
        using var response = await _service.GetOrder(order);
        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpPost("/search")]
    private async Task SearchOrders([Session] AppSession session, [Data] RequestModel request)
    {
        var filters = request.BodySpan.FromJson<OrderFilter>();
        using var response = await _service.SearchOrders(filters);
        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpPost("")]
    private async Task CreateOrder([Session] AppSession session, [Data] RequestModel request)
    {
        var items = request.BodySpan.FromJson<List<Models.OrderItem>>();
        using var response = await _service.CreateOrder(items);
        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpPut("")]
    private async Task UpdateOrder([Session] AppSession session, [Data] RequestModel request)
    {
        var updateData = request.BodySpan.FromJson<Models.Order>();
        using var response = await _service.UpdateOrder(updateData);
        session.SendResponseAsync(response);
    }

    [RateLimiter(100, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [HttpDelete("")]
    private async Task DeleteOrder([Session] AppSession session, [Data] RequestModel request)
    {
        var order = request.BodySpan.FromJson<Models.Order>();
        using var response = await _service.DeleteOrder(order);
        session.SendResponseAsync(response);
    }
}

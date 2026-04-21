using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Order;

[Handler("v1", "/api/order")]
public class OrderHandler : RouteHandler
{
    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpGet("")]
    private void GetOrders([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpGet("/id")]
    private void GetOrderDetail([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpGet("/date")]
    private void GetOrdersByDate([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("")]
    private void CreateOrder([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPut("")]
    private void UpdateOrder([Session] AppSession session, [Data] RequestModel request)
    {
    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.Admin)]
    [HttpDelete("")]
    private void DeleteOrder([Session] AppSession session, [Data] RequestModel request)
    {

    }
}

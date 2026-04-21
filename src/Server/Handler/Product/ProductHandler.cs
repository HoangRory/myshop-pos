using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Product;

[Handler("v1", "/api/product")]
public class ProductHandler : RouteHandler
{
    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpGet("")]
    private void GetProducts([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpGet("/id")]
    private void GetProductDetail([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("")]
    private void CreateProduct([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPost("/search")]
    private void SearchProducts([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.User)]
    [HttpPut("")]
    private void UpdateProduct([Session] AppSession session, [Data] RequestModel request)
    {

    }

    [RateLimiter(100, 60)]
    [Authorize(UserRole.Admin)]
    [HttpDelete("")]
    private void DeleteProduct([Session] AppSession session, [Data] RequestModel request)
    {

    }
}

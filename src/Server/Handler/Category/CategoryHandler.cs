using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Category;

[Handler("v1", "/api/category")]
public class CategoryHandler : RouteHandler
{
    private readonly CategoryService _categoryService = new();

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("/id")]
    private async Task GetCategoryDetail([Session] AppSession session, [Data] RequestModel request)
    {
        var category = request.BodySpan.FromJson<Models.Category>();
        using var response = await _categoryService.GetCategoryDetail(category);
        session.SendResponseAsync(response);
    }
#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpGet("")]
    private async Task GetCategories([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _categoryService.GetCategories();
        session.SendResponseAsync(response);
    }


#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpPut("")]
    private async Task UpdateCategory([Session] AppSession session, [Data] RequestModel request)
    {
        var category = request.BodySpan.FromJson<Models.Category>();
        using var response = await _categoryService.UpdateCategory(category);
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("")]
    private async Task CreateCategory([Session] AppSession session, [Data] RequestModel request)
    {
        var category = request.BodySpan.FromJson<Models.Category>();
        using var response = await _categoryService.CreateCategory(category);
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpDelete("")]
    private async Task DeleteCategory([Session] AppSession session, [Data] RequestModel request)
    {
        var category = request.BodySpan.FromJson<Models.Category>();
        using var response = await _categoryService.DeleteCategory(category);
        session.SendResponseAsync(response);
    }

}

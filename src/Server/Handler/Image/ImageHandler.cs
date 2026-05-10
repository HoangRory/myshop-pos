using LuciferCore.Attributes;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Image;

[Handler("v1", "/api/image")]
public class ImageHandler : RouteHandler
{
    private readonly ImageService _imageService = new();

#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("upload")]
    private async Task Upload([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _imageService.UploadImage(request);
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpGet("download")]
    private async Task Download([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _imageService.DownloadImage(request);
        session.SendResponseAsync(response);
    }
}

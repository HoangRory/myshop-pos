using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.BR;

[Handler("v1", "/api/backup-restore")]
public class BackupRestoreHandler : RouteHandler
{
    private readonly BackupRestoreService _backupRestoreService = new();

    [RateLimiter(10, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpPost("restore")]
    private async Task Restore([Session] AppSession session, [Data] RequestModel request)
    {
        var bk = request.BodySpan.FromJson<BackupRestore>();
        using var response = await _backupRestoreService.Restore(bk);
        session.SendResponseAsync(response);
    }

    [RateLimiter(10, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpGet("backup")]
    private async Task CreateBackup([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _backupRestoreService.CreateBackup();
        session.SendResponseAsync(response);
    }

    [RateLimiter(10, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpGet("auto-backup")]
    private async Task AutoBackup([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _backupRestoreService.SetAutoBackup();
        session.SendResponseAsync(response);
    }

    [RateLimiter(10, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpGet("")]
    private async Task GetAllBackups([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _backupRestoreService.GetAllBackups();
        session.SendResponseAsync(response);
    }

    [RateLimiter(10, 60)]
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [HttpDelete("")]
    private async Task DeleteBackup([Session] AppSession session, [Data] RequestModel request)
    {
        var bk = request.BodySpan.FromJson<BackupRestore>();
        using var response = await _backupRestoreService.DeleteBackup(bk);
        session.SendResponseAsync(response);
    }


}

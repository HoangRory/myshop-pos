using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Model;
using LuciferCore.Service;
using Server.Core;

namespace Server.Handler.Voucher;

[Handler("v1", "/api/voucher")]
public class VoucherHandler : RouteHandler
{
    private readonly VoucherService _voucherService = new();

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("/id")]
    private async Task GetVoucherDetail([Session] AppSession session, [Data] RequestModel request)
    {
        var voucher = request.BodySpan.FromJson<Models.DiscountVoucher>();
        using var response = await _voucherService.GetVoucherDetail(voucher);
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpGet("")]
    private async Task GetVouchers([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _voucherService.GetVouchers();
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpPut("")]
    private async Task UpdateVoucher([Session] AppSession session, [Data] RequestModel request)
    {
        var voucher = request.BodySpan.FromJson<Models.DiscountVoucher>();
        using var response = await _voucherService.UpdateVoucher(voucher);
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("")]
    private async Task CreateVoucher([Session] AppSession session, [Data] RequestModel request)
    {
        var voucher = request.BodySpan.FromJson<Models.DiscountVoucher>();
        using var response = await _voucherService.CreateVoucher(voucher);
        session.SendResponseAsync(response);
    }

#if DEBUG
    [Authorize(UserRole.Guest)]
#else
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpDelete("")]
    private async Task DeleteVoucher([Session] AppSession session, [Data] RequestModel request)
    {
        var voucher = request.BodySpan.FromJson<Models.DiscountVoucher>();
        using var response = await _voucherService.DeleteVoucher(voucher);
        session.SendResponseAsync(response);
    }
}

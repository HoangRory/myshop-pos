using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Server.Contract;

namespace Server.Handler.Voucher;

public class VoucherService
{
    public async Task<ResponseModel> GetVoucherDetail(Models.DiscountVoucher? voucher)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (voucher == null || string.IsNullOrEmpty(voucher.VoucherCode))
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }
        var db = Lucifer.GetModelT<IRepository<Models.DiscountVoucher>>();
        var result = await db.GetByIdAsync(voucher.VoucherCode);
        if (result == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
            return response;
        }
        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, result.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> GetVouchers()
    {
        var response = Lucifer.Rent<ResponseModel>();
        var db = Lucifer.GetModelT<IRepository<Models.DiscountVoucher>>();
        var result = await db.GetAsync();
        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, result.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> UpdateVoucher(Models.DiscountVoucher? voucher)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (voucher == null || string.IsNullOrEmpty(voucher.VoucherCode))
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }
        var db = Lucifer.GetModelT<IRepository<Models.DiscountVoucher>>();
        var result = await db.UpdateAsync(voucher);
        if (result <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Update Failed"u8, StorageData.TextPlainCharset);
            return response;
        }
        response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "OK"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> CreateVoucher(Models.DiscountVoucher? voucher)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (voucher == null || string.IsNullOrEmpty(voucher.VoucherCode))
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }
        var db = Lucifer.GetModelT<IRepository<Models.DiscountVoucher>>();
        var result = await db.AddAsync(voucher);
        if (result <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Create Failed"u8, StorageData.TextPlainCharset);
            return response;
        }
        response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "OK"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> DeleteVoucher(Models.DiscountVoucher? voucher)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (voucher == null || string.IsNullOrEmpty(voucher.VoucherCode))
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }
        var db = Lucifer.GetModelT<IRepository<Models.DiscountVoucher>>();
        var result = await db.DeleteByIdAsync(voucher.VoucherCode);
        if (result <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Delete Failed"u8, StorageData.TextPlainCharset);
            return response;
        }
        response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "OK"u8, StorageData.TextPlainCharset);
        return response;
    }
}

using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Server.Contract;

namespace Server.Handler.Category;

public class CategoryService
{
    public async Task<ResponseModel> GetCategoryDetail(Models.Category? category)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (category == null || category.CategoryId <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var db = Lucifer.GetModelT<IRepository<Models.Category>>();
        var result = await db.GetByIdAsync(category.CategoryId);

        if (result == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, result.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> GetCategories()
    {
        var response = Lucifer.Rent<ResponseModel>();
        var db = Lucifer.GetModelT<IRepository<Models.Category>>();
        var result = await db.GetAsync();
        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, result.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> UpdateCategory(Models.Category? category)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (category == null || category.CategoryId <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }
        var db = Lucifer.GetModelT<IRepository<Models.Category>>();
        var result = await db.UpdateAsync(category);
        if (result <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Update Failed"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "OK"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> CreateCategory(Models.Category? category)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (category == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var db = Lucifer.GetModelT<IRepository<Models.Category>>();

        var result = await db.AddAsync(category);
        if (result <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Add Failed"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "OK"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> DeleteCategory(Models.Category? category)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (category == null || category.CategoryId <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var db = Lucifer.GetModelT<IRepository<Models.Category>>();
        var result = await db.DeleteByIdAsync(category.CategoryId);

        if (result <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Delete Failed"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "OK"u8, StorageData.TextPlainCharset);
        return response;
    }

}

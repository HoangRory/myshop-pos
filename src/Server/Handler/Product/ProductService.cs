using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Server.Handler.Product;

using Server.Contract;
using Server.Models;

public class ProductService
{
    public async Task<ResponseModel> GetProduct(Product? product)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (product == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var db = Lucifer.GetModelT<IRepository<Product>>();

        var productData = await db.GetByIdAsync(product.ProductId);

        if (productData == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, productData.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> GetProducts()
    {
        var response = Lucifer.Rent<ResponseModel>();
        var db = Lucifer.GetModelT<IRepository<Product>>();
        var products = await db.GetAsync();

        if (products == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, products.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> SearchProducts(ProductFilter? filter)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (filter == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();

        var query = db.Set<Product>().AsNoTracking().AsQueryable();

        // 1. Lọc theo loại
        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId);

        // 2. Tìm kiếm từ khóa (Keyword)
        if (!string.IsNullOrEmpty(filter.Keyword))
            query = query.Where(p => p.Name.Contains(filter.Keyword));

        // 3. Lọc theo khoảng giá
        if (filter.MinPrice.HasValue) query = query.Where(p => p.SalePrice >= filter.MinPrice);
        if (filter.MaxPrice.HasValue) query = query.Where(p => p.SalePrice <= filter.MaxPrice);

        // 4. Sắp xếp dynamic
        query = filter.SortBy.ToLower() switch
        {
            "price" => filter.IsAscending ? query.OrderBy(p => p.SalePrice) : query.OrderByDescending(p => p.SalePrice),
            "name" => filter.IsAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => filter.IsAscending ? query.OrderBy(p => p.ProductId) : query.OrderByDescending(p => p.ProductId)
        };

        // 5. Phân trang (Paging)
        var totalItems = await query.CountAsync();
        var items = await query.Skip((filter.PageIndex - 1) * filter.PageSize)
                               .Take(filter.PageSize)
                               .ToListAsync();

        var result = new { Total = totalItems, Data = items };
        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, result.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> AddProduct(Product? product)
    {
        var response = Lucifer.Rent<ResponseModel>();

        if (product == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var repo = Lucifer.GetModelT<IRepository<Product>>();

        product.UpdatedAt = DateTime.Now;
        var result = await repo.AddAsync(product);

        response.MakeCustomResponse<byte, byte, byte>(result > 0 ? 201 : 500, StorageData.Http11Protocol,
            result > 0 ? "Success"u8 : "Fail"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> UpdateProduct(Product? product)
    {
        var response = Lucifer.Rent<ResponseModel>();
        var repo = Lucifer.GetModelT<IRepository<Product>>();

        if (product == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        // Logic check tồn tại trước khi update
        var result = await repo.UpdateAsync(product);

        response.MakeCustomResponse<byte, byte, byte>(result > 0 ? 200 : 404, StorageData.Http11Protocol,
            result > 0 ? "Updated"u8 : "Not Found"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> DeleteProduct(Product? product)
    {
        var response = Lucifer.Rent<ResponseModel>();

        if (product == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var repo = Lucifer.GetModelT<IRepository<Product>>();

        var result = await repo.DeleteByIdAsync(product.ProductId);

        response.MakeCustomResponse<byte, byte, byte>(result > 0 ? 200 : 404, StorageData.Http11Protocol,
            result > 0 ? "Deleted"u8 : "Not Found"u8, StorageData.TextPlainCharset);
        return response;
    }

    public async Task<ResponseModel> ImportProducts(List<Product>? products)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (products == null || products.Count == 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "No data to import"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();
        var strategy = db.Database.CreateExecutionStrategy();

        // Sử dụng Strategy để đảm bảo an toàn dữ liệu khi insert số lượng lớn
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                foreach (var p in products)
                {
                    // Reset ID để DB tự tạo mới nếu là import hàng mới
                    p.ProductId = 0;
                    p.UpdatedAt = now;
                }

                await db.Set<Product>().AddRangeAsync(products);
                var result = await db.SaveChangesAsync();

                await transaction.CommitAsync();

                response.MakeCustomResponse<byte, char, byte>(201, StorageData.Http11Protocol, $"Imported {result} products", StorageData.TextPlainCharset);
                return response;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Import failed due to data error"u8, StorageData.TextPlainCharset);
                return response;
            }
        });

    }
}

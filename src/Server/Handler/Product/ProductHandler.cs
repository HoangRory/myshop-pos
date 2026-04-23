using LuciferCore.Attributes;
using LuciferCore.Extensions;
using LuciferCore.Handler;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Service;
using LuciferCore.Storage;
using Server.Core;
using Server.Models;

namespace Server.Handler.Product;

[Handler("v1", "/api/product")]
public class ProductHandler : RouteHandler
{
    private readonly ProductService _productService = new();

    // Xem danh sách toàn bộ sản phẩm
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpGet("")]
    private async Task GetProducts([Session] AppSession session, [Data] RequestModel request)
    {
        using var response = await _productService.GetProducts();
        session.SendResponseAsync(response);
    }

    // Xem chi tiết sản phẩm theo ID
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpGet("/id")]
    private async Task GetProductDetail([Session] AppSession session, [Data] RequestModel request)
    {
        var product = request.BodySpan.FromJson<Models.Product>();
        using var response = await _productService.GetProduct(product);
        session.SendResponseAsync(response);
    }

    // Tạo mới sản phẩm
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("")]
    private async Task CreateProduct([Session] AppSession session, [Data] RequestModel request)
    {
        var product = request.BodySpan.FromJson<Models.Product>();
        using var response = await _productService.AddProduct(product);
        session.SendResponseAsync(response);
    }

    // Tìm kiếm sản phẩm theo bộ lọc
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("/search")]
    private async Task SearchProducts([Session] AppSession session, [Data] RequestModel request)
    {
        var filter = request.BodySpan.FromJson<ProductFilter>();
        using var response = await _productService.SearchProducts(filter);
        session.SendResponseAsync(response);
    }

    // Cập nhật thông tin sản phẩm
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.User)]
#endif
    [RateLimiter(100, 60)]
    [HttpPut("")]
    private async Task UpdateProduct([Session] AppSession session, [Data] RequestModel request)
    {
        var product = request.BodySpan.FromJson<Models.Product>();
        using var response = await _productService.UpdateProduct(product);
        session.SendResponseAsync(response);
    }

    // Xoá sản phẩm
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpDelete("")]
    private async Task DeleteProduct([Session] AppSession session, [Data] RequestModel request)
    {
        var product = request.BodySpan.FromJson<Models.Product>();
        using var response = await _productService.DeleteProduct(product);
        session.SendResponseAsync(response);
    }

    // Import sản phẩm từ file CSV
#if DEBUG
    [Authorize(UserRole.Guest)]
#else 
    [Authorize(UserRole.Admin)]
#endif
    [RateLimiter(100, 60)]
    [HttpPost("/import")]
    private async Task ImportProduct([Session] AppSession session, [Data] RequestModel request)
    {
        using var stream = new MemoryStream(request.BodySpan.ToArray());

        try
        {
            // Lưu ý: Client cần gửi file đúng định dạng
            var products = MiniExcelLibs.MiniExcel.Query<Models.Product>(stream).ToList();

            // 3. Gọi Service để lưu vào DB
            using var response = await _productService.ImportProducts(products);
            session.SendResponseAsync(response);
        }
        catch (Exception)
        {
            using var response = Lucifer.Rent<ResponseModel>();
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Invalid file format"u8, StorageData.TextPlainCharset);
            session.SendResponseAsync(response);
        }
    }
}

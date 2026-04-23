using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Microsoft.EntityFrameworkCore;
using Server.Contract;
using Server.Database.EFcore;

namespace Server.Handler.Order;

public class OrderService
{
    public async Task<ResponseModel> GetOrder(Models.Order? order)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (order == null || order.OrderId <= 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        var db = Lucifer.GetModelT<IRepository<Models.Order>>();

        var orderData = await db.GetByIdAsync(order.OrderId);

        if (orderData == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, orderData.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> GetOrders()
    {
        var response = Lucifer.Rent<ResponseModel>();
        var db = Lucifer.GetModelT<IRepository<Models.Order>>();
        var orders = await db.GetAsync();

        if (orders == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
            return response;
        }

        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, orders.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> SearchOrders(OrderFilter? filter)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (filter == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();
        var query = db.Set<Models.Order>().AsNoTracking().AsQueryable();

        // Lọc theo ngày
        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate);
        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate);

        // Lọc theo trạng thái
        if (filter.Status.HasValue)
            query = query.Where(o => (int?)o.Status == filter.Status);

        // Phân trang
        const int maxPageSize = 100;
        var pageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
        var pageSize = filter.PageSize < 1 ? 1 : (filter.PageSize > maxPageSize ? maxPageSize : filter.PageSize);
        var skip = (pageIndex - 1) * pageSize;

        var totalItems = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedAt)
                               .Skip(skip)
                               .Take(pageSize)
                               .ToListAsync();

        var result = new { Total = totalItems, Data = items };
        response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, result.ToJson(), StorageData.ApplicationJson);
        return response;
    }

    public async Task<ResponseModel> CreateOrder(List<Models.OrderItem>? items)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (items == null || items.Count == 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var trans = await db.Database.BeginTransactionAsync();
            try
            {
                var newOrder = new Models.Order
                {
                    CreatedAt = DateTime.Now,
                    Status = 0, // Mới tạo, đang chờ khách check bill và thanh toán
                    OrderItems = new List<Models.OrderItem>()
                };

                decimal subTotal = 0;

                foreach (var item in items)
                {
                    // Lấy product để lấy giá thật và trừ kho
                    var product = await db.Set<Models.Product>()
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product == null) throw new Exception($"Sản phẩm ID {item.ProductId} không tồn tại");

                    if (product.StockCount < item.Quantity)
                        throw new Exception($"Sản phẩm {product.Name} không đủ hàng (Còn: {product.StockCount})");

                    // Trừ kho ngay để "giữ hàng" cho đơn này
                    product.StockCount -= item.Quantity;

                    var realItem = new Models.OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.SalePrice // Giá server chốt
                    };

                    subTotal += (product.SalePrice ?? 0) * (item.Quantity ?? 0);
                    newOrder.OrderItems.Add(realItem);
                }

                newOrder.SubTotal = subTotal;
                newOrder.FinalTotal = subTotal; // Tạm thời bằng subTotal, client sẽ update voucher sau

                await db.Set<Models.Order>().AddAsync(newOrder);
                await db.SaveChangesAsync();
                await trans.CommitAsync();

                // Trả về đơn hàng có đầy đủ OrderId và UnitPrice thật để Client hiển thị
                response.MakeCustomResponse<byte, char, byte>(201, StorageData.Http11Protocol, newOrder.ToJson(), StorageData.ApplicationJson);
            }
            catch (Exception ex)
            {
                try
                {
                    // Kiểm tra an toàn trước khi Rollback
                    if (trans != null) await trans.RollbackAsync();
                }
                catch { }

                response.MakeCustomResponse<byte, char, byte>(400, StorageData.Http11Protocol, ex.Message, StorageData.TextPlainCharset);
            }
            return response;
        });
    }

    // Hàm này để Client gọi khi khách quẹt thẻ/trả tiền xong hoặc áp Voucher
    public async Task<ResponseModel> UpdateOrder(Models.Order? updateData)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (updateData == null || updateData.OrderId == 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var trans = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Dùng AsNoTracking để lấy data cũ mà không "xí phần" object trong RAM
                // Giúp tránh lỗi "another instance with the same key value is already being tracked"
                var currentOrder = await db.Set<Models.Order>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == updateData.OrderId);

                if (currentOrder == null) throw new Exception("Không tìm thấy đơn hàng");
                if (currentOrder.Status == 2) throw new Exception("Đơn hàng đã hủy, không thể cập nhật");

                // 2. Xử lý logic Hủy đơn (Hoàn kho)
                if (updateData.Status == 2)
                {
                    var items = await db.Set<Models.OrderItem>()
                                        .AsNoTracking()
                                        .Where(x => x.OrderId == updateData.OrderId).ToListAsync();
                    foreach (var item in items)
                    {
                        var p = await db.Set<Models.Product>().FindAsync(item.ProductId);
                        if (p != null) p.StockCount += item.Quantity;
                    }

                    // Gán trực tiếp vào updateData để db.UpdateNoNull thực thi
                    updateData.Status = 2;
                    db.UpdateNoNull(updateData);
                    await db.SaveChangesAsync();
                    await trans.CommitAsync();

                    response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Canceled"u8, StorageData.TextPlainCharset);
                    return response;
                }

                // 3. Xử lý Voucher và tính lại tiền
                decimal discount = 0;
                if (!string.IsNullOrEmpty(updateData.VoucherCode))
                {
                    var voucher = await db.Set<Models.DiscountVoucher>()
                        .FirstOrDefaultAsync(v => v.VoucherCode == updateData.VoucherCode);

                    if (voucher == null || !voucher.ExpiryDate.HasValue || voucher.ExpiryDate.Value < DateTime.Now)
                        throw new Exception("Mã giảm giá không tồn tại hoặc đã hết hạn");

                    if (voucher.DiscountType == 1) // Giảm tiền mặt
                        discount = voucher.DiscountValue ?? 0;
                    else if (voucher.DiscountType == 2) // Giảm %
                        discount = (currentOrder.SubTotal ?? 0) * ((voucher.DiscountValue ?? 0) / 100);

                    // Gán kết quả tính toán vào updateData
                    updateData.DiscountAmount = discount;
                }

                // Tính FinalTotal dựa trên SubTotal gốc (từ DB) và Discount mới
                decimal final = (currentOrder.SubTotal ?? 0) - discount;
                updateData.FinalTotal = Math.Max(0, final);
                updateData.SubTotal = currentOrder.SubTotal; // Lấy lại giá trị cũ từ DB gán vào object trả về

                // 4. Cập nhật dữ liệu (db.UpdateNoNull sẽ gán State = Modified cho updateData)
                db.UpdateNoNull(updateData);

                await db.SaveChangesAsync();
                await trans.CommitAsync();

                // Trả về kết quả sau khi đã tính toán
                response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, updateData.ToJson(), StorageData.ApplicationJson);
            }
            catch (Exception)
            {
                try { if (trans != null) await trans.RollbackAsync(); } catch { }

                const string msg = "Unable to update order.";
                response.MakeCustomResponse<byte, char, byte>(400, StorageData.Http11Protocol, msg, StorageData.TextPlainCharset);
            }
            return response;
        });
    }


    public async Task<ResponseModel> DeleteOrder(Models.Order? order)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (order == null || order.OrderId == 0)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        using var db = Lucifer.GetModelT<DbContext>();
        var strategy = db.Database.CreateExecutionStrategy();

        var orderId = order.OrderId;

        return await strategy.ExecuteAsync(async () =>
        {
            using var trans = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Lấy thông tin đơn hàng và các item để hoàn kho
                var order = await db.Set<Models.Order>()
                                    .Include(o => o.OrderItems)
                                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Not Found"u8, StorageData.TextPlainCharset);
                    return response;
                }

                // 2. Nếu đơn hàng chưa bị hủy (nghĩa là đang giữ hàng), thì xóa đơn phải hoàn kho
                if (order.Status != 2) // Giả sử 2 là trạng thái Đã Hủy
                {
                    foreach (var item in order.OrderItems)
                    {
                        var p = await db.Set<Models.Product>().FindAsync(item.ProductId);
                        if (p != null) p.StockCount += item.Quantity;
                    }
                }

                // 3. Xóa đơn hàng (EF sẽ tự xóa OrderItems nếu bạn cấu hình Cascade Delete)
                db.Set<Models.Order>().Remove(order);
                await db.SaveChangesAsync();
                await trans.CommitAsync();

                response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Deleted"u8, StorageData.TextPlainCharset);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                response.MakeCustomResponse<byte, char, byte>(500, StorageData.Http11Protocol, ex.Message, StorageData.TextPlainCharset);
            }
            return response;
        });
    }
}

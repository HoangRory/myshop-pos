namespace MyShop.Client.Models
{
    /// <summary>
    /// Snapshot của OrdersViewModel dùng để lưu trạng thái tạm thời
    /// </summary>
    public class OrdersViewModelSnapshot
    {
        public int? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SelectedStatus { get; set; }
        public Order? Detail { get; set; }
        public List<OrderItemSnapshot>? OrderItems { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PaymentNote { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Snapshot của OrderItem để lưu lại các sản phẩm đã thêm
    /// </summary>
    public class OrderItemSnapshot
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }
}

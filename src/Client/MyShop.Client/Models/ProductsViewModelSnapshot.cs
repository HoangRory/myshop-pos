namespace MyShop.Client.Models
{
    /// <summary>
    /// Snapshot của ProductsViewModel dùng để lưu trạng thái tạm thời
    /// </summary>
    public class ProductsViewModelSnapshot
    {
        public int? UserId { get; set; }
        public string? NewCategoryName { get; set; }
        public string? SearchKeyword { get; set; }
        public Product? EditingProduct { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? SelectedProductId { get; set; }
    }
}

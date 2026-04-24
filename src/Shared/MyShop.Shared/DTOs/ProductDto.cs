namespace MyShop.Shared.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal ImportPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int StockCount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
    }
}

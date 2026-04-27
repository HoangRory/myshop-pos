namespace MyShop.Client.Models
{
    public class ProductEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public int StockCount { get; set; }
        // Add more fields as needed for editing
    }
}

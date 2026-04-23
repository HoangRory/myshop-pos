namespace Server.Models;

public class ProductFilter
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Phân trang & Sắp xếp
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Id"; // Id, Price, Name, Stock
    public bool IsAscending { get; set; } = true;
}
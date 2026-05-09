namespace MyShop.Client.Models.Report;

public class ProductReport
{
    public string ProductName { get; set; } = null!;
    public List<TimeSeriesPoint> Series { get; set; } = new();
}

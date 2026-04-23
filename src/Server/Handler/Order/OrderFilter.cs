namespace Server.Handler.Order;

public class OrderFilter
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Status { get; set; } // 0: Pending, 1: Paid, 2: Canceled    
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

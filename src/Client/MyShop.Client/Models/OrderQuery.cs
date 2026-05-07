namespace MyShop.Client.Models
{
    /// <summary>
    /// Order filter/query object for API requests.
    /// Mirrors backend OrderFilter class.
    /// </summary>
    public class OrderQuery
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Status { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

namespace MyShop.Client.ViewModels.ProductViewModel
{
    public class ProductPagingViewModel : BaseViewModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : (int)System.Math.Ceiling((double)TotalCount / PageSize);
    }
}

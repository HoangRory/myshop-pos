namespace MyShop.Client.ViewModels.ProductViewModel
{
    public class ProductFilterViewModel : BaseViewModel
    {
        public string? SearchKeyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SelectedProductType { get; set; }
    }
}

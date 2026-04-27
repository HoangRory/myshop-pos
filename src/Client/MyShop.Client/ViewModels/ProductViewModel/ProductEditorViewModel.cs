using MyShop.Client.Models;

namespace MyShop.Client.ViewModels.ProductViewModel
{
    public class ProductEditorViewModel : BaseViewModel
    {
        public ProductEditModel EditModel { get; set; } = new ProductEditModel();
    }
}

using System.Collections.ObjectModel;
using MyShop.Client.Models;

namespace MyShop.Client.ViewModels.ProductViewModel
{
    public class ProductsListViewModel : BaseViewModel
    {
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public Product? SelectedProduct { get; set; }
    }
}

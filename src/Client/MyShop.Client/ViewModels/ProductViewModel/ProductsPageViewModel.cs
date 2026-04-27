using MyShop.Client.Models;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.ProductViewModel
{
    public class ProductsPageViewModel : BaseViewModel
    {
        public ProductsListViewModel ListViewModel { get; }
        public ProductEditorViewModel EditorViewModel { get; }
        public ProductFilterViewModel FilterViewModel { get; }
        public ProductPagingViewModel PagingViewModel { get; }

        public ProductsPageViewModel(ProductsListViewModel list, ProductEditorViewModel editor, ProductFilterViewModel filter, ProductPagingViewModel paging)
        {
            ListViewModel = list;
            EditorViewModel = editor;
            FilterViewModel = filter;
            PagingViewModel = paging;
        }
    }
}

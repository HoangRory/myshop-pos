using System.ComponentModel;

namespace MyShop.Client.Models
{
    public class Product : INotifyPropertyChanged
    {
        private int _productId;
        public int ProductId { get => _productId; set { if (_productId != value) { _productId = value; OnPropertyChanged(nameof(ProductId)); } } }

        private string _sku = string.Empty;
        public string Sku { get => _sku; set { if (_sku != value) { _sku = value; OnPropertyChanged(nameof(Sku)); } } }

        private string _name = string.Empty;
        public string Name { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } } }

        private decimal _importPrice;
        public decimal ImportPrice { get => _importPrice; set { if (_importPrice != value) { _importPrice = value; OnPropertyChanged(nameof(ImportPrice)); } } }

        private decimal _salePrice;
        public decimal SalePrice { get => _salePrice; set { if (_salePrice != value) { _salePrice = value; OnPropertyChanged(nameof(SalePrice)); } } }

        private int _stockCount;
        public int StockCount { get => _stockCount; set { if (_stockCount != value) { _stockCount = value; OnPropertyChanged(nameof(StockCount)); } } }

        private string _description = string.Empty;
        public string Description { get => _description; set { if (_description != value) { _description = value; OnPropertyChanged(nameof(Description)); } } }

        private int? _categoryId;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int? CategoryId { get => _categoryId; set { if (_categoryId != value) { _categoryId = value; OnPropertyChanged(nameof(CategoryId)); } } }
        public string? CategoryName { get; set; } // Thêm property để binding tên danh mục

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}

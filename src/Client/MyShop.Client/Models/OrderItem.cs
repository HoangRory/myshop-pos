using System.ComponentModel;

namespace MyShop.Client.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set
            {
                if (_productName != value)
                {
                    _productName = value;
                    OnPropertyChanged(nameof(ProductName));
                }
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        private object? _selectedProduct;
        public object? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(nameof(SelectedProduct));

                    if (value is Product product)
                    {
                        ProductName = product.Name;
                        Price = product.SalePrice;
                        IsEditing = false;
                    }
                }
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public decimal Total => Quantity * Price;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return ProductName;
        }
    }
}

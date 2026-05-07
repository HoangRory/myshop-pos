using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyShop.Client.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        private int _orderItemId;
        public int OrderItemId
        {
            get => _orderItemId;
            set
            {
                if (_orderItemId != value)
                {
                    _orderItemId = value;
                    OnPropertyChanged(nameof(OrderItemId));
                }
            }
        }

        private int _orderId;
        public int OrderId
        {
            get => _orderId;
            set
            {
                if (_orderId != value)
                {
                    _orderId = value;
                    OnPropertyChanged(nameof(OrderId));
                }
            }
        }

        private int _productId;
        public int ProductId
        {
            get => _productId;
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }

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

        /// <summary>
        /// UI-only property: Used for product selection in ComboBox
        /// Not serialized to JSON
        /// </summary>
        [JsonIgnore]
        private object? _selectedProduct;
        [JsonIgnore]
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
                        ProductId = product.ProductId;
                        ProductName = product.Name;
                        Price = product.SalePrice;
                        IsEditing = false;
                    }
                }
            }
        }

        /// <summary>
        /// UI-only property: Indicates if row is in editing mode
        /// Not serialized to JSON
        /// </summary>
        [JsonIgnore]
        private bool _isEditing;
        [JsonIgnore]
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

        /// <summary>
        /// UI-only property: Search text for product combobox
        /// Not serialized to JSON
        /// </summary>
        [JsonIgnore]
        private string _searchText = string.Empty;
        [JsonIgnore]
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

        /// <summary>
        /// UI-only computed property: Total price for this item
        /// Not serialized to JSON
        /// </summary>
        [JsonIgnore]
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

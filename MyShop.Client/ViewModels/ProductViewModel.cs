using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MyShop.Client.Models;
using MyShop.Client.Services;

namespace MyShop.Client.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly ProductApiClient _apiClient;
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ProductViewModel()
        {
            _apiClient = new ProductApiClient();
        }

        public async Task LoadProductsAsync()
        {
            IsLoading = true;
            try
            {
                var products = await _apiClient.GetProductsAsync();
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

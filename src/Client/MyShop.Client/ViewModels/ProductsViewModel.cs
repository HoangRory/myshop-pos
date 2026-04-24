using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using MyShop.Client.Mappers;
using MyShop.Shared.DTOs;
using MyShop.Shared.Requests;
using MyShop.Shared.Responses;
using MyShop.Client.Helpers;
using CommunityToolkit.Mvvm.Input;
namespace MyShop.Client.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IProductApiClient _productApiClient;

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    FillFormFromSelected();
                    UpdateCommandStates();
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set {
                if (SetProperty(ref _name, value))
                {
                    ErrorMessage = string.Empty;
                    UpdateCommandStates();
                }
            }
        }

        private string _sku;
        public string SKU
        {
            get => _sku;
            set {
                if (SetProperty(ref _sku, value))
                {
                    ErrorMessage = string.Empty;
                    UpdateCommandStates();
                }
            }
        }

        private decimal _salePrice;
        public decimal SalePrice
        {
            get => _salePrice;
            set {
                if (SetProperty(ref _salePrice, value))
                {
                    ErrorMessage = string.Empty;
                    UpdateCommandStates();
                }
            }
        }

        private int _stockCount;
        public int StockCount
        {
            get => _stockCount;
            set {
                if (SetProperty(ref _stockCount, value))
                {
                    ErrorMessage = string.Empty;
                    UpdateCommandStates();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { if (SetProperty(ref _isLoading, value)) UpdateCommandStates(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public AsyncRelayCommand LoadProductsCommand { get; }
        public AsyncRelayCommand AddProductCommand { get; }
        public AsyncRelayCommand UpdateProductCommand { get; }
        public AsyncRelayCommand DeleteProductCommand { get; }
        public AsyncRelayCommand ClearFormCommand { get; }

        public ProductsViewModel(IProductApiClient productApiClient)
        {
            _productApiClient = productApiClient;
            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, CanExecuteLoadProducts);
            AddProductCommand = new AsyncRelayCommand(AddProductAsync, CanExecuteAddProduct);
            UpdateProductCommand = new AsyncRelayCommand(UpdateProductAsync, CanExecuteUpdateOrDeleteProduct);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, CanExecuteUpdateOrDeleteProduct);
            ClearFormCommand = new AsyncRelayCommand(_ => { ClearForm(); return Task.CompletedTask; }, CanExecuteClearForm);
        }

        private void FillFormFromSelected()
        {
            if (SelectedProduct != null)
            {
                Name = SelectedProduct.Name;
                SKU = SelectedProduct.SKU;
                SalePrice = SelectedProduct.SalePrice;
                StockCount = SelectedProduct.StockCount;
            }
            else
            {
                Name = string.Empty;
                SKU = string.Empty;
                SalePrice = 0;
                StockCount = 0;
            }
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            Name = string.Empty;
            SKU = string.Empty;
            SalePrice = 0;
            StockCount = 0;
            ErrorMessage = string.Empty;
        }

        private async Task LoadProductsAsync()
        {
            if (IsLoading || IsLoaded) return;
            IsLoading = true;
            try
            {
                await LoadProductsCoreAsync();
                IsLoaded = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProductsCoreAsync()
        {
            try
            {
                var response = await _productApiClient.GetAllAsync();
                Products.Clear();
                if (response.Success && response.Data != null)
                {
                    var models = ProductMapper.ToModelList(response.Data);
                    foreach (var p in models)
                        Products.Add(p);
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Failed to load products.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
        

        private async Task AddProductAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                if (!ValidateInput())
                {
                    ErrorMessage = "Vui lòng nhập đầy đủ và hợp lệ thông tin sản phẩm.";
                    return;
                }
                var model = new Product { Name = Name, SKU = SKU, SalePrice = SalePrice, StockCount = StockCount };
                var request = ProductMapper.ToCreateRequest(model);
                var response = await _productApiClient.CreateAsync(request);
                if (response.Success)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    await LoadProductsCoreAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Thêm sản phẩm thất bại.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateProductAsync()
        {
            if (IsLoading || SelectedProduct == null) return;
            IsLoading = true;
            try
            {
                // Không modify SelectedProduct trực tiếp, tạo model mới để update
                var model = new Product
                {
                    Id = SelectedProduct.Id,
                    Name = Name,
                    SKU = SKU,
                    SalePrice = SalePrice,
                    StockCount = StockCount
                };
                var request = ProductMapper.ToUpdateRequest(model);
                var response = await _productApiClient.UpdateAsync(request);
                if (response.Success)
                {
                    ErrorMessage = string.Empty;
                    await LoadProductsCoreAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Cập nhật sản phẩm thất bại.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteProductAsync()
        {
            if (IsLoading || SelectedProduct == null) return;
            IsLoading = true;
            try
            {
                var response = await _productApiClient.DeleteAsync(SelectedProduct.Id);
                if (response.Success)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    await LoadProductsCoreAsync();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Xóa sản phẩm thất bại.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (string.IsNullOrWhiteSpace(SKU)) return false;
            if (SalePrice <= 0) return false;
            if (StockCount < 0) return false;
            return true;
        }

        private bool CanExecuteLoadProducts() => !IsLoading;
        private bool CanExecuteAddProduct() => !IsLoading && ValidateInput();
        private bool CanExecuteUpdateOrDeleteProduct() => !IsLoading && SelectedProduct != null;
        private bool CanExecuteClearForm() => !IsLoading;

        private void UpdateCommandStates()
        {
            LoadProductsCommand.NotifyCanExecuteChanged();
            AddProductCommand.NotifyCanExecuteChanged();
            UpdateProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();
            ClearFormCommand.NotifyCanExecuteChanged();
        }
    }
}

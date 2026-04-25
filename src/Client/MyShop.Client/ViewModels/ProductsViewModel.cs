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

        // Full in-memory list loaded from API
        private readonly ObservableCollection<Product> _allProducts = new ObservableCollection<Product>();

        // Products currently displayed after filtering/paging/sorting
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        // --- Paging / Filtering / Sorting state ---
        private int _pageIndex = 1;
        public int PageIndex { get => _pageIndex; set { if (SetProperty(ref _pageIndex, value)) ApplyFilteringAndPaging(); } }

        private int _pageSize = 10;
        public int PageSize { get => _pageSize; set { if (SetProperty(ref _pageSize, value)) ApplyFilteringAndPaging(); } }

        private int _totalCount;
        public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }

        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set { if (SetProperty(ref _searchKeyword, value)) { PageIndex = 1; ApplyFilteringAndPaging(); } } }

        private decimal? _minPrice;
        public decimal? MinPrice { get => _minPrice; set { if (SetProperty(ref _minPrice, value)) { PageIndex = 1; ApplyFilteringAndPaging(); } } }

        private decimal? _maxPrice;
        public decimal? MaxPrice { get => _maxPrice; set { if (SetProperty(ref _maxPrice, value)) { PageIndex = 1; ApplyFilteringAndPaging(); } } }

        private string _selectedSortField = "Name";
        public string SelectedSortField { get => _selectedSortField; set { if (SetProperty(ref _selectedSortField, value)) ApplyFilteringAndPaging(); } }

        private bool _sortDescending = false;
        public bool SortDescending { get => _sortDescending; set { if (SetProperty(ref _sortDescending, value)) ApplyFilteringAndPaging(); } }

        private string _selectedProductType;
        public string SelectedProductType { get => _selectedProductType; set { if (SetProperty(ref _selectedProductType, value)) { PageIndex = 1; ApplyFilteringAndPaging(); } } }

        public ObservableCollection<string> ProductTypes { get; } = new ObservableCollection<string>();

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
        public System.Windows.Input.ICommand NextPageCommand { get; }
        public System.Windows.Input.ICommand PrevPageCommand { get; }
        public System.Windows.Input.ICommand ApplyFiltersCommand { get; }
        public System.Windows.Input.ICommand ImportExcelCommand { get; }
        public System.Windows.Input.ICommand ImportAccessCommand { get; }
        public System.Windows.Input.ICommand AddProductTypeCommand { get; }

        public ProductsViewModel(IProductApiClient productApiClient)
        {
            _productApiClient = productApiClient;
            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, CanExecuteLoadProducts);
            AddProductCommand = new AsyncRelayCommand(AddProductAsync, CanExecuteAddProduct);
            UpdateProductCommand = new AsyncRelayCommand(UpdateProductAsync, CanExecuteUpdateOrDeleteProduct);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, CanExecuteUpdateOrDeleteProduct);
            ClearFormCommand = new AsyncRelayCommand(_ => { ClearForm(); return Task.CompletedTask; }, CanExecuteClearForm);

            NextPageCommand = new MyShop.Client.Helpers.RelayCommand(_ => { if (PageIndex < TotalPages) PageIndex++; }, _ => PageIndex < TotalPages);
            PrevPageCommand = new MyShop.Client.Helpers.RelayCommand(_ => { if (PageIndex > 1) PageIndex--; }, _ => PageIndex > 1);
            ApplyFiltersCommand = new MyShop.Client.Helpers.RelayCommand(_ => { PageIndex = 1; ApplyFilteringAndPaging(); });
            ImportExcelCommand = new MyShop.Client.Helpers.RelayCommand(_ => ImportFromExcel());
            ImportAccessCommand = new MyShop.Client.Helpers.RelayCommand(_ => ImportFromAccess());
            AddProductTypeCommand = new MyShop.Client.Helpers.RelayCommand(_ => AddProductType());
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
                _allProducts.Clear();
                Products.Clear();
                if (response.Success && response.Data != null)
                {
                    var models = ProductMapper.ToModelList(response.Data);
                    foreach (var p in models)
                        _allProducts.Add(p);

                    // populate product types from data (if any). Use CategoryId if available
                    ProductTypes.Clear();
                    ProductTypes.Add("(All)");
                    foreach (var t in System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Select(_allProducts, p => (p.CategoryId.HasValue ? p.CategoryId.Value.ToString() : "Unknown"))))
                        ProductTypes.Add(t);

                    ErrorMessage = string.Empty;
                    // apply filters / paging
                    ApplyFilteringAndPaging();
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

        private void ApplyFilteringAndPaging()
        {
            try
            {
                var query = System.Linq.Enumerable.AsEnumerable(_allProducts);

                // Filter by product type
                if (!string.IsNullOrWhiteSpace(SelectedProductType) && SelectedProductType != "(All)")
                    query = System.Linq.Enumerable.Where(query, p => (p.CategoryId.HasValue ? p.CategoryId.Value.ToString() : "Unknown") == SelectedProductType);

                // Search keyword
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    var kw = SearchKeyword.Trim();
                    query = System.Linq.Enumerable.Where(query, p => (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0) || (!string.IsNullOrEmpty(p.SKU) && p.SKU.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                // Price filter
                if (MinPrice.HasValue)
                    query = System.Linq.Enumerable.Where(query, p => p.SalePrice >= MinPrice.Value);
                if (MaxPrice.HasValue)
                    query = System.Linq.Enumerable.Where(query, p => p.SalePrice <= MaxPrice.Value);

                // Sorting
                query = SelectedSortField switch
                {
                    "Name" => SortDescending ? System.Linq.Enumerable.OrderByDescending(query, p => p.Name) : System.Linq.Enumerable.OrderBy(query, p => p.Name),
                    "SalePrice" => SortDescending ? System.Linq.Enumerable.OrderByDescending(query, p => p.SalePrice) : System.Linq.Enumerable.OrderBy(query, p => p.SalePrice),
                    "StockCount" => SortDescending ? System.Linq.Enumerable.OrderByDescending(query, p => p.StockCount) : System.Linq.Enumerable.OrderBy(query, p => p.StockCount),
                    _ => query
                };

                var list = System.Linq.Enumerable.ToList(query);
                TotalCount = list.Count;

                // Adjust PageIndex if out of bounds
                if (PageIndex < 1) PageIndex = 1;
                if (PageIndex > TotalPages) PageIndex = TotalPages;

                var skip = (PageIndex - 1) * PageSize;
                var paged = System.Linq.Enumerable.Skip(list, skip).Take(PageSize);

                Products.Clear();
                foreach (var p in paged)
                    Products.Add(p);

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Filtering error: {ex.Message}";
            }
        }

        private void ImportFromExcel()
        {
            // Minimal placeholder implementation - shows message via ErrorMessage.
            // Real implementation should open file dialog and parse Excel (ExcelDataReader/EPPlus).
            ErrorMessage = "Import from Excel is not implemented in this build.";
        }

        private void ImportFromAccess()
        {
            ErrorMessage = "Import from Access is not implemented in this build.";
        }

        private void AddProductType()
        {
            // Very small helper to add a product type - in real app show dialog to input new type
            var newType = "NewType" + (ProductTypes.Count + 1);
            if (!ProductTypes.Contains(newType)) ProductTypes.Add(newType);
            ErrorMessage = $"Added product type '{newType}' (demo).";
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

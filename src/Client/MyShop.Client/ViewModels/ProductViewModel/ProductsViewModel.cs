using System.Collections.ObjectModel;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
namespace MyShop.Client.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IProductService _productService;

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }


        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<string> ProductTypes { get; } = new ObservableCollection<string>();

        // Query/filter state
        private int _pageIndex = 1;
        public int PageIndex { get => _pageIndex; set { if (SetProperty(ref _pageIndex, value)) LoadProductsCommand.Execute(null); } }

        private int _pageSize = 10;
        public int PageSize { get => _pageSize; set { if (SetProperty(ref _pageSize, value)) LoadProductsCommand.Execute(null); } }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                if (SetProperty(ref _totalCount, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set { if (SetProperty(ref _searchKeyword, value)) PageIndex = 1; } }

        private decimal? _minPrice;
        public decimal? MinPrice { get => _minPrice; set { if (SetProperty(ref _minPrice, value)) PageIndex = 1; } }

        private decimal? _maxPrice;
        public decimal? MaxPrice { get => _maxPrice; set { if (SetProperty(ref _maxPrice, value)) PageIndex = 1; } }

        private string _selectedSortField = "Name";
        public string SelectedSortField { get => _selectedSortField; set { if (SetProperty(ref _selectedSortField, value)) LoadProductsCommand.Execute(null); } }

        private bool _sortDescending = false;
        public bool SortDescending { get => _sortDescending; set { if (SetProperty(ref _sortDescending, value)) LoadProductsCommand.Execute(null); } }

        private string _selectedProductType;
        public string SelectedProductType { get => _selectedProductType; set { if (SetProperty(ref _selectedProductType, value)) PageIndex = 1; } }

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
        public AsyncRelayCommand SaveProductCommand { get; }
        public AsyncRelayCommand UpdateProductCommand { get; }
        public AsyncRelayCommand DeleteProductCommand { get; }
        public AsyncRelayCommand ClearFormCommand { get; }
        public System.Windows.Input.ICommand NextPageCommand { get; }
        public System.Windows.Input.ICommand PrevPageCommand { get; }
        public System.Windows.Input.ICommand ApplyFiltersCommand { get; }
        public System.Windows.Input.ICommand ImportExcelCommand { get; }
        public System.Windows.Input.ICommand ImportAccessCommand { get; }
        public System.Windows.Input.ICommand AddProductTypeCommand { get; }

        public ProductsViewModel(IProductService productService)
        {
            _productService = productService;
            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, CanExecuteLoadProducts);
            AddProductCommand = new AsyncRelayCommand(OpenAddFormAsync, CanExecuteOpenAddForm);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, CanExecuteSaveProduct);
            UpdateProductCommand = new AsyncRelayCommand(UpdateProductAsync, CanExecuteUpdateOrDeleteProduct);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, CanExecuteUpdateOrDeleteProduct);
            ClearFormCommand = new AsyncRelayCommand(_ => { ClearForm(); return Task.CompletedTask; }, CanExecuteClearForm);

            NextPageCommand = new MyShop.Client.Helpers.RelayCommand(_ => { if (PageIndex < TotalPages) PageIndex++; }, _ => PageIndex < TotalPages);
            PrevPageCommand = new MyShop.Client.Helpers.RelayCommand(_ => { if (PageIndex > 1) PageIndex--; }, _ => PageIndex > 1);
            ApplyFiltersCommand = new MyShop.Client.Helpers.RelayCommand(_ => { PageIndex = 1; LoadProductsCommand.Execute(null); });
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

        private Task OpenAddFormAsync()
        {
            // Prepare an empty product for the form. FillFormFromSelected will populate fields.
            SelectedProduct = new Product();
            ErrorMessage = string.Empty;
            return Task.CompletedTask;
        }

        private async Task SaveProductAsync()
        {
            if (SelectedProduct == null) return;
            if (!ValidateInput())
            {
                ErrorMessage = "Vui lòng nhập đầy đủ và hợp lệ thông tin sản phẩm.";
                return;
            }

            // If the selected product has an Id (non-zero) treat as update, otherwise create
            if (SelectedProduct.Id == 0)
            {
                await CreateProductAsync();
            }
            else
            {
                await UpdateProductAsync();
            }
        }


        private async Task LoadProductsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                int? categoryId = null;
                if (!string.IsNullOrEmpty(SelectedProductType) && SelectedProductType != "(All)" && int.TryParse(SelectedProductType, out var catId))
                {
                    categoryId = catId;
                }
                var query = new ProductQuery
                {
                    PageIndex = PageIndex,
                    PageSize = PageSize,
                    Keyword = SearchKeyword,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    SortBy = SelectedSortField,
                    IsAscending = !SortDescending,
                    CategoryId = categoryId
                };
                var (products, totalCount) = await _productService.GetProductsAsync(query);
                Products.Clear();
                foreach (var p in products)
                    Products.Add(p);
                TotalCount = totalCount;
                // Populate product types
                ProductTypes.Clear();
                ProductTypes.Add("(All)");
                foreach (var t in products.Select(p => p.CategoryId?.ToString() ?? "Unknown").Distinct())
                    if (!ProductTypes.Contains(t)) ProductTypes.Add(t);

                ErrorMessage = string.Empty;
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


        // Removed ApplyFilteringAndPaging and all LINQ logic. All data logic is now in ProductService.

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
        

        private async Task CreateProductAsync()
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
                var model = new ProductEditModel { Name = Name, SKU = SKU, SalePrice = SalePrice, StockCount = StockCount };
                var result = await _productService.CreateAsync(model);
                if (result != null)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    await LoadProductsAsync();
                }
                else
                {
                    ErrorMessage = "Thêm sản phẩm thất bại.";
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
                var model = new ProductEditModel
                {
                    Id = SelectedProduct.Id,
                    Name = Name,
                    SKU = SKU,
                    SalePrice = SalePrice,
                    StockCount = StockCount
                };
                var result = await _productService.UpdateAsync(model);
                if (result != null)
                {
                    ErrorMessage = string.Empty;
                    await LoadProductsAsync();
                }
                else
                {
                    ErrorMessage = "Cập nhật sản phẩm thất bại.";
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
                var result = await _productService.DeleteAsync(SelectedProduct.Id);
                if (result)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    await LoadProductsAsync();
                }
                else
                {
                    ErrorMessage = "Xóa sản phẩm thất bại.";
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
        private bool CanExecuteOpenAddForm() => !IsLoading;
        private bool CanExecuteSaveProduct() => !IsLoading && SelectedProduct != null && ValidateInput();
        private bool CanExecuteUpdateOrDeleteProduct() => !IsLoading && SelectedProduct != null;
        private bool CanExecuteClearForm() => !IsLoading;

        private void UpdateCommandStates()
        {
            LoadProductsCommand.NotifyCanExecuteChanged();
            AddProductCommand.NotifyCanExecuteChanged();
            SaveProductCommand.NotifyCanExecuteChanged();
            UpdateProductCommand.NotifyCanExecuteChanged();
            DeleteProductCommand.NotifyCanExecuteChanged();
            ClearFormCommand.NotifyCanExecuteChanged();
        }
    }
}

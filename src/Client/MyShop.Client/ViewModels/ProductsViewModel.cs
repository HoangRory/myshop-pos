using System.Collections.ObjectModel;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using MyShop.Client.Services;
using CommunityToolkit.Mvvm.Input;
namespace MyShop.Client.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }


        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    PageIndex = 1;
                }
            }
        }
        // --- Editing Flow State ---
        private Product? _editingProduct;
        public Product? EditingProduct
        {
            get => _editingProduct;
            set
            {
                if (SetProperty(ref _editingProduct, value))
                {
                    // Khi gán object mới, thông báo toàn bộ property thay đổi để binding UI cập nhật
                    OnPropertyChanged(nameof(EditingProduct));
                    // Nếu có các command phụ thuộc, cập nhật trạng thái
                    SaveProductCommand.NotifyCanExecuteChanged();
                    DeleteProductCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private Product? _snapshotProduct;

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

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    // Không mở edit mode trực tiếp khi chọn dòng, chỉ lưu selection
                    // Nếu muốn bắt đầu edit, gọi hàm riêng
                    OpenEditMode(value);
                    UpdateCommandStates();
                }
            }
        }

        public void OpenEditMode(Product? product)
        {
            if (product == null)
            {
                EditingProduct = null;
                _snapshotProduct = null;
                return;
            }
            // Clone thủ công từng property, không dùng MemberwiseClone
            var clone = new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                ImportPrice = product.ImportPrice,
                SalePrice = product.SalePrice,
                StockCount = product.StockCount,
                Description = product.Description
            };
            _snapshotProduct = new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                ImportPrice = product.ImportPrice,
                SalePrice = product.SalePrice,
                StockCount = product.StockCount,
                Description = product.Description
            };
            EditingProduct = clone;
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
        public AsyncRelayCommand DeleteProductCommand { get; }
        public AsyncRelayCommand ClearFormCommand { get; }
        public System.Windows.Input.ICommand NextPageCommand { get; }
        public System.Windows.Input.ICommand PrevPageCommand { get; }
        public System.Windows.Input.ICommand ApplyFiltersCommand { get; }
        public System.Windows.Input.ICommand ImportExcelCommand { get; }
        public System.Windows.Input.ICommand ImportAccessCommand { get; }
        public System.Windows.Input.ICommand AddProductTypeCommand { get; }

        public ProductsViewModel(IProductService productService, IDialogService dialogService, ICategoryService categoryService)
        {
            _productService = productService;
            _dialogService = dialogService;
            _categoryService = categoryService;
            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, CanExecuteLoadProducts);
            AddProductCommand = new AsyncRelayCommand(OpenAddFormAsync, CanExecuteOpenAddForm);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, CanExecuteSaveProduct);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, CanExecuteUpdateOrDeleteProduct);
            ClearFormCommand = new AsyncRelayCommand(_ => { ClearForm(); return Task.CompletedTask; }, CanExecuteClearForm);

            NextPageCommand = new MyShop.Client.Helpers.RelayCommand(_ => { if (PageIndex < TotalPages) PageIndex++; }, _ => PageIndex < TotalPages);
            PrevPageCommand = new MyShop.Client.Helpers.RelayCommand(_ => { if (PageIndex > 1) PageIndex--; }, _ => PageIndex > 1);
            ApplyFiltersCommand = new MyShop.Client.Helpers.RelayCommand(_ => { PageIndex = 1; LoadProductsCommand.Execute(null); });
            ImportExcelCommand = new MyShop.Client.Helpers.RelayCommand(_ => ImportFromExcel());
            ImportAccessCommand = new MyShop.Client.Helpers.RelayCommand(_ => ImportFromAccess());

            // Load categories when initializing
            _ = LoadCategoriesAsync();
        }



        private void ClearForm()
        {
            ClearEdit();
        }

        private Task OpenAddFormAsync()
        {
            SelectedProduct = null;
            OpenEditMode(new Product());
            ErrorMessage = string.Empty;
            return Task.CompletedTask;
        }

        private async Task SaveProductAsync()
        {
            if (EditingProduct == null) return;
            if (!ValidateInput())
            {
                //ErrorMessage = "Vui lòng nhập đầy đủ và hợp lệ thông tin sản phẩm.";
                return;
            }

            IsLoading = true;
            try
            {
                bool result;
                bool isCreate = EditingProduct.ProductId == 0;
                if (isCreate)
                {
                    result = await _productService.CreateAsync(EditingProduct);
                }
                else
                {
                    result = await _productService.UpdateAsync(EditingProduct);
                }

                if (result)
                {
                    _dialogService.Success(
                        isCreate ? "Thành công" : "Cập nhật thành công",
                        isCreate ? "Thêm sản phẩm thành công." : "Cập nhật sản phẩm thành công.");
                    ClearEdit();
                }
                else
                {
                    ErrorMessage = isCreate ? "Thêm sản phẩm thất bại." : "Cập nhật sản phẩm thất bại.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                await LoadProductsAsync();
            }
        }

        private void CancelEdit()
        {
            if (_snapshotProduct == null)
            {
                EditingProduct = null;
                return;
            }
            // Khôi phục lại EditingProduct từ snapshot (clone thủ công)
            EditingProduct = new Product
            {
                ProductId = _snapshotProduct.ProductId,
                Name = _snapshotProduct.Name,
                Sku = _snapshotProduct.Sku,
                CategoryId = _snapshotProduct.CategoryId,
                ImportPrice = _snapshotProduct.ImportPrice,
                SalePrice = _snapshotProduct.SalePrice,
                StockCount = _snapshotProduct.StockCount,
                Description = _snapshotProduct.Description
            };
        }

        private void ClearEdit()
        {
            SelectedProduct = null;
            EditingProduct = null;
            _snapshotProduct = null;
            ErrorMessage = string.Empty;
        }

        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();

            var allCategories = await _categoryService.GetAllAsync();

            Categories.Add(new Category
            {
                CategoryId = 0,
                Name = "(Tất cả)"
            });

            foreach (var c in allCategories)
                Categories.Add(c);

            if (SelectedCategory == null)
                SelectedCategory = Categories.FirstOrDefault();
        }

        private async Task LoadProductsAsync()
        {
            if (IsLoading) return;

            if (EditingProduct != null)
            {
                var confirm = _dialogService.Confirm(
                    "Xác nhận",
                    "Bạn đang chỉnh sửa sản phẩm. Nếu tiếp tục sẽ mất thay đổi. Bạn có muốn tiếp tục không?");

                if (!confirm) return;

                ClearEdit(); // hủy edit trước khi load
            }

            IsLoading = true;
            try
            {
                var query = new ProductQuery
                {
                    PageIndex = PageIndex,
                    PageSize = PageSize,
                    Keyword = SearchKeyword,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    SortBy = SelectedSortField,
                    IsAscending = !SortDescending,
                    CategoryId = SelectedCategory?.CategoryId == 0
                    ? null
                    : SelectedCategory?.CategoryId
                };
                var (products, totalCount) = await _productService.SearchAsync(query);
                Products.Clear();
                foreach (var p in products)
                {
                    p.CategoryName = Categories
                        .FirstOrDefault(c => c.CategoryId == p.CategoryId)
                        ?.Name ?? "Không có";
                    Products.Add(p);
                }
                TotalCount = totalCount;

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



        private async Task DeleteProductAsync()
        {
            if (IsLoading || SelectedProduct == null) return;
            IsLoading = true;
            try
            {
                var result = await _productService.DeleteAsync(SelectedProduct.ProductId);
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
            if (EditingProduct == null)
            {
                ErrorMessage = "Không có dữ liệu sản phẩm để kiểm tra.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditingProduct.Name))
            {
                ErrorMessage = "Tên sản phẩm không được để trống.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditingProduct.Sku))
            {
                ErrorMessage = "SKU không được để trống.";
                return false;
            }

            if (EditingProduct.ImportPrice < 0)
            {
                ErrorMessage = "Giá nhập phải lớn hơn 0.";
                return false;
            }

            if (EditingProduct.SalePrice < 0)
            {
                ErrorMessage = "Giá bán phải lớn hơn 0.";
                return false;
            }

            if (EditingProduct.StockCount < 0)
            {
                ErrorMessage = "Số lượng tồn kho không được âm.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private bool CanExecuteLoadProducts() => !IsLoading;
        private bool CanExecuteOpenAddForm() => !IsLoading;
        private bool CanExecuteSaveProduct() => !IsLoading && EditingProduct != null;
        private bool CanExecuteUpdateOrDeleteProduct() => !IsLoading && SelectedProduct != null;
        private bool CanExecuteClearForm() => !IsLoading;

        private void UpdateCommandStates()
        {
            LoadProductsCommand.NotifyCanExecuteChanged();
            AddProductCommand.NotifyCanExecuteChanged();
            SaveProductCommand.NotifyCanExecuteChanged();
            // UpdateProductCommand removed
            DeleteProductCommand.NotifyCanExecuteChanged();
            ClearFormCommand.NotifyCanExecuteChanged();
        }
    }
}

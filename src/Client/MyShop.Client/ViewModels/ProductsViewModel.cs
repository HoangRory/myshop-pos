using CommunityToolkit.Mvvm.Input;
using LuciferCore.Attributes;
using Microsoft.Win32;
using MyShop.Client.Models;
using MyShop.Client.Services;
using MyShop.Client.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Products")]
    public class ProductsViewModel : BaseViewModel
    {
        public string PageTitle { get; } = "Sản phẩm";
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;
        private readonly IImageService _imageService;
        private readonly ITemporaryDataService _tempDataService;
        private readonly IAuthService _authService;

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> FilterCategories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Category> EditCategories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Category> DeletableCategories { get; } = new ObservableCollection<Category>();

        private bool _isAddCategoryPanelVisible;
        public bool IsAddCategoryPanelVisible
        {
            get => _isAddCategoryPanelVisible;
            set
            {
                if (SetProperty(ref _isAddCategoryPanelVisible, value))
                {
                    OnPropertyChanged(nameof(AddCategoryButtonText));
                }
            }
        }

        private bool _isDeleteCategoryPanelVisible;
        public bool IsDeleteCategoryPanelVisible
        {
            get => _isDeleteCategoryPanelVisible;
            set
            {
                if (SetProperty(ref _isDeleteCategoryPanelVisible, value))
                {
                    OnPropertyChanged(nameof(DeleteCategoryButtonText));
                }
            }
        }

        public string AddCategoryButtonText => IsAddCategoryPanelVisible ? "Hủy" : "Thêm danh mục";
        public string DeleteCategoryButtonText => IsDeleteCategoryPanelVisible ? "Hủy" : "Xóa danh mục";

        // Tracks whether there are unapplied filter/sort/search changes
        private bool _filtersDirty;

        public bool IsApplyEnabled => !IsLoading && _filtersDirty;

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value)
                    return;

                if (SetProperty(ref _selectedCategory, value))
                {
                    // Do not auto-apply filter changes. Require user to press Apply.
                    _filtersDirty = true;
                    ApplyFiltersCommand?.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(IsApplyEnabled));
                }
            }
        }
        // --- Editing Flow State ---
        // ===============================
        // Tracks if EditingProduct differs from _snapshotProduct
        // ===============================

        private bool IsEditDirty()
        {
            if (EditingProduct == null || _snapshotProduct == null)
                return false;

            // Compare key properties to determine if changes were made
            return EditingProduct.Name != _snapshotProduct.Name ||
                   EditingProduct.Sku != _snapshotProduct.Sku ||
                   EditingProduct.CategoryId != _snapshotProduct.CategoryId ||
                   EditingProduct.ImportPrice != _snapshotProduct.ImportPrice ||
                   EditingProduct.SalePrice != _snapshotProduct.SalePrice ||
                   EditingProduct.StockCount != _snapshotProduct.StockCount ||
                   EditingProduct.Description != _snapshotProduct.Description ||
                   EditingProduct.Images != _snapshotProduct.Images;
        }

        private bool TryLeaveEditMode()
        {
            if (EditingProduct == null)
                return true;

            // Only confirm if there are actual unsaved changes
            if (!IsEditDirty())
            {
                ClearEdit();
                return true;
            }

            var confirm = _dialogService.Confirm(
                "Xác nhận",
                "Bạn đang chỉnh sửa sản phẩm. Nếu tiếp tục, các thay đổi chưa lưu sẽ bị mất. Bạn có muốn tiếp tục không?"
            );

            if (!confirm)
                return false;

            ClearEdit();
            return true;
        }



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

        private static Product CloneProduct(Product product)
        {
            return new Product
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                ImportPrice = product.ImportPrice,
                SalePrice = product.SalePrice,
                StockCount = product.StockCount,
                Description = product.Description,
                Images = product.Images,
                CategoryName = product.CategoryName
            };
        }

        private void RestoreSelectedProduct(Product? product)
        {
            if (ReferenceEquals(_selectedProduct, product))
                return;

            _selectedProduct = product;
            OnPropertyChanged(nameof(SelectedProduct));
            UpdateCommandStates();
        }

        // Query/filter state
        private int _pageIndex = 1;
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                if (SetProperty(ref _pageIndex, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                    NotifyPagingCommands();
                    LoadProductsCommand.Execute(null);
                }
            }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                    NotifyPagingCommands();
                    PageIndex = 1;
                    LoadProductsCommand.Execute(null);
                }
            }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                if (SetProperty(ref _totalCount, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                    NotifyPagingCommands();
                }
            }
        }

        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);

        private string _searchKeyword;
        public string SearchKeyword { get => _searchKeyword; set { if (SetProperty(ref _searchKeyword, value)) { _filtersDirty = true; ApplyFiltersCommand?.NotifyCanExecuteChanged(); OnPropertyChanged(nameof(IsApplyEnabled)); } } }

        private decimal? _minPrice;
        public decimal? MinPrice { get => _minPrice; set { if (SetProperty(ref _minPrice, value)) { _filtersDirty = true; ApplyFiltersCommand?.NotifyCanExecuteChanged(); OnPropertyChanged(nameof(IsApplyEnabled)); } } }

        private decimal? _maxPrice;
        public decimal? MaxPrice { get => _maxPrice; set { if (SetProperty(ref _maxPrice, value)) { _filtersDirty = true; ApplyFiltersCommand?.NotifyCanExecuteChanged(); OnPropertyChanged(nameof(IsApplyEnabled)); } } }

        private string _selectedSortField = "Name";
        public string SelectedSortField { get => _selectedSortField; set { if (SetProperty(ref _selectedSortField, value)) { _filtersDirty = true; ApplyFiltersCommand?.NotifyCanExecuteChanged(); OnPropertyChanged(nameof(IsApplyEnabled)); } } }

        private bool _sortDescending = false;
        public bool SortDescending { get => _sortDescending; set { if (SetProperty(ref _sortDescending, value)) { _filtersDirty = true; ApplyFiltersCommand?.NotifyCanExecuteChanged(); OnPropertyChanged(nameof(IsApplyEnabled)); } } }

        // Sort Options for UI
        private ObservableCollection<string> _sortOptions = new ObservableCollection<string> { "Tên", "Giá", "Tồn kho" };
        public ObservableCollection<string> SortOptions
        {
            get => _sortOptions;
            private set => SetProperty(ref _sortOptions, value);
        }

        private string _sortBy = "Tên";
        public string SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                {
                    _filtersDirty = true;
                    ApplyFiltersCommand?.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(IsApplyEnabled));
                }
            }
        }

        private bool _isAscending = true;
        public bool IsAscending
        {
            get => _isAscending;
            set
            {
                if (SetProperty(ref _isAscending, value))
                {
                    _filtersDirty = true;
                    ApplyFiltersCommand?.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(IsApplyEnabled));
                }
            }
        }

        private string _selectedProductType;
        public string SelectedProductType { get => _selectedProductType; set { if (SetProperty(ref _selectedProductType, value)) { _filtersDirty = true; ApplyFiltersCommand?.NotifyCanExecuteChanged(); OnPropertyChanged(nameof(IsApplyEnabled)); } } }

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct == value)
                    return;

                // If currently editing another product, confirm before switching selection
                if (EditingProduct != null && value != null && EditingProduct.ProductId != value.ProductId)
                {
                    if (!TryLeaveEditMode())
                        return; // user cancelled, do not change selection

                    // TryLeaveEditMode cleared the edit state; proceed with selection
                    if (SetProperty(ref _selectedProduct, value))
                    {
                        OpenEditMode(value);
                        UpdateCommandStates();
                    }
                }
                else
                {
                    if (SetProperty(ref _selectedProduct, value))
                    {
                        if (value == null)
                        {
                            // Selection can be cleared during reload/filtering; keep the edit session alive.
                            UpdateCommandStates();
                            return;
                        }

                        // Không mở edit mode trực tiếp khi chọn dòng, chỉ lưu selection
                        // Nếu muốn bắt đầu edit, gọi hàm riêng
                        OpenEditMode(value);
                        UpdateCommandStates();
                    }
                }
            }
        }

        private async Task LoadPreviewImagesAsync()
        {
            PreviewImages.Clear();

            foreach (var fileName in SelectedImages)
            {
                try
                {
                    var bytes = await _imageService.DownloadImageAsync(fileName);

                    using var ms = new MemoryStream(bytes);

                    var bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    PreviewImages.Add(bitmap);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Load image failed: {ex.Message}");
                }
            }

            OnPropertyChanged(nameof(PreviewImages));
        }

        private async Task LoadProductThumbnailAsync(Product product)
        {
            if (product == null) return;

            try
            {
                var first = _imageService.ParseImageNames(product.Images).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(first)) return;

                var bytes = await _image_service_safe_download(first);
                if (bytes == null || bytes.Length == 0) return;

                using var ms = new MemoryStream(bytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    product.Thumbnail = bitmap;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thumbnail load failed: {ex.Message}");
            }
        }

        // Helper to call image service and swallow exceptions, returning bytes or null
        private async Task<byte[]?> _image_service_safe_download(string fileName)
        {
            try
            {
                return await _image_service_download_wrapper(fileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Download image failed: {ex.Message}");
                return null;
            }
        }

        private Task<byte[]> _image_service_download_wrapper(string fileName)
        {
            return _imageService.DownloadImageAsync(fileName);
        }
        // Images for editing/viewing
        private ObservableCollection<string> _selectedImages = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedImages
        {
            get => _selectedImages;
            set => SetProperty(ref _selectedImages, value);
        }
        public ObservableCollection<BitmapImage> PreviewImages { get; set; } = new();

        private bool _isUploadingImage;
        public bool IsUploadingImage
        {
            get => _isUploadingImage;
            set => SetProperty(ref _isUploadingImage, value);
        }

        public void OpenEditMode(Product? product)
        {
            if (product == null)
            {
                EditingProduct = null;
                _snapshotProduct = null;
                SelectedImages.Clear();
                return;
            }
            var clone = CloneProduct(product);
            _snapshotProduct = CloneProduct(product);
            EditingProduct = clone;
            // Populate selected images collection from Images string
            SelectedImages = new ObservableCollection<string>(
                _imageService.ParseImageNames(clone.Images)
            );

            _ = LoadPreviewImagesAsync();
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
        public AsyncRelayCommand CancelEditCommand { get; }
        public AsyncRelayCommand UploadImageCommand { get; }
        public RelayCommand<string> RemoveImageCommand { get; }
        public RelayCommand NextPageCommand { get; }
        public RelayCommand PrevPageCommand { get; }
        public RelayCommand ApplyFiltersCommand { get; }
        public AsyncRelayCommand ImportExcelCommand { get; }
        public ICommand ImportAccessCommand { get; }
        public ICommand AddProductTypeCommand { get; }
        public AsyncRelayCommand AddCategoryCommand { get; }
        public AsyncRelayCommand DeleteCategoryCommand { get; }
        public RelayCommand ToggleAddCategoryPanelCommand { get; }
        public RelayCommand ToggleDeleteCategoryPanelCommand { get; }

        public ProductsViewModel(IProductService productService, IDialogService dialogService, ICategoryService categoryService, IImageService imageService, ITemporaryDataService tempDataService, IAuthService authService)
        {
            _productService = productService;
            _dialogService = dialogService;
            _categoryService = categoryService;
            _imageService = imageService;
            _tempDataService = tempDataService;
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            _authService.OnRecoveryRequested += OnRecoveryRequested;

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, CanExecuteLoadProducts);
            AddProductCommand = new AsyncRelayCommand(OpenAddFormAsync, CanExecuteOpenAddForm);
            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync, CanExecuteAddCategory);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync, CanExecuteDeleteCategory);
            ToggleAddCategoryPanelCommand = new RelayCommand(ToggleAddCategoryPanel, CanExecuteToggleCategoryPanel);
            ToggleDeleteCategoryPanelCommand = new RelayCommand(ToggleDeleteCategoryPanel, CanExecuteToggleCategoryPanel);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, CanExecuteSaveProduct);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, CanExecuteUpdateOrDeleteProduct);
            ClearFormCommand = new AsyncRelayCommand(() => { ClearForm(); return Task.CompletedTask; }, CanExecuteClearForm);
            UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
            RemoveImageCommand = new RelayCommand<string>(RemoveImage);
            CancelEditCommand = new AsyncRelayCommand(() => { TryLeaveEditMode(); return Task.CompletedTask; }, CanExecuteClearForm);

            // ===============================
            // STEP 4: Sửa NextPageCommand
            // ===============================
            NextPageCommand = new RelayCommand(
                () =>
                {
                    if (PageIndex < TotalPages)
                    {
                        PageIndex++;
                    }
                },
                () => PageIndex < TotalPages
            );

            // ===============================
            // STEP 5: Sửa PrevPageCommand
            // ===============================
            PrevPageCommand = new RelayCommand(
                () =>
                {
                    if (PageIndex > 1)
                    {
                        PageIndex--;
                    }
                },
                () => PageIndex > 1
            );

            // ===============================
            // STEP 6: Sửa ApplyFiltersCommand
            // ===============================
            ApplyFiltersCommand = new RelayCommand(
                () =>
                {
                    PageIndex = 1;
                    LoadProductsCommand.Execute(null);
                },
                () => !IsLoading && _filtersDirty
            );
            ImportExcelCommand = new AsyncRelayCommand(ImportFromExcelAsync, CanExecuteImportExcel);
            ImportAccessCommand = new RelayCommand(ImportFromAccess);

            // Subscribe to settings changes to update PageSize
            AppSettingsService.ItemsPerPageChanged += (s, e) =>
            {
                var config = AppConfig.Load();
                PageSize = config.ItemsPerPage;
            };

            // Load PageSize from AppConfig after commands are initialized
            PageSize = AppConfig.Load().ItemsPerPage;

            // Cố gắng khôi phục dữ liệu tạm thời
            _ = RecoverDataAsync();

            // Đăng ký cho auto-save
            RegisterAutoSave(_tempDataService, "Products");
        }

        private void RemoveImage(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return;

            if (SelectedImages.Contains(imageName))
            {
                SelectedImages.Remove(imageName);

                if (EditingProduct != null)
                {
                    EditingProduct.Images = string.Join(";", SelectedImages);
                }
                _ = LoadPreviewImagesAsync();
            }
        }

        private async void OnRecoveryRequested(List<string> modules)
        {
            if (!modules.Contains("Products"))
                return;

            await RecoverDataAsync();
            await LoadProductsAsync();
        }

        private bool CanExecuteUploadImage()
        {
            return !IsLoading && !IsUploadingImage && EditingProduct != null;
        }

        private async Task UploadImageAsync()
        {
            if (EditingProduct == null)
                return;

            var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp",
                Multiselect = true
            };

            if (dialog.ShowDialog() != true)
                return;

            IsUploadingImage = true;

            try
            {
                foreach (var filePath in dialog.FileNames)
                {
                    try
                    {
                        var bytes = await File.ReadAllBytesAsync(filePath);

                        var uploadedFileName = await _imageService.UploadImageAsync(
                            bytes,
                            Path.GetFileName(filePath)
                        );

                        if (!SelectedImages.Contains(uploadedFileName))
                        {
                            SelectedImages.Add(uploadedFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Upload failed: {ex.Message}");
                    }
                }

                // Sync lại vào Product.Images
                EditingProduct.Images = string.Join(";", SelectedImages);
                await LoadPreviewImagesAsync();

                OnPropertyChanged(nameof(SelectedImages));
                OnPropertyChanged(nameof(EditingProduct));
            }
            finally
            {
                IsUploadingImage = false;
            }
        }
        private bool CanExecuteToggleCategoryPanel() => !IsLoading;

        private void NotifyPagingCommands()
        {
            NextPageCommand?.NotifyCanExecuteChanged();
            PrevPageCommand?.NotifyCanExecuteChanged();
        }

        private void ToggleAddCategoryPanel()
        {
            var next = !IsAddCategoryPanelVisible;
            IsAddCategoryPanelVisible = next;
            if (next)
            {
                IsDeleteCategoryPanelVisible = false;
            }
        }

        private void ToggleDeleteCategoryPanel()
        {
            var next = !IsDeleteCategoryPanelVisible;
            IsDeleteCategoryPanelVisible = next;
            if (next)
            {
                IsAddCategoryPanelVisible = false;
                if (SelectedCategoryToDelete == null)
                {
                    SelectedCategoryToDelete = DeletableCategories.FirstOrDefault();
                }
            }
        }

        private void ClearForm()
        {
            // Confirm if user is currently editing; TryLeaveEditMode will clear on confirm
            TryLeaveEditMode();
        }

        private async Task OpenAddFormAsync()
        {
            await EnsureCategoriesLoadedAsync();
            SelectedProduct = null;
            OpenEditMode(new Product());
            ErrorMessage = string.Empty;
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
                if (EditingProduct.CategoryId == -1)
                {
                    EditingProduct.CategoryId = null; // Gán null nếu chọn "(Không có)"
                }
                // Ensure Images property is synchronized from SelectedImages
                EditingProduct.Images = string.Join(";", SelectedImages);
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
                    // Update snapshot after successful save so IsEditDirty returns false
                    if (EditingProduct != null && _snapshotProduct != null)
                    {
                        _snapshotProduct.Name = EditingProduct.Name;
                        _snapshotProduct.Sku = EditingProduct.Sku;
                        _snapshotProduct.CategoryId = EditingProduct.CategoryId;
                        _snapshotProduct.ImportPrice = EditingProduct.ImportPrice;
                        _snapshotProduct.SalePrice = EditingProduct.SalePrice;
                        _snapshotProduct.StockCount = EditingProduct.StockCount;
                        _snapshotProduct.Description = EditingProduct.Description;
                        _snapshotProduct.Images = EditingProduct.Images;
                    }
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

        private void ClearEdit()
        {
            SelectedProduct = null;
            EditingProduct = null;
            _snapshotProduct = null;
            ErrorMessage = string.Empty;
        }

        private async Task LoadCategoriesAsync()
        {
            FilterCategories.Clear();
            EditCategories.Clear();
            DeletableCategories.Clear();

            var allCategories = await _categoryService.GetAllAsync();

            FilterCategories.Add(new Category
            {
                CategoryId = 0,
                Name = "(Tất cả)"
            });

            EditCategories.Add(new Category
            {
                CategoryId = -1,
                Name = "(Không có)"
            });

            foreach (var c in allCategories)
            {
                EditCategories.Add(c);
                FilterCategories.Add(c);
                DeletableCategories.Add(c);
            }

            if (SelectedCategory == null)
                SelectedCategory = FilterCategories.FirstOrDefault();

            if (SelectedCategoryToDelete != null)
            {
                SelectedCategoryToDelete = DeletableCategories
                    .FirstOrDefault(c => c.CategoryId == SelectedCategoryToDelete.CategoryId);
            }
        }

        private string _newCategoryName;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set
            {
                if (SetProperty(ref _newCategoryName, value))
                {
                    AddCategoryCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool CanExecuteAddCategory() => !IsLoading && !string.IsNullOrWhiteSpace(NewCategoryName);

        private async Task AddCategoryAsync()
        {
            if (IsLoading) return;

            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                ErrorMessage = "Tên danh mục không được để trống.";
                return;
            }

            IsLoading = true;
            try
            {
                var model = new Category { Name = NewCategoryName };
                var result = await _categoryService.CreateAsync(model);
                if (result)
                {
                    _dialogService.Success("Thành công", "Thêm danh mục thành công.");
                    await LoadCategoriesAsync();
                    // Select the created category if possible
                    var created = EditCategories.FirstOrDefault(c => c.Name == NewCategoryName);
                    if (created != null && EditingProduct != null)
                    {
                        EditingProduct.CategoryId = created.CategoryId;
                        OnPropertyChanged(nameof(EditingProduct));
                    }
                    SelectedCategoryToDelete = created;
                    NewCategoryName = string.Empty;
                }
                else
                {
                    ErrorMessage = "Thêm danh mục thất bại.";
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

        private Category? _selectedCategoryToDelete;
        public Category? SelectedCategoryToDelete
        {
            get => _selectedCategoryToDelete;
            set
            {
                if (SetProperty(ref _selectedCategoryToDelete, value))
                {
                    DeleteCategoryCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool CanExecuteDeleteCategory() => !IsLoading && SelectedCategoryToDelete != null;

        private async Task DeleteCategoryAsync()
        {
            if (IsLoading || SelectedCategoryToDelete == null) return;

            var category = SelectedCategoryToDelete;
            bool shouldReloadProducts = false;
            var confirm = _dialogService.Confirm(
                "Xác nhận",
                $"Bạn có chắc muốn xóa danh mục '{category.Name}' không?");

            if (!confirm)
                return;

            IsLoading = true;
            try
            {
                var result = await _categoryService.DeleteAsync(category.CategoryId);
                if (result)
                {
                    _dialogService.Success("Thành công", "Xóa danh mục thành công.");

                    if (EditingProduct != null && EditingProduct.CategoryId == category.CategoryId)
                    {
                        EditingProduct.CategoryId = -1;
                        OnPropertyChanged(nameof(EditingProduct));
                    }

                    SelectedCategoryToDelete = null;
                    await LoadCategoriesAsync();
                    shouldReloadProducts = true;
                }
                else
                {
                    ErrorMessage = "Xóa danh mục thất bại.";
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

            if (shouldReloadProducts)
            {
                await LoadProductsAsync();
            }
        }

        private async Task EnsureCategoriesLoadedAsync()
        {
            if (FilterCategories.Any() && EditCategories.Any())
                return;

            await LoadCategoriesAsync();
        }

        private async Task LoadProductsAsync()
        {
            if (IsLoading) return;

            await EnsureCategoriesLoadedAsync();

            IsLoading = true;
            try
            {
                var activeEditProductId = EditingProduct?.ProductId;
                var activeSelectedProductId = SelectedProduct?.ProductId;

                // Map UI SortBy display names to API field names
                string sortByField = _sortBy switch
                {
                    "Tên" => "Name",
                    "Giá" => "Price",
                    "Tồn kho" => "Stock",
                    _ => "Name"
                };

                var query = new ProductQuery
                {
                    PageIndex = PageIndex,
                    PageSize = PageSize,
                    Keyword = SearchKeyword,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    SortBy = sortByField,
                    IsAscending = IsAscending,
                    CategoryId = SelectedCategory?.CategoryId == 0
                    ? null
                    : SelectedCategory?.CategoryId
                };
                var (products, totalCount) = await _productService.SearchAsync(query);

                Products.Clear();
                foreach (var p in products)
                {
                    var product = CloneProduct(p);
                    product.CategoryName = FilterCategories
                        .FirstOrDefault(c => c.CategoryId == p.CategoryId)
                        ?.Name ?? "Không có";

                    Products.Add(product);

                    // Start loading the thumbnail asynchronously (do not block the UI)
                    _ = LoadProductThumbnailAsync(product);
                }

                var restoreProductId = activeEditProductId ?? activeSelectedProductId;
                if (restoreProductId.HasValue)
                {
                    var restoredSelection = Products.FirstOrDefault(x => x.ProductId == restoreProductId.Value);
                    RestoreSelectedProduct(restoredSelection);
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

        private bool CanExecuteImportExcel() => !IsLoading;

        private async Task ImportFromExcelAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Chọn file Excel để nhập sản phẩm",
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    CheckFileExists = true,
                    Multiselect = false
                };

                bool? result = dialog.ShowDialog();
                if (result != true)
                {
                    IsLoading = false;
                    return;
                }

                string filePath = dialog.FileName;
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    ErrorMessage = "Vui lòng chọn file Excel hợp lệ.";
                    IsLoading = false;
                    return;
                }

                bool importResult = await _productService.ImportExcelAsync(filePath);

                if (importResult)
                {
                    _dialogService.Success("Thành công", "Nhập sản phẩm từ Excel thành công.");
                    await LoadProductsAsync();
                }
                else
                {
                    ErrorMessage = "Nhập sản phẩm từ Excel thất bại.";
                    // Nếu muốn popup lỗi, có thể dùng Success với tiêu đề "Lỗi"
                    //_dialogService.Success("Lỗi", ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi nhập Excel: {ex.Message}";
                // Nếu muốn popup lỗi, có thể dùng Success với tiêu đề "Lỗi"
                //_dialogService.Success("Lỗi", ErrorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ImportFromAccess()
        {
            ErrorMessage = "Import from Access is not implemented in this build.";
        }



        private async Task DeleteProductAsync()
        {
            var productToDelete = SelectedProduct ?? EditingProduct;
            if (IsLoading || productToDelete == null) return;
            IsLoading = true;
            // Confirm deletion
            var confirm = _dialogService.Confirm(
                        "Xác nhận",
                        $"Bạn có chắc muốn xóa sản phẩm '{productToDelete.Name}' không?");
            if (!confirm)
            {
                IsLoading = false;
                return;
            }
            try
            {
                var result = await _productService.DeleteAsync(productToDelete.ProductId);
                if (result)
                {
                    _dialogService.Success("Thành công", "Xóa sản phẩm thành công.");
                    ErrorMessage = string.Empty;
                    ClearForm();
                    IsLoading = false;
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
        private bool CanExecuteUpdateOrDeleteProduct() => !IsLoading && EditingProduct != null;
        private bool CanExecuteClearForm() => !IsLoading;

        private void UpdateCommandStates()
        {
            LoadProductsCommand.NotifyCanExecuteChanged();
            AddProductCommand.NotifyCanExecuteChanged();
            SaveProductCommand.NotifyCanExecuteChanged();
            // UpdateProductCommand removed
            DeleteProductCommand.NotifyCanExecuteChanged();
            ClearFormCommand.NotifyCanExecuteChanged();
            AddCategoryCommand?.NotifyCanExecuteChanged();
            DeleteCategoryCommand?.NotifyCanExecuteChanged();
            ToggleAddCategoryPanelCommand?.NotifyCanExecuteChanged();
            ToggleDeleteCategoryPanelCommand?.NotifyCanExecuteChanged();
            ApplyFiltersCommand?.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsApplyEnabled));
            NotifyPagingCommands();
        }

        // =====================================
        // Auto-Save and Recovery Methods
        // =====================================

        /// <summary>
        /// Khôi phục dữ liệu tạm thời nếu ứng dụng bị tắt bất ngờ
        /// </summary>
        private async Task RecoverDataAsync()
        {
            var snapshot = await TryRecoverDataAsync<ProductsViewModelSnapshot>(_tempDataService, "Products");
            if (snapshot == null)
                return;

            try
            {
                // Validate UserId matches current user
                if (int.TryParse(_authService.AccountId, out int currentUserId))
                {
                    if (snapshot.UserId != currentUserId)
                    {
                        // Data belongs to different user, delete it
                        _tempDataService.DeleteTemporaryData("Products");
                        return;
                    }
                }
                else
                {
                    // Cannot determine current user ID, skip recovery
                    return;
                }

                if(MyShop.Client.Helpers.RecoveryHelper.ShowRecoveryDialog(new List<string> { "Products" }) != true)
                {
                    // User declined recovery, delete temp data
                    _tempDataService.DeleteTemporaryData("Products");
                    return;
                }

                // Khôi phục category được chọn
                if (snapshot.SelectedCategory != null)
                {
                    var category = FilterCategories.FirstOrDefault(c => c.CategoryId == snapshot.SelectedCategory.CategoryId);
                    if (category != null)
                    {
                        SelectedCategory = category;
                    }
                }

                // Khôi phục sản phẩm đang chỉnh sửa
                if (snapshot.EditingProduct != null)
                {
                    EditingProduct = snapshot.EditingProduct;
                }

                // Khôi phục Search keyword 
                if (snapshot.SearchKeyword != null)
                    SearchKeyword = snapshot.SearchKeyword;
                // Khôi phục pagination
                PageIndex = snapshot.CurrentPage;
                PageSize = snapshot.PageSize;

                CommitRecovery(_tempDataService, "Products");
                System.Diagnostics.Debug.WriteLine("Dữ liệu sản phẩm đã được khôi phục");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi khôi phục dữ liệu: {ex.Message}");
                // Nếu lỗi, xóa dữ liệu để tránh lặp lại
                _tempDataService.DeleteTemporaryData("Products");
            }
        }

        /// <summary>
        /// Cung cấp dữ liệu để lưu tạm thời
        /// </summary>
        protected override object? GetAutoSaveData()
        {
            // Get current user ID
            int? currentUserId = null;
            if (int.TryParse(_authService.AccountId, out int userId))
            {
                currentUserId = userId;
            }

            return new ProductsViewModelSnapshot
            {
                UserId = currentUserId,
                SelectedCategory = SelectedCategory,
                SearchKeyword = _searchKeyword,
                EditingProduct = EditingProduct,
                CurrentPage = PageIndex,
                PageSize = PageSize
            };
        }

        public void LoadData()
        {
            if (!IsLoaded)
            {
                if (LoadProductsCommand.CanExecute(null))
                    LoadProductsCommand.Execute(null);
            }
        }
    }
}

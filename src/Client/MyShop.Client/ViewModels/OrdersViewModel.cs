using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using LuciferCore.Attributes;
using MyShop.Client.Services;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Orders")]
    public class OrdersViewModel : BaseViewModel
    {
        public string PageTitle { get; } = "Đơn hàng";
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly ITemporaryDataService _tempDataService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowPaymentDetails =>
            Detail != null &&
            (IsPaymentMode || IsPaidOrder);
        private bool _isPaymentMode;
        public bool IsPaymentMode
        {
            get => _isPaymentMode;
            set
            {
                if (SetProperty(ref _isPaymentMode, value))
                {
                    OnPropertyChanged(nameof(ShowPaymentDetails));
                }
            }
        }
        public bool IsPaidOrder =>
            Detail?.Status == (byte)OrderStatus.Paid;
        public bool IsCancelledOrder =>
            Detail?.Status == (byte)OrderStatus.Cancelled;
        public bool IsFinalizedOrder =>
            IsPaidOrder || IsCancelledOrder;
        public bool IsNewOrder =>
            Detail?.OrderId == -1;

        private string? _selectedStatus;
        public string? SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        private DateTime? _fromDate;
        private string _fromDateText = string.Empty;
        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value?.Date))
                {
                    SyncDateTextFromDate(isFromDate: true);
                }
            }
        }

        public string FromDateText
        {
            get => _fromDateText;
            set => SetDateText(value, isFromDate: true);
        }

        private DateTime? _toDate;
        private string _toDateText = string.Empty;
        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value?.Date))
                {
                    SyncDateTextFromDate(isFromDate: false);
                }
            }
        }

        public string ToDateText
        {
            get => _toDateText;
            set => SetDateText(value, isFromDate: false);
        }

        private Order? _detail;
        public Order? Detail
        {
            get => _detail;
            set
            {
                if (ReferenceEquals(_detail, value))
                {
                    return;
                }

                if (_detail != null)
                {
                    _detail.PropertyChanged -= Detail_PropertyChanged;
                }

                if (SetProperty(ref _detail, value))
                {
                    if (_detail != null)
                    {
                        _detail.PropertyChanged += Detail_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(IsNewOrder));
                    OnPropertyChanged(nameof(ShowPaymentDetails));
                    OnPropertyChanged(nameof(IsPaidOrder));
                    OnPropertyChanged(nameof(IsCancelledOrder));
                    OnPropertyChanged(nameof(IsFinalizedOrder));
                }
            }
        }

        private OrderItem? _pendingNewOrderItem;
        public OrderItem? PendingNewOrderItem
        {
            get => _pendingNewOrderItem;
            set => SetProperty(ref _pendingNewOrderItem, value);
        }

        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (SetProperty(ref _selectedOrder, value) && value != null)
                {
                    // Load order details when selected (fire and forget with proper async handling)
                    _ = LoadOrderDetailAsync(value);
                }
            }
        }

        private int _pageIndex = 1;
        public int PageIndex
        {
            get => _pageIndex;
            set => SetProperty(ref _pageIndex, value);
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        private string _pageInfo = "Page 1";
        public string PageInfo
        {
            get => _pageInfo;
            set => SetProperty(ref _pageInfo, value);
        }

        private bool _canPrevPage;
        public bool CanPrevPage
        {
            get => _canPrevPage;
            set => SetProperty(ref _canPrevPage, value);
        }

        private bool _canNextPage;
        public bool CanNextPage
        {
            get => _canNextPage;
            set => SetProperty(ref _canNextPage, value);
        }

        private bool _isSyncingDateText;
        private const string DisplayDateFormat = "dd/MM/yyyy";
        private static readonly string[] AcceptedDateFormats =
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "yyyy-MM-dd"
        };
        private static readonly CultureInfo DateCulture = new("vi-VN");

        public ObservableCollection<Order> Orders { get; } = new();
        public ObservableCollection<string> StatusOptions { get; } = new();
        public ObservableCollection<Product> AvailableProducts { get; } = new();

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand EnterPaymentModeCommand { get; }
        public ICommand ApplyPaymentCommand { get; }
        public ICommand ApplyVoucherCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public OrdersViewModel(IOrderService orderService, IProductService productService, IDialogService dialogService, ITemporaryDataService tempDataService, IAuthService authService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _tempDataService = tempDataService ?? throw new ArgumentNullException(nameof(tempDataService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _authService.OnRecoveryRequested += OnRecoveryRequested;

            // Initialize status options
            StatusOptions.Add("All");
            StatusOptions.Add("Chờ thanh toán");
            StatusOptions.Add("Đã thanh toán");
            StatusOptions.Add("Đã hủy");

            // Initialize commands with async support
            SearchCommand = new AsyncRelayCommand(_ => OnSearchAsync());
            CreateOrderCommand = new RelayCommand(_ => OnCreateOrder());
            DeleteOrderCommand = new AsyncRelayCommand<Order>(param => OnDeleteOrderAsync(param));
            AddProductCommand = new RelayCommand(_ => OnAddProduct());
            RemoveItemCommand = new RelayCommand(param => OnRemoveItem(param));
            EnterPaymentModeCommand = new RelayCommand(_ => OnEnterPaymentMode());
            ApplyPaymentCommand = new AsyncRelayCommand(_ => OnApplyPaymentAsync(), _ => Detail != null && (Detail.PaymentMethod ?? 0) > 0);
            ApplyVoucherCommand = new AsyncRelayCommand<Order>(param => OnApplyVoucherAsync(param), param => param != null && IsPaymentMode);
            CancelOrderCommand = new AsyncRelayCommand(_ => OnCancelOrderAsync(), _ => Detail != null && Detail.OrderId != -1 && Detail.Status != (byte)OrderStatus.Paid);
            GoBackCommand = new AsyncRelayCommand(_ => OnGoBackAsync());
            PrevPageCommand = new AsyncRelayCommand(_ => OnPrevPageAsync(), _ => CanPrevPage);
            NextPageCommand = new AsyncRelayCommand(_ => OnNextPageAsync(), _ => CanNextPage);

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
            RegisterAutoSave(_tempDataService, "Orders");

            // Load initial data
            InitializeDataAsync();
        }

        private async void OnRecoveryRequested(List<string> modules)
        {
            if (!modules.Contains("Orders"))
                return;

            await RecoverDataAsync();
            await OnSearchAsync();
        }
        private async void InitializeDataAsync()
        {
            IsLoading = true;
            try
            {
                // Load products from service
                var products = await _productService.GetAllAsync();
                foreach (var product in products)
                {
                    AvailableProducts.Add(product);
                }

                // Load all orders
                await OnSearchAsync();

                SelectedStatus = "All";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing data: {ex.Message}");
                // In production, you might want to show a user-friendly message
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnSearchAsync()
        {
            IsLoading = true;
            try
            {
                if (!await RollbackDetailToDraftAsync(false))
                {
                    return;
                }

                PageIndex = 1;

                // Convert SelectedStatus string to byte? status
                byte? statusFilter = null;
                if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "All")
                {
                    statusFilter = SelectedStatus switch
                    {
                        "Chờ thanh toán" => (byte)OrderStatus.Pending,
                        "Đã thanh toán" => (byte)OrderStatus.Paid,
                        "Đã hủy" => (byte)OrderStatus.Cancelled,
                        _ => null
                    };
                }

                // Build query object from filter parameters
                var query = new OrderQuery
                {
                    PageIndex = PageIndex,
                    PageSize = PageSize,
                    FromDate = FromDate,
                    ToDate = ToDate,
                    Status = statusFilter
                };

                // Call service to search orders
                var (orders, total) = await _orderService.SearchAsync(query);

                // Update collection
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                UpdatePaginationInfo(total);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching orders: {ex.Message}");
                // In production, show error message to user
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task<bool> RollbackDetailToDraftAsync(bool forceRollback)
        {
            if (Detail == null)
            {
                return true;
            }
            // Nếu chỉ đang ở payment mode
            // thì KHÔNG autosave/create order mới
            if (IsPaymentMode)
            {
                IsPaymentMode = false;
                return true;
            }

            var shouldRollback = Detail.Status != (byte)OrderStatus.Paid && (Detail.OrderId == -1 || IsPaymentMode);

            if (!forceRollback && !shouldRollback)
            {
                return true;
            }

            var backupOrderItems = CloneOrderItems(Detail.OrderItems);

            // Auto-save current draft instead of deleting
            if (!await AutoSaveCurrentDetailAsync())
            {
                return false;
            }

            RestoreDraftDetail(backupOrderItems);
            return true;
        }

        private static List<OrderItem> CloneOrderItems(IEnumerable<OrderItem> orderItems)
        {
            return orderItems.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                IsEditing = i.IsEditing
            }).ToList();
        }

        private async Task<bool> AutoSaveCurrentDetailAsync()
        {
            if (Detail == null || Detail.OrderItems.Count == 0 || Detail.Status == (byte)OrderStatus.Paid)
            {
                return true;
            }

            try
            {
                // Backup current order items
                var orderItems = Detail.OrderItems.ToList();

                // Delete old order from server if it exists
                if (Detail.OrderId != -1)
                {
                    var deleteSuccess = await _orderService.DeleteAsync(Detail.OrderId);
                    if (!deleteSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to delete old order during auto-save");
                        return false;
                    }
                }

                // Create new order with backed up items
                var createdOrder = await _orderService.CreateAsync(orderItems);
                if (createdOrder != null)
                {
                    return true;
                }

                await OnSearchAsync();

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error auto-saving order: {ex.Message}");
                return false;
            }
        }

        private void RestoreDraftDetail(IEnumerable<OrderItem> backupOrderItems)
        {
            Detail = new Order
            {
                OrderId = -1,
                CreatedAt = DateTime.Now,
                Status = (byte)OrderStatus.Pending,
                PaymentMethod = null,
                OrderItems = new ObservableCollection<OrderItem>(backupOrderItems)
            };

            // Ensure product names match product IDs for the restored draft
            SyncOrderItemProductNames(Detail.OrderItems);

            IsPaymentMode = false;
            SelectedOrder = null;
        }

        private void SetDateText(string? input, bool isFromDate)
        {
            var normalized = string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim();

            if (_isSyncingDateText)
            {
                if (isFromDate)
                {
                    SetProperty(ref _fromDateText, normalized, nameof(FromDateText));
                }
                else
                {
                    SetProperty(ref _toDateText, normalized, nameof(ToDateText));
                }

                return;
            }

            var changed = isFromDate
                ? SetProperty(ref _fromDateText, normalized, nameof(FromDateText))
                : SetProperty(ref _toDateText, normalized, nameof(ToDateText));

            if (!changed)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(normalized))
            {
                if (isFromDate)
                {
                    FromDate = null;
                }
                else
                {
                    ToDate = null;
                }

                return;
            }

            if (TryParseFlexibleDate(normalized, out var parsedDate))
            {
                if (isFromDate)
                {
                    FromDate = parsedDate;
                }
                else
                {
                    ToDate = parsedDate;
                }
            }
        }

        private void SyncDateTextFromDate(bool isFromDate)
        {
            var targetText = isFromDate ? FormatDate(FromDate) : FormatDate(ToDate);
            var currentText = isFromDate ? _fromDateText : _toDateText;

            if (string.Equals(currentText, targetText, StringComparison.Ordinal))
            {
                return;
            }

            _isSyncingDateText = true;
            if (isFromDate)
            {
                _fromDateText = targetText;
                OnPropertyChanged(nameof(FromDateText));
            }
            else
            {
                _toDateText = targetText;
                OnPropertyChanged(nameof(ToDateText));
            }
            _isSyncingDateText = false;
        }

        private static bool TryParseFlexibleDate(string input, out DateTime date)
        {
            if (DateTime.TryParseExact(input, AcceptedDateFormats, DateCulture, DateTimeStyles.None, out var parsed))
            {
                date = parsed.Date;
                return true;
            }

            if (DateTime.TryParse(input, DateCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
            {
                date = parsed.Date;
                return true;
            }

            date = default;
            return false;
        }

        private static string FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString(DisplayDateFormat, DateCulture) : string.Empty;
        }

        private async void OnCreateOrder()
        {
            // Auto-save current draft before creating a new order
            await AutoSaveCurrentDetailAsync();

            Detail = new Order
            {
                OrderId = -1, // Temporary ID for new order
                CreatedAt = DateTime.Now,
                Status = (byte)OrderStatus.Pending,
                PaymentMethod = null,
                OrderItems = new ObservableCollection<OrderItem>()
            };

            IsPaymentMode = false;
            OnPropertyChanged(nameof(IsNewOrder));
        }

        private async Task LoadOrderDetailAsync(Order order)
        {
            IsLoading = true;
            try
            {
                // Get full order details from service
                var fullOrder = await _orderService.GetByIdAsync(order.OrderId);

                if (fullOrder != null)
                {
                    SyncOrderItemProductNames(fullOrder.OrderItems);

                    // Clone the order for safe editing
                    Detail = new Order
                    {
                        OrderId = fullOrder.OrderId,
                        AccountId = fullOrder.AccountId,
                        CreatedAt = fullOrder.CreatedAt,
                        Status = fullOrder.Status,
                        PaymentMethod = fullOrder.PaymentMethod,
                        SubTotal = fullOrder.SubTotal,
                        VoucherCode = fullOrder.VoucherCode,
                        DiscountAmount = fullOrder.DiscountAmount,
                        FinalTotal = fullOrder.FinalTotal,
                        Note = fullOrder.Note,
                        OrderItems = new ObservableCollection<OrderItem>(fullOrder.OrderItems)
                    };

                    IsPaymentMode = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading order detail: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SyncOrderItemProductNames(IEnumerable<OrderItem> orderItems)
        {
            if (orderItems == null)
            {
                return;
            }

            foreach (var item in orderItems)
            {
                if (item == null || item.ProductId == null || !string.IsNullOrWhiteSpace(item.ProductName))
                {
                    continue;
                }

                var product = AvailableProducts.FirstOrDefault(x => x.ProductId == item.ProductId.Value);
                if (product != null)
                {
                    item.ProductName = product.Name;
                }
            }
        }

        private async Task OnDeleteOrderAsync(Order? order)
        {
            if (order == null)
                return;

            IsLoading = true;
            try
            {
                // Parse order ID for API call

                var success = await _orderService.DeleteAsync(order.OrderId);

                if (success)
                {
                    Orders.Remove(order);
                    if (Detail?.OrderId == order.OrderId)
                    {
                        Detail = null;
                    }

                    // Refresh list to sync with server
                    await OnSearchAsync();
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnAddProduct()
        {
            // Allow adding products only when not in payment mode and order is not paid
            if (Detail != null && !IsPaymentMode && !IsFinalizedOrder)
            {
                var newItem = new OrderItem
                {
                    ProductName = string.Empty,
                    Quantity = 1,
                    UnitPrice = 0,
                    IsEditing = true
                };
                Detail.OrderItems.Add(newItem);
                PendingNewOrderItem = newItem;
            }
        }

        private void OnRemoveItem(object? param)
        {
            if (Detail != null && !IsPaymentMode && Detail.Status != (byte)OrderStatus.Paid && param is OrderItem item)
            {
                Detail.OrderItems.Remove(item);
            }
        }

        private async void OnEnterPaymentMode()
        {
            if (Detail == null || Detail.Status == (byte)OrderStatus.Paid)
                return;

            IsLoading = true;

            try
            {
                if (!await EnsureDetailOrderExistsAsync())
                {
                    return;
                }

                // Recalculate subtotal and default final total
                Detail.SubTotal = Detail.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
                Detail.FinalTotal = (Detail.SubTotal ?? 0m) - (Detail.DiscountAmount ?? 0m);

                // Enter payment mode (show payment input fields)
                IsPaymentMode = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error entering payment mode: {ex.Message}");
                _dialogService.Error("Lỗi", "Không thể vào chế độ thanh toán.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnApplyPaymentAsync()
        {
            if (Detail == null || Detail.Status == (byte)OrderStatus.Paid)
                return;

            IsLoading = true;

            try
            {
                if (!await EnsureDetailOrderExistsAsync())
                {
                    return;
                }

                // Calculate totals
                Detail.SubTotal = Detail.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
                var discount = Detail.DiscountAmount ?? 0m;
                Detail.FinalTotal = Detail.SubTotal - discount;

                // Mark as paid
                Detail.Status = (byte)OrderStatus.Paid;

                var updatedOrder = await _orderService.UpdateAsync(Detail);

                if (updatedOrder != null)
                {
                    await OnSearchAsync();
                    ResetPaymentState();
                    Detail = null;
                    SelectedOrder = null;
                    _dialogService.Success("Thành công", "Thanh toán thành công.");
                }
                else
                {
                    _dialogService.Error("Thất bại", "Thanh toán thất bại.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying payment: {ex.Message}");
                _dialogService.Error("Lỗi", "Có lỗi xảy ra khi xử lý thanh toán.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetPaymentState()
        {
            IsPaymentMode = false;
            PendingNewOrderItem = null;
        }

        private async Task OnCancelOrderAsync()
        {
            if (Detail == null || Detail.OrderId == -1)
                return;

            IsLoading = true;

            try
            {
                var orderId = Detail.OrderId;
                var success = await _orderService.CancelAsync(orderId);

                if (success)
                {
                    Detail = null;
                    SelectedOrder = null;
                    await OnSearchAsync();
                    _dialogService.Success("Thành công", "Hủy đơn hàng thành công.");
                }
                else
                {
                    _dialogService.Error("Thất bại", "Hủy đơn hàng thất bại.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling order: {ex.Message}");
                _dialogService.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<bool> EnsureDetailOrderExistsAsync()
        {
            if (Detail == null)
            {
                return false;
            }
            // Build current items list from Detail (aggregate duplicates before sending)
            var orderItems = AggregateOrderItems(Detail.OrderItems);

            // If order already exists on server, check whether items changed
            if (Detail.OrderId != -1)
            {
                try
                {
                    var serverOrder = await _orderService.GetByIdAsync(Detail.OrderId);

                    if (serverOrder != null)
                    {
                        var serverItems = serverOrder.OrderItems.ToList();

                        // If items are identical, nothing to do
                        if (!OrderItemsDiffer(serverItems, orderItems))
                        {
                            return true;
                        }

                        // Items changed: delete old order then create new one
                        var deleteSuccess = await _orderService.DeleteAsync(Detail.OrderId);
                        if (!deleteSuccess)
                        {
                            _dialogService.Error("Thất bại", "Không thể xóa đơn cũ để tái tạo.");
                            return false;
                        }

                        var recreated = await _orderService.CreateAsync(orderItems);
                        if (recreated == null)
                        {
                            _dialogService.Error("Thất bại", "Cập nhật đơn hàng thất bại.");
                            return false;
                        }

                        // Ensure product names are synced to product IDs
                        SyncOrderItemProductNames(recreated.OrderItems);

                        Detail = recreated;
                        _selectedOrder = recreated;
                        OnPropertyChanged(nameof(SelectedOrder));
                        await OnSearchAsync();
                        return true;
                    }
                    else
                    {
                        // Server order missing; create a new one
                        var created = await _orderService.CreateAsync(orderItems);
                        if (created == null)
                        {
                            _dialogService.Error("Thất bại", "Tạo đơn hàng thất bại.");
                            return false;
                        }

                        // Ensure product names are synced to product IDs
                        SyncOrderItemProductNames(created.OrderItems);

                        Detail = created;
                        _selectedOrder = created;
                        OnPropertyChanged(nameof(SelectedOrder));
                        await OnSearchAsync();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error ensuring order exists: {ex.Message}");
                    _dialogService.Error("Lỗi", "Có lỗi xảy ra khi kiểm tra đơn trên server.");
                    return false;
                }
            }

            // If order is new locally (OrderId == -1), create it on server
            var createdOrder = await _orderService.CreateAsync(orderItems);
            if (createdOrder == null)
            {
                _dialogService.Error("Thất bại", "Tạo đơn hàng thất bại.");
                return false;
            }

            // Ensure product names are synced to product IDs
            SyncOrderItemProductNames(createdOrder.OrderItems);

            Detail = createdOrder;
            _selectedOrder = createdOrder;
            OnPropertyChanged(nameof(SelectedOrder));
            await OnSearchAsync();
            return true;
        }

        private static bool OrderItemsDiffer(List<OrderItem> a, List<OrderItem> b)
        {
            if (a == null && b == null) return false;
            if (a == null || b == null) return true;
            if (a.Count != b.Count) return true;

            // Compare by exact items without aggregating: handle duplicates by consuming matches
            var bCopy = new List<OrderItem>(b);

            foreach (var item in a)
            {
                var idx = bCopy.FindIndex(x => x.ProductId == item.ProductId
                                               && (x.Quantity ?? 0) == (item.Quantity ?? 0)
                                               && (x.UnitPrice ?? 0m) == (item.UnitPrice ?? 0m));
                if (idx == -1) return true;
                bCopy.RemoveAt(idx);
            }

            return bCopy.Count != 0;
        }

        private List<OrderItem> AggregateOrderItems(IEnumerable<OrderItem> items)
        {
            if (items == null)
            {
                return new List<OrderItem>();
            }

            var grouped = items
                .Where(i => i != null && i.ProductId != null)
                .GroupBy(i => i.ProductId)
                .Select(g => new OrderItem
                {
                    ProductId = g.Key,
                    ProductName = g.Select(x => x.ProductName).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty,
                    Quantity = g.Sum(x => x.Quantity ?? 0),
                    UnitPrice = g.Select(x => x.UnitPrice ?? 0m).FirstOrDefault(),
                    IsEditing = false
                })
                .ToList();

            // Include any items without ProductId (treat them individually)
            var withoutId = items.Where(i => i == null || i.ProductId == null).Select(i => new OrderItem
            {
                ProductId = i?.ProductId,
                ProductName = i?.ProductName ?? string.Empty,
                Quantity = i?.Quantity ?? 0,
                UnitPrice = i?.UnitPrice ?? 0m,
                IsEditing = false
            });

            grouped.AddRange(withoutId);

            return grouped;
        }

        private async Task OnApplyVoucherAsync(Order? order)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.VoucherCode))
            {
                _dialogService.Error("Cảnh báo", "Vui lòng nhập mã voucher.");
                return;
            }

            var voucherCode = order.VoucherCode.Trim();
            order.VoucherCode = voucherCode;

            IsLoading = true;

            try
            {
                // Call API to apply voucher - PUT request
                var result = await _orderService.ApplyVoucherAsync(order.OrderId, voucherCode);

                if (result != null)
                {
                    // Apply server-calculated values so users can retry with another voucher immediately.
                    order.VoucherCode = result.VoucherCode;
                    order.SubTotal = result.SubTotal;
                    order.DiscountAmount = result.DiscountAmount;
                    order.FinalTotal = result.FinalTotal;
                    _dialogService.Success("Thành công", "Áp dụng voucher thành công.");
                }
                else
                {
                    _dialogService.Error("Thất bại", "Không thể áp dụng voucher.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying voucher: {ex.Message}");

                if (ex.Message.Contains("Invalid or Expired Voucher", StringComparison.OrdinalIgnoreCase))
                {
                    _dialogService.Error("Voucher không hợp lệ", "Voucher không hợp lệ hoặc đã hết hạn. Vui lòng thử mã khác.");
                }
                else
                {
                    _dialogService.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }



        private async Task OnGoBackAsync()
        {
            if (Detail == null)
                return;

            // Nếu đang payment mode
            // chỉ thoát payment mode
            if (IsPaymentMode)
            {
                IsPaymentMode = false;
                return;
            }

            await RollbackDetailToDraftAsync(true);

            // Xóa dữ liệu tạm thời khi hoàn tất đơn hàng
            _tempDataService.DeleteTemporaryData("Orders");
        }

        private async Task OnPrevPageAsync()
        {
            if (PageIndex > 1)
            {
                PageIndex--;
                await OnSearchAsync();
            }
        }

        private async Task OnNextPageAsync()
        {
            if (PageIndex < TotalPages)
            {
                PageIndex++;
                await OnSearchAsync();
            }
        }

        private void UpdatePaginationInfo(int totalItems)
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            CanPrevPage = PageIndex > 1;
            CanNextPage = PageIndex < TotalPages;
            PageInfo = $"Page {PageIndex} / {TotalPages}";
        }

        // =====================================
        // Auto-Save and Recovery Methods
        // =====================================

        /// <summary>
        /// Khôi phục dữ liệu tạm thời nếu ứng dụng bị tắt bất ngờ
        /// </summary>
        private async Task RecoverDataAsync()
        {
            var snapshot = await TryRecoverDataAsync<OrdersViewModelSnapshot>(_tempDataService, "Orders");
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
                        _tempDataService.DeleteTemporaryData("Orders");
                        return;
                    }
                }
                else
                {
                    // Cannot determine current user ID, skip recovery
                    return;
                }

                if(RecoveryHelper.ShowRecoveryDialog(new List<string> { "Orders" }) != true)
                {     
                    // User declined recovery, delete temp data
                    _tempDataService.DeleteTemporaryData("Orders");
                    return;
                }

                // Khôi phục các filter
                if (snapshot.FromDate.HasValue)
                    FromDate = snapshot.FromDate.Value;

                if (snapshot.ToDate.HasValue)
                    ToDate = snapshot.ToDate.Value;

                if (!string.IsNullOrEmpty(snapshot.SelectedStatus))
                    SelectedStatus = snapshot.SelectedStatus;

                // Khôi phục đơn hàng đang chỉnh sửa
                if (snapshot.Detail != null)
                {
                    Detail = snapshot.Detail;

                    // Khôi phục các sản phẩm đã thêm vào đơn hàng
                    if (snapshot.OrderItems != null && snapshot.OrderItems.Count > 0)
                    {
                        Detail.OrderItems = new ObservableCollection<OrderItem>(
                            snapshot.OrderItems.Select(item => new OrderItem
                            {
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                Quantity = item.Quantity,
                                UnitPrice = item.Price
                            })
                        );
                    }
                }

                PageIndex = snapshot.CurrentPage;
                PageSize = snapshot.PageSize;
                CommitRecovery(_tempDataService, "Orders");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi khôi phục dữ liệu: {ex.Message}");
                // Nếu lỗi, xóa dữ liệu để tránh lặp lại
                _tempDataService.DeleteTemporaryData("Orders");
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

            return new OrdersViewModelSnapshot
            {
                UserId = currentUserId,
                FromDate = FromDate,
                ToDate = ToDate,
                SelectedStatus = SelectedStatus,
                Detail = Detail,
                OrderItems = Detail?.OrderItems != null
                    ? Detail.OrderItems.Select(item => new OrderItemSnapshot
                    {
                        ProductId = item.ProductId ?? 0,
                        ProductName = item.ProductName,
                        Price = item.UnitPrice ?? 0,
                        Quantity = item.Quantity.HasValue ? item.Quantity.Value : 0
                    }).ToList()
                    : null,
                CurrentPage = PageIndex,
                PageSize = PageSize
            };
        }

        private void Detail_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Order.Status) or nameof(Order.StatusText) or nameof(Order.PaymentMethod) or null)
            {
                OnPropertyChanged(nameof(ShowPaymentDetails));
                OnPropertyChanged(nameof(IsPaidOrder));
                OnPropertyChanged(nameof(IsCancelledOrder));
                OnPropertyChanged(nameof(IsFinalizedOrder));
            }
            if (e.PropertyName is nameof(Order.OrderId))
            {
                OnPropertyChanged(nameof(IsNewOrder));
            }
        }
    }
}

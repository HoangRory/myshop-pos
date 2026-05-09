using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using LuciferCore.Attributes;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Orders")]
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isDetailPanelOpen;
        public bool IsDetailPanelOpen
        {
            get => _isDetailPanelOpen;
            set => SetProperty(ref _isDetailPanelOpen, value);
        }

        public bool ShowPaymentDetails =>
            Detail != null &&
            (IsPaymentMode || Detail.Status == (byte)OrderStatus.Paid);
        private bool _isPaymentMode;
        public bool IsPaymentMode
        {
            get => _isPaymentMode;
            set => SetProperty(ref _isPaymentMode, value);
        }
        public bool IsPaidOrder =>
            Detail?.Status == (byte)OrderStatus.Paid;
        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

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

                    UpdateDetailState();
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
        public ICommand ResetCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand ViewOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand EnterPaymentModeCommand { get; }
        public ICommand ApplyPaymentCommand { get; }
        public ICommand ExitPaymentModeCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public OrdersViewModel(IOrderService orderService, IProductService productService, IDialogService dialogService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Initialize status options
            StatusOptions.Add("All");
            StatusOptions.Add("Chờ thanh toán");
            StatusOptions.Add("Đã thanh toán");
            StatusOptions.Add("Đã hủy");

            // Initialize commands with async support
            SearchCommand = new AsyncRelayCommand(_ => OnSearchAsync());
            ResetCommand = new RelayCommand(_ => OnReset());
            CreateOrderCommand = new RelayCommand(_ => OnCreateOrder());
            ViewOrderCommand = new RelayCommand(param => OnViewOrder(param));
            EditOrderCommand = new RelayCommand(param => OnEditOrder(param));
            DeleteOrderCommand = new AsyncRelayCommand<Order>(OnDeleteOrderAsync);
            AddProductCommand = new RelayCommand(_ => OnAddProduct());
            RemoveItemCommand = new RelayCommand(param => OnRemoveItem(param));
            EnterPaymentModeCommand = new RelayCommand(_ => OnEnterPaymentMode());
            ApplyPaymentCommand = new AsyncRelayCommand(_ => OnApplyPaymentAsync(), _ => Detail != null);
            ExitPaymentModeCommand = new RelayCommand(_ => OnExitPaymentMode());
            CancelEditCommand = new RelayCommand(_ => OnCancelEdit());
            PrevPageCommand = new AsyncRelayCommand(_ => OnPrevPageAsync(), _ => CanPrevPage);
            NextPageCommand = new AsyncRelayCommand(_ => OnNextPageAsync(), _ => CanNextPage);

            // Load initial data
            InitializeDataAsync();
        }

        private void OnInitializeCommandsIfNeeded()
        {
            // This method ensures commands can be reconstructed if needed
            // Not typically needed, but available for dynamic command updates
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
                await ReloadOrdersAsync();

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

                // Calculate total pages based on total count
                TotalPages = total > 0 ? (int)Math.Ceiling(total / (double)PageSize) : 1;
                UpdatePaginationInfo();
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

        private void OnReset()
        {
            SearchKeyword = string.Empty;
            SelectedStatus = "All";
            FromDate = null;
            ToDate = null;
            PageIndex = 1;
            Orders.Clear();
            Detail = null;
            SelectedOrder = null;

            // Reload all orders after reset
            _ = ReloadOrdersAsync();
        }

        private void SetDateText(string? input, bool isFromDate)
        {
            var normalized = NormalizeDateInput(input);

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

        private static string NormalizeDateInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return input.Trim();
        }

        private void OnCreateOrder()
        {
            Detail = new Order
            {
                OrderId = -1, // Temporary ID for new order
                CreatedAt = DateTime.Now,
                Status = (byte)OrderStatus.Pending,
                PaymentMethod = null,
                OrderItems = new ObservableCollection<OrderItem>()
            };

            IsPaymentMode = false;
        }

        private async Task LoadOrderDetailAsync(Order order)
        {
            if (order == null)
                return;

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

        private void OnViewOrder(object? param)
        {
            if (param is Order order)
            {
                _ = LoadOrderDetailAsync(order);
            }
        }

        private void OnEditOrder(object? param)
        {
            if (param is Order order)
            {
                _ = LoadOrderDetailAsync(order);
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

                    // Reload list to sync with server
                    await ReloadOrdersAsync();
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
            // Allow adding products only when Detail exists and payment has not been applied (PaymentMethod == null) and not paid
            if (Detail != null && Detail.PaymentMethod == null && Detail.Status != (byte)OrderStatus.Paid)
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
            if (Detail != null && Detail.PaymentMethod == null && Detail.Status != (byte)OrderStatus.Paid && param is OrderItem item)
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
                // If order is new, create it on server first so it has an OrderId
                if (Detail.OrderId == -1)
                {
                    var orderItems = Detail.OrderItems.ToList();
                    var createdOrder = await _orderService.CreateAsync(orderItems);
                    if (createdOrder != null)
                    {
                        Detail = createdOrder;
                        SelectedOrder = createdOrder;
                        await ReloadOrdersAsync();
                    }
                    else
                    {
                        _dialogService.Error("Thất bại", "Tạo đơn hàng thất bại.");
                        return;
                    }
                }

                // Recalculate subtotal and default final total
                Detail.SubTotal = Detail.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
                Detail.FinalTotal = (Detail.SubTotal ?? 0m) - (Detail.DiscountAmount ?? 0m);

                // Enter payment mode (lock editing via XAML bindings)
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
                // Ensure order exists on server
                if (Detail.OrderId == -1)
                {
                    var orderItems = Detail.OrderItems.ToList();
                    var createdOrder = await _orderService.CreateAsync(orderItems);
                    if (createdOrder != null)
                    {
                        Detail = createdOrder;
                        SelectedOrder = createdOrder;
                        await ReloadOrdersAsync();
                    }
                    else
                    {
                        _dialogService.Error("Thất bại", "Tạo đơn hàng thất bại.");
                        return;
                    }
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
                    Detail = updatedOrder;
                    SelectedOrder = updatedOrder;
                    await ReloadOrdersAsync();
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

        private void OnExitPaymentMode()
        {
            IsPaymentMode = false;
        }

        private void OnCancelEdit()
        {
            Detail = null;
            SelectedOrder = null;
            IsPaymentMode = false;
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

        private async Task ReloadOrdersAsync()
        {
            IsLoading = true;
            try
            {
                // Load all orders from service
                var orders = await _orderService.GetAllAsync();

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                // Recalculate pagination based on loaded data
                TotalPages = Math.Max(1, (int)Math.Ceiling(Orders.Count / (double)PageSize));
                UpdatePaginationInfo();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reloading orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdatePaginationInfo()
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling(Orders.Count / (double)PageSize));
            CanPrevPage = PageIndex > 1;
            CanNextPage = PageIndex < TotalPages;
            PageInfo = $"Page {PageIndex}";
        }

        private void Detail_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Order.Status) or nameof(Order.StatusText) or nameof(Order.PaymentMethod) or null)
            {
                UpdateDetailState();
            }
        }

        private void UpdateDetailState()
        {

            OnPropertyChanged(nameof(ShowPaymentDetails));
            OnPropertyChanged(nameof(IsPaidOrder));
            OnPropertyChanged(nameof(IsPaymentMode));
        }
    }
}

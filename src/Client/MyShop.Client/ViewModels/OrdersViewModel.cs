using System.Collections.ObjectModel;
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
        public DateTime? FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        private Order? _detail;
        public Order? Detail
        {
            get => _detail;
            set => SetProperty(ref _detail, value);
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
        public ICommand SaveOrderCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public OrdersViewModel(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

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
            SaveOrderCommand = new AsyncRelayCommand(_ => OnSaveOrderAsync(), _ => Detail != null);
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

        private void OnCreateOrder()
        {
            Detail = new Order
            {
                OrderId = -1, // Temporary ID for new order
                CreatedAt = DateTime.Now,
                Status = (byte)OrderStatus.Pending,
                OrderItems = new ObservableCollection<OrderItem>()
            };
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
            if (Detail != null)
            {
                var newItem = new OrderItem
                {
                    ProductName = string.Empty,
                    Quantity = 1,
                    UnitPrice = 0,
                    IsEditing = true
                };
                Detail.OrderItems.Add(newItem);
            }
        }

        private void OnRemoveItem(object? param)
        {
            if (Detail != null && param is OrderItem item)
            {
                Detail.OrderItems.Remove(item);
            }
        }

        private async Task OnSaveOrderAsync()
        {
            if (Detail == null)
                return;

            IsLoading = true;

            try
            {
                if (Detail.OrderId == -1)
                {
                    var orderItems = Detail.OrderItems.ToList();

                    var createdOrder =
                        await _orderService.CreateAsync(orderItems);

                    if (createdOrder != null)
                    {
                        Detail = createdOrder;

                        SelectedOrder = createdOrder;

                        await ReloadOrdersAsync();
                    }
                }
                else
                {
                    var updatedOrder = await _orderService.UpdateAsync(Detail);

                    if (updatedOrder != null)
                    {
                        // Update current detail
                        Detail = updatedOrder;

                        // Update selected order
                        SelectedOrder = updatedOrder;

                        // Reload list from server
                        await ReloadOrdersAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error saving order: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnCancelEdit()
        {
            Detail = null;
            SelectedOrder = null;
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
    }
}

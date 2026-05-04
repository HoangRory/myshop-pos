using System.Collections.ObjectModel;
using System.Windows.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using LuciferCore.Attributes;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Orders")]
    public class OrdersViewModel : BaseViewModel
    {
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
                    // Load order details when selected
                    LoadOrderDetail(value);
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

        public OrdersViewModel()
        {
            // Initialize status options
            StatusOptions.Add("All");
            StatusOptions.Add("Pending");
            StatusOptions.Add("Completed");
            StatusOptions.Add("Cancelled");

            // Initialize commands
            SearchCommand = new RelayCommand(_ => OnSearch());
            ResetCommand = new RelayCommand(_ => OnReset());
            CreateOrderCommand = new RelayCommand(_ => OnCreateOrder());
            ViewOrderCommand = new RelayCommand(param => OnViewOrder(param));
            EditOrderCommand = new RelayCommand(param => OnEditOrder(param));
            DeleteOrderCommand = new RelayCommand(param => OnDeleteOrder(param));
            AddProductCommand = new RelayCommand(_ => OnAddProduct());
            RemoveItemCommand = new RelayCommand(param => OnRemoveItem(param));
            SaveOrderCommand = new RelayCommand(_ => OnSaveOrder(), _ => Detail != null);
            CancelEditCommand = new RelayCommand(_ => OnCancelEdit());
            PrevPageCommand = new RelayCommand(_ => OnPrevPage(), _ => CanPrevPage);
            NextPageCommand = new RelayCommand(_ => OnNextPage(), _ => CanNextPage);

            // Load initial data
            InitializeData();
        }

        private void InitializeData()
        {
            IsLoading = true;
            try
            {
                // Initialize available products
                AvailableProducts.Add(new Product { ProductId = 1, Name = "Product A", SalePrice = 500000, Sku = "SKU001" });
                AvailableProducts.Add(new Product { ProductId = 2, Name = "Product B", SalePrice = 500000, Sku = "SKU002" });
                AvailableProducts.Add(new Product { ProductId = 3, Name = "Product C", SalePrice = 800000, Sku = "SKU003" });
                AvailableProducts.Add(new Product { ProductId = 4, Name = "Product D", SalePrice = 500000, Sku = "SKU004" });

                // Sample data - replace with actual API call
                Orders.Add(new Order
                {
                    OrderId = "ORD001",
                    Date = DateTime.Now.AddDays(-5),
                    TotalAmount = 1500000,
                    Status = "Completed",
                    OrderItems = new ObservableCollection<OrderItem>
                    {
                        new OrderItem { ProductName = "Product A", Quantity = 2, Price = 500000 },
                        new OrderItem { ProductName = "Product B", Quantity = 1, Price = 500000 }
                    }
                });

                Orders.Add(new Order
                {
                    OrderId = "ORD002",
                    Date = DateTime.Now.AddDays(-3),
                    TotalAmount = 800000,
                    Status = "Pending",
                    OrderItems = new ObservableCollection<OrderItem>
                    {
                        new OrderItem { ProductName = "Product C", Quantity = 1, Price = 800000 }
                    }
                });

                Orders.Add(new Order
                {
                    OrderId = "ORD003",
                    Date = DateTime.Now.AddDays(-1),
                    TotalAmount = 2000000,
                    Status = "Pending",
                    OrderItems = new ObservableCollection<OrderItem>
                    {
                        new OrderItem { ProductName = "Product D", Quantity = 4, Price = 500000 }
                    }
                });

                SelectedStatus = "All";
                UpdatePaginationInfo();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSearch()
        {
            IsLoading = true;
            try
            {
                // Filter based on search keyword, status, and date range
                PageIndex = 1;
                UpdatePaginationInfo();
                // TODO: Call API with filters
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
            InitializeData();
        }

        private void OnCreateOrder()
        {
            Detail = new Order
            {
                OrderId = "ORD-NEW",
                Date = DateTime.Now,
                Status = "Pending",
                OrderItems = new ObservableCollection<OrderItem>()
            };
        }

        private void LoadOrderDetail(Order order)
        {
            IsLoading = true;
            try
            {
                // Clone the order for editing
                Detail = new Order
                {
                    OrderId = order.OrderId,
                    Date = order.Date,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderItems = new ObservableCollection<OrderItem>(order.OrderItems)
                };
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
                LoadOrderDetail(order);
            }
        }

        private void OnEditOrder(object? param)
        {
            if (param is Order order)
            {
                LoadOrderDetail(order);
            }
        }

        private void OnDeleteOrder(object? param)
        {
            if (param is Order order)
            {
                // TODO: Show confirmation dialog
                // TODO: Call API to delete
                Orders.Remove(order);
                if (Detail?.OrderId == order.OrderId)
                {
                    Detail = null;
                }
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
                    Price = 0,
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

        private void OnSaveOrder()
        {
            if (Detail == null)
                return;

            IsLoading = true;
            try
            {
                // TODO: Call API to save order
                // For now, just update the list
                var existingOrder = Orders.FirstOrDefault(o => o.OrderId == Detail.OrderId);
                if (existingOrder != null)
                {
                    // Update existing
                    existingOrder.Date = Detail.Date;
                    existingOrder.Status = Detail.Status;
                    existingOrder.TotalAmount = Detail.OrderItems.Sum(i => i.Total);
                    existingOrder.OrderItems = Detail.OrderItems;
                }
                else
                {
                    // Add new
                    Detail.TotalAmount = Detail.OrderItems.Sum(i => i.Total);
                    Orders.Add(Detail);
                }

                Detail = null;
                SelectedOrder = null;
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

        private void OnPrevPage()
        {
            if (PageIndex > 1)
            {
                PageIndex--;
                UpdatePaginationInfo();
                // TODO: Load previous page data
            }
        }

        private void OnNextPage()
        {
            PageIndex++;
            UpdatePaginationInfo();
            // TODO: Load next page data
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

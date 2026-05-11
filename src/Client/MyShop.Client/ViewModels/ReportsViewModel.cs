using LiveCharts;
using LiveCharts.Wpf;
using LuciferCore.Attributes;
using MyShop.Client.Helpers;
using MyShop.Client.Models.Report;
using MyShop.Client.Services.Interfaces;
using System.ComponentModel;
using System.Windows.Input;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Reports")]
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly IReportService _reportService;
        private readonly IDialogService _dialogService;

        public event PropertyChangedEventHandler? PropertyChanged;

        // --- States ---
        private bool _isLoaded = true;
        public bool IsLoaded
        {
            get => _isLoaded;
            set { _isLoaded = value; OnPropertyChanged(nameof(IsLoaded)); }
        }

        private DateTime _fromDate = DateTime.Now.AddMonths(-1);
        public DateTime FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(nameof(FromDate)); } }

        private DateTime _toDate = DateTime.Now;
        public DateTime ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(nameof(ToDate)); } }

        private int _selectedGroupType = 1; // Mặc định là 1 (Ngày)
        public int SelectedGroupType
        {
            get => _selectedGroupType;
            set
            {
                _selectedGroupType = value;
                OnPropertyChanged(nameof(SelectedGroupType));
                _ = LoadReportData(); // Tự động load lại khi đổi kiểu nhóm
            }
        }

        // --- Chart Data ---
        public SeriesCollection ProductSeries { get; set; } = new();
        public SeriesCollection RevenueSeries { get; set; } = new();

        private List<string> _labels = new();
        public List<string> Labels { get => _labels; set { _labels = value; OnPropertyChanged(nameof(Labels)); } }

        // Định dạng hiển thị tiền tệ trên trục Y
        public Func<double, string> Formatter { get; set; } = value => value.ToString("N0") + " đ";

        public ICommand RefreshCommand { get; }

        public ReportsViewModel(IReportService reportService, IDialogService dialogService)
        {
            _reportService = reportService;
            _dialogService = dialogService;

            RefreshCommand = new RelayCommand(async _ => await LoadReportData(), _ => IsLoaded);
        }

        public void LoadData()
        {
            // Vì Reflection gọi method đồng bộ, ta dùng Fire and Forget để load async
            _ = LoadReportData();
        }

        public async Task LoadReportData()
        {
            if (FromDate > ToDate)
            {
                _dialogService.Error("Lỗi", "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
                return;
            }

            IsLoaded = false;
            try
            {
                var filter = new Server.Handler.Report.ReportFilter
                {
                    FromDate = FromDate,
                    ToDate = ToDate,
                    GroupType = SelectedGroupType // Theo ngày
                };

                // 1. Lấy và cập nhật báo cáo Sản phẩm (Line Chart)
                var products = await _reportService.GetProductReportAsync(filter);
                UpdateProductChart(products);

                // 2. Lấy và cập nhật báo cáo Doanh thu (Column Chart)
                var revenues = await _reportService.GetRevenueReportAsync(filter);
                UpdateRevenueChart(revenues);
            }
            catch (Exception ex)
            {
                _dialogService.Error("Lỗi tải báo cáo", ex.Message);
            }
            finally
            {
                IsLoaded = true;
            }
        }

        private void UpdateProductChart(List<ProductReport> data)
        {
            ProductSeries.Clear();
            if (data == null || data.Count == 0) return;

            var firstWithData = data.FirstOrDefault(x => x.Series.Any());
            if (firstWithData != null)
            {
                Labels = firstWithData.Series.Select(s => s.Time).ToList();
            }

            foreach (var product in data)
            {
                ProductSeries.Add(new LineSeries
                {
                    Title = product.ProductName,
                    Values = new ChartValues<int>(product.Series.Select(s => s.Quantity)),
                    StrokeThickness = 3,         // Tăng độ dày đường để dễ nhìn
                    PointGeometrySize = 10,      // Hiện các điểm chấm tròn
                    Fill = System.Windows.Media.Brushes.Transparent // Không tô màu vùng dưới đường
                });
            }

            // --- BỔ SUNG DÒNG NÀY ---
            OnPropertyChanged(nameof(ProductSeries));
        }

        private void UpdateRevenueChart(List<RevenueReport> data)
        {
            RevenueSeries.Clear();
            if (data == null || data.Count == 0) return;

            var revValues = new ChartValues<decimal>(data.Select(r => r.Revenue));
            var profitValues = new ChartValues<decimal>(data.Select(r => r.Profit));

            RevenueSeries.Add(new ColumnSeries
            {
                Title = "Doanh thu",
                Values = revValues,
                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#2F6F9F")
            });

            RevenueSeries.Add(new ColumnSeries
            {
                Title = "Lợi nhuận",
                Values = profitValues,
                Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#4CAF50")
            });

            // Nếu biểu đồ doanh thu có timeline khác sản phẩm, cập nhật lại Labels ở đây
            Labels = data.Select(r => r.Time).ToList();

            OnPropertyChanged(nameof(RevenueSeries));
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
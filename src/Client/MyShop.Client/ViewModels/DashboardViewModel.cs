using LiveCharts;
using LiveCharts.Wpf;
using LuciferCore.Attributes;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Dashboard")]
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly IDashboardService _dashboardService;
        private readonly IDialogService _dialogService;
        private readonly IImageService _imageService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private Dashboard _dashboardData = new();
        public Dashboard DashboardData
        {
            get => _dashboardData;
            set { _dashboardData = value; OnPropertyChanged(); }
        }

        private bool _isLoaded = true;
        public bool IsLoaded
        {
            get => _isLoaded;
            set { _isLoaded = value; OnPropertyChanged(); }
        }

        // --- LiveCharts Properties (Xử lý biểu đồ không hard-code) ---
        public SeriesCollection RevenueSeries { get; set; } = new();
        private List<string> _labels = new();
        public List<string> Labels { get => _labels; set { _labels = value; OnPropertyChanged(nameof(Labels)); } }
        public Func<double, string> Formatter { get; set; } = value => value.ToString("N0") + " đ";

        public DashboardViewModel(IDashboardService dashboardService, IDialogService dialogService, IImageService imageService)
        {
            _dashboardService = dashboardService;
            _dialogService = dialogService;
            _imageService = imageService;
        }

        public void LoadData()
        {
            // Vì Reflection gọi method đồng bộ, ta dùng Fire and Forget để load async
            _ = LoadDataInternalAsync();
        }

        private async Task LoadDataInternalAsync()
        {
            if (IsLoaded == false) return; // Tránh re-entry nếu đang load dở

            IsLoaded = false;
            try
            {
                var data = await _dashboardService.GetDashboardDataAsync();
                if (data != null)
                {
                    DashboardData = data;
                    UpdateRevenueChart(data.MonthlyRevenueChart);

                    foreach (var p in data.BestSellingProducts) _ = LoadProductThumbnailAsync(p);
                    foreach (var p in data.LowStockProducts) _ = LoadProductThumbnailAsync(p);
                }
            }
            catch (Exception ex)
            {
                _dialogService.Error("Lỗi", "Không thể nạp dữ liệu Dashboard.");
            }
            finally
            {
                IsLoaded = true;
            }
        }

        private void UpdateRevenueChart(List<decimal?> chartData)
        {
            RevenueSeries.Clear();
            if (chartData == null || chartData.Count == 0) return;

            RevenueSeries.Add(new LineSeries
            {
                Title = "Doanh thu",
                Values = new ChartValues<decimal>(chartData.Select(x => x ?? 0)),
                PointGeometrySize = 8,
                StrokeThickness = 3,
                // Gradient đổ bóng mượt mà giống Reports
                Fill = new System.Windows.Media.LinearGradientBrush
                {
                    StartPoint = new System.Windows.Point(0, 0),
                    EndPoint = new System.Windows.Point(0, 1),
                    GradientStops = new System.Windows.Media.GradientStopCollection {
                        new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb(60, 47, 111, 159), 0),
                        new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Transparent, 1)
                    }
                }
            });

            Labels = Enumerable.Range(1, chartData.Count).Select(i => i.ToString()).ToList();
            OnPropertyChanged(nameof(RevenueSeries));
        }

        private async Task LoadProductThumbnailAsync(Product? product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.Images)) return;
            try
            {
                var firstImage = _imageService.ParseImageNames(product.Images).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(firstImage)) return;

                var bytes = await _imageService.DownloadImageAsync(firstImage);
                if (bytes == null || bytes.Length == 0) return;

                using var ms = new MemoryStream(bytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze(); // Chốt thread để hiển thị trên UI

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => { product.Thumbnail = bitmap; });
            }
            catch { /* Img load failed silent */ }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
namespace MyShop.Client.Services.Interfaces
{
    /// <summary>
    /// Service để tự động lưu và phục hồi dữ liệu tạm thời khi ứng dụng tắt đột ngột
    /// </summary>
    public interface ITemporaryDataService
    {
        /// <summary>
        /// Khởi động dịch vụ auto-save với chu kỳ cập nhật
        /// </summary>
        void Start(int saveIntervalMilliseconds = 30000); // Mặc định 30 giây

        /// <summary>
        /// Dừng dịch vụ auto-save
        /// </summary>
        void Stop();

        /// <summary>
        /// Lưu dữ liệu ngay lập tức cho một ViewModel
        /// </summary>
        Task SaveAsync(string viewModelName, object? data);

        /// <summary>
        /// Tải dữ liệu tạm thời cho một ViewModel
        /// </summary>
        Task<T?> LoadAsync<T>(string viewModelName) where T : class;

        /// <summary>
        /// Kiểm tra xem có dữ liệu tạm thời cho một ViewModel hay không
        /// </summary>
        bool HasTemporaryData(string viewModelName);

        /// <summary>
        /// Xóa dữ liệu tạm thời cho một ViewModel
        /// </summary>
        void DeleteTemporaryData(string viewModelName);

        /// <summary>
        /// Xóa tất cả dữ liệu tạm thời
        /// </summary>
        void ClearAllTemporaryData();

        /// <summary>
        /// Đăng ký một ViewModel để tự động lưu dữ liệu
        /// </summary>
        void RegisterViewModel(string viewModelName, Func<object?> dataGetter);

        /// <summary>
        /// Hủy đăng ký một ViewModel
        /// </summary>
        void UnregisterViewModel(string viewModelName);
    }
}

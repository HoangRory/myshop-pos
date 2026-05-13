namespace MyShop.Client
{
    /// <summary>
    /// Quản lý trạng thái toàn cục của ứng dụng
    /// </summary>
    public static class AppState
    {
        /// <summary>
        /// Danh sách các ViewModel cần phục hồi dữ liệu tạm thời
        /// </summary>
        public static HashSet<string> ViewModelsToRecover { get; set; } = new();
    }
}

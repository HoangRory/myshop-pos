using MyShop.Client.Services.Interfaces;
using System.Windows;

namespace MyShop.Client.Helpers
{
    /// <summary>
    /// Helper để xử lý phục hồi dữ liệu tạm thời sau khi ứng dụng tắt đột ngột
    /// </summary>
    public static class RecoveryHelper
    {
        /// <summary>
        /// Hiển thị dialog hỏi người dùng có muốn khôi phục dữ liệu hay không
        /// </summary>
        public static bool ShowRecoveryDialog(List<string> viewModelNames)
        {
            if (viewModelNames.Count == 0)
                return false;

            var message = "MyShop phát hiện ứng dụng bị tắt bất ngờ.\n\n" +
                         "Các dữ liệu chưa lưu sau đây có thể được khôi phục:\n";

            foreach (var name in viewModelNames)
            {
                message += $"• {GetFriendlyViewModelName(name)}\n";
            }

            message += "\nBạn có muốn khôi phục dữ liệu này không?";

            var result = MessageBox.Show(
                message,
                "Khôi phục dữ liệu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes
            );

            return result == MessageBoxResult.Yes;
        }

        private static string GetFriendlyViewModelName(string viewModelName)
        {
            var nameMap = new Dictionary<string, string>
            {
                { "Orders", "Đơn hàng" },
                { "Products", "Sản phẩm" },
                { "Reports", "Báo cáo" },
                { "Dashboard", "Bảng điều khiển" },
                { "Settings", "Cài đặt" },
                { "BR", "Sao lưu & Khôi phục" }
            };

            return nameMap.TryGetValue(viewModelName, out var friendly) ? friendly : viewModelName;
        }
    }
}

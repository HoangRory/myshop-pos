namespace MyShop.Client.Services
{
    /// <summary>
    /// Service để broadcast khi cài đặt ứng dụng thay đổi
    /// </summary>
    public static class AppSettingsService
    {
        public static event EventHandler? ItemsPerPageChanged;

        public static void NotifyItemsPerPageChanged()
        {
            ItemsPerPageChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}

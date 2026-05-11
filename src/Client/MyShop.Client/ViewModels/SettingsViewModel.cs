using LuciferCore.Attributes;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Settings")]
    public class SettingsViewModel : BaseViewModel
    {
        public string PageTitle { get; } = "Cài đặt";
    }
}

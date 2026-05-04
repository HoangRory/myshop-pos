using System.Windows;
using MyShop.Client.Services.Interfaces;

namespace MyShop.Client.Services
{
    public class DialogService : IDialogService
    {
        public bool Confirm(string title, string message)
        {
            return MessageBox.Show(message, title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        public void Success(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

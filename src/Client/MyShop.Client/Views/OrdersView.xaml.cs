using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Views
{
    /// <summary>
    /// Interaction logic for OrdersView.xaml
    /// </summary>
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            InitializeComponent();
            // Get OrdersViewModel from DI container
            this.DataContext = DIContainer.ServiceProvider.GetRequiredService<OrdersViewModel>();
        }
    }
}

using System.Windows.Controls;
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
            this.DataContext = new OrdersViewModel();
        }
    }
}

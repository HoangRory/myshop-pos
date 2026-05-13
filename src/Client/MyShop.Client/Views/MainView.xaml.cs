using System.Windows.Controls;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            SizeChanged += MainView_SizeChanged;
        }

        private void MainView_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.HostWidthChanged(ActualWidth);
            }
        }
    }
}
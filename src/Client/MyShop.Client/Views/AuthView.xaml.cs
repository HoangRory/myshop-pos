using MyShop.Client.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MyShop.Client.Views
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
        }

        private void ToSignup_Click(object sender, RoutedEventArgs e)
        {
            // Kích hoạt trạng thái Signup trong XAML
            VisualStateManager.GoToState(this, "SignupState", true);
            if (DataContext is AuthViewModel vm) vm.IsLoginMode = false;
        }

        private void ToLogin_Click(object sender, RoutedEventArgs e)
        {
            // Kích hoạt trạng thái Login trong XAML
            VisualStateManager.GoToState(this, "LoginState", true);
            if (DataContext is AuthViewModel vm) vm.IsLoginMode = true;
        }
    }
}

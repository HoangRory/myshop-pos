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

        // Khi View (Giao diện) đã sẵn sàng
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Ép kiểu DataContext về AuthViewModel của bạn
            if (DataContext is AuthViewModel vm)
            {
                // Kiểm tra nếu ViewModel đã có chuỗi Dummy từ hàm TryAutoLogin
                if (!string.IsNullOrEmpty(vm.Password))
                {
                    // Đẩy giá trị từ ViewModel vào Control thủ công
                    PassBox.Password = vm.Password;
                }
            }
        }
    }
}

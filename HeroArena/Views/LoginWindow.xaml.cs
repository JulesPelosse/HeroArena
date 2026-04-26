using System.Windows;
using System.Windows.Controls;
using HeroArena.ViewModels;

namespace HeroArena.Views
{
    public partial class LoginWindow : Window
    {
        private LoginVMX _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginVMX();
            this.DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}
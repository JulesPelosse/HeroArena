using System.Windows;
using HeroArena.ViewModels;

namespace HeroArena.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = new SettingsVMX();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
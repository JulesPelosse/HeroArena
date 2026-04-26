using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using HeroArena.Services;

namespace HeroArena.ViewModels
{
    public class SettingsVMX : INotifyPropertyChanged
    {
        private string _connectionString;
        private string _statusMessage;

        public string ConnectionString
        {
            get => _connectionString;
            set { _connectionString = value; OnPropertyChanged(nameof(ConnectionString)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public ICommand SaveConnectionCommand { get; }
        public ICommand InitializeDatabaseCommand { get; }

        public SettingsVMX()
        {

            ConnectionString = Properties.Settings.Default.ConnectionString;
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ExerciceHero;Trusted_Connection=True;";
            }

            SaveConnectionCommand = new RelayCommand(ExecuteSaveConnection);
            InitializeDatabaseCommand = new RelayCommand(ExecuteInitializeDatabase);
        }

        private void ExecuteSaveConnection(object parameter)
        {
            try
            {
                Properties.Settings.Default.ConnectionString = ConnectionString;
                Properties.Settings.Default.Save();
                StatusMessage = "✓ Chaîne de connexion sauvegardée";
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"❌ Erreur : {ex.Message}";
            }
        }

        private void ExecuteInitializeDatabase(object parameter)
        {
            try
            {
                DatabaseService.InitializeDatabase();
                StatusMessage = "✓ Base de données initialisée avec succès !";
                MessageBox.Show("Base de données initialisée avec les données par défaut.\n\nLogin: admin\nPassword: admin123",
                                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"❌ Erreur : {ex.Message}";
                MessageBox.Show($"Erreur lors de l'initialisation : {ex.Message}",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
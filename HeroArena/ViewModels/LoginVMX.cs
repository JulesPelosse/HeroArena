using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HeroArena.Data;
using HeroArena.Helpers;
using HeroArena.Models;
using HeroArena.Views;

namespace HeroArena.ViewModels
{
    public class LoginVMX : INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username { get => _username; set { _username = value; OnPropertyChanged(nameof(Username)); } }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(nameof(Password)); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }

        public ICommand LoginCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginVMX()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        private void ExecuteLogin(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Veuillez saisir un pseudo et un mot de passe.";
                    return;
                }

                using (var context = new HeroArenaContext())
                {
                    var user = context.Logins.FirstOrDefault(l => l.Username == Username);

                    // Vérification du mot de passe haché
                    if (user != null && PasswordHelper.VerifyPassword(Password, user.PasswordHash))
                    {
                        var mainWindow = new MainWindow { DataContext = new MainVMX() };
                        mainWindow.Show();
                        Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
                    }
                    else
                    {
                        ErrorMessage = "Identifiants incorrects.";
                    }
                }
            }
            catch (Exception ex) { ErrorMessage = "Erreur BDD : " + ex.Message; }
        }

        private void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir les deux champs pour créer un compte.";
                return;
            }

            try
            {
                using (var context = new HeroArenaContext())
                {
                    // 1. Vérifier si l'utilisateur existe déjà
                    if (context.Logins.Any(l => l.Username == Username))
                    {
                        ErrorMessage = "Ce pseudo est déjà utilisé.";
                        return;
                    }

                    // 2. Créer le compte Login avec le mot de passe haché
                    var newLogin = new Login
                    {
                        Username = Username,
                        PasswordHash = PasswordHelper.HashPassword(Password) // Hachage ici !
                    };

                    context.Logins.Add(newLogin);
                    context.SaveChanges(); // On sauvegarde pour obtenir l'ID

                    // 3. Créer le profil Joueur associé
                    var newPlayer = new Player
                    {
                        Name = Username, // On donne le pseudo comme nom par défaut
                        LoginID = newLogin.ID
                    };

                    context.Players.Add(newPlayer);
                    context.SaveChanges();

                    ErrorMessage = "✅ Compte créé avec succès ! Connectez-vous.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur lors de l'inscription : " + ex.Message;
            }
        }

        private void ExecuteOpenSettings(object parameter) { new SettingsWindow().ShowDialog(); }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // CLASSE RELAYCOMMAND (Nécessaire pour les boutons)
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
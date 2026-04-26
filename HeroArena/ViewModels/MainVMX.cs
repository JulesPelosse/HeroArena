using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using HeroArena.Models;
using HeroArena.Data;
using HeroArena.Views;

namespace HeroArena.ViewModels
{
    public class MainVMX : INotifyPropertyChanged
    {
        // --- Propriétés privées ---
        private ObservableCollection<Hero> _heroesList;
        private ObservableCollection<Spell> _spellsList;
        private List<Spell> _masterSpellsList; // La sauvegarde pour le bouton "Tout afficher"
        private Hero _selectedHero;

        // --- Propriétés publiques ---
        public ObservableCollection<Hero> HeroesList
        {
            get => _heroesList;
            set { _heroesList = value; OnPropertyChanged(nameof(HeroesList)); }
        }

        public ObservableCollection<Spell> SpellsList
        {
            get => _spellsList;
            set { _spellsList = value; OnPropertyChanged(nameof(SpellsList)); }
        }

        public Hero SelectedHero
        {
            get => _selectedHero;
            set
            {
                _selectedHero = value;
                OnPropertyChanged(nameof(SelectedHero));
                // Notifie l'interface que l'état du bouton Commencer peut changer
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // --- Commandes ---
        public ICommand StartCombatCommand { get; }

        // --- Constructeur ---
        public MainVMX()
        {
            LoadData();
            StartCombatCommand = new RelayCommand(ExecuteStartCombat, _ => SelectedHero != null);
        }

        // --- Logique de chargement ---
        private void LoadData()
        {
            try
            {
                using (var context = new HeroArenaContext())
                {
                    // 1. Charger les héros et inclure les sorts via la table de liaison
                    var heroesFromDb = context.Heroes
                        .Include(h => h.HeroSpells)
                        .ThenInclude(hs => hs.Spell)
                        .ToList();

                    // Map la relation Many-to-Many manuellement pour faciliter l'accès
                    foreach (var hero in heroesFromDb)
                    {
                        hero.Spells = hero.HeroSpells.Select(hs => hs.Spell).ToList();
                    }

                    HeroesList = new ObservableCollection<Hero>(heroesFromDb);

                    // 2. Charger TOUS les sorts du jeu et créer la sauvegarde
                    _masterSpellsList = context.Spells.ToList();

                    // On initialise la liste affichée avec la sauvegarde
                    SpellsList = new ObservableCollection<Spell>(_masterSpellsList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur BDD lors du chargement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MÉTHODES DE FILTRE (Appelées par le code-behind) ---

        /// <summary>
        /// Réinitialise la liste des sorts avec la totalité de la base de données.
        /// </summary>
        public void RestoreAllSpells()
        {
            if (_masterSpellsList != null)
            {
                SpellsList = new ObservableCollection<Spell>(_masterSpellsList);
            }
        }

        /// <summary>
        /// Filtre la liste des sorts pour ne garder que ceux d'un héros spécifique.
        /// </summary>
        public void FilterSpellsByHero(Hero hero)
        {
            if (hero != null && hero.Spells != null)
            {
                SpellsList = new ObservableCollection<Spell>(hero.Spells);
            }
        }

        // --- Action de Combat ---
        private void ExecuteStartCombat(object parameter)
        {
            try
            {
                if (SelectedHero == null) return;

                // On passe le héros sélectionné et la liste complète pour générer les ennemis
                var combatVM = new CombatVMX(SelectedHero, HeroesList);
                CombatWindow combatWin = new CombatWindow();
                combatWin.DataContext = combatVM;
                combatWin.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du lancement du combat : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
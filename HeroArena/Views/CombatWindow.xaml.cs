using System.Windows;
using HeroArena.ViewModels;
using HeroArena.Models;

namespace HeroArena.Views
{
    public partial class CombatWindow : Window
    {
        public CombatWindow()
        {
            InitializeComponent();
        }

        // Bouton pour fermer l'arène
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Bouton pour attaquer
        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. On s'assure qu'on a bien accès au ViewModel
            if (this.DataContext is CombatVMX vm)
            {
                // 2. On récupère le sort sélectionné dans la ListBox (x:Name="SpellList")
                var selectedSpell = SpellList.SelectedItem as Spell;

                // 3. On déclenche l'attaque manuellement
                vm.ExecuteSpell(selectedSpell);
            }
        }
    }
}
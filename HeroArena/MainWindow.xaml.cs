using System.Windows;
using System.Windows.Controls;
using HeroArena.ViewModels;
using HeroArena.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace HeroArena
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HeroFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainVMX vm && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is Hero hero)
                {
                    vm.SpellsList = new ObservableCollection<Spell>(hero.Spells);
                }
            }
        }


        private void ShowAllSpells_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainVMX vm)
            {

                vm.RestoreAllSpells();


            }
        }
    }
}
using HeroArena.Models;
using System.Collections.ObjectModel;

namespace HeroArena  // ⚠️ AJOUTEZ CE NAMESPACE !
{
    public class DataStore
    {
        public static ObservableCollection<Hero> AllHeroes { get; set; }

        static DataStore()
        {
            AllHeroes = new ObservableCollection<Hero>();

            // Données de test
            AllHeroes.Add(new Hero { ID = 1, Name = "Guerrier", Health = 120, ImageURL = "" });
            AllHeroes.Add(new Hero { ID = 2, Name = "Mage", Health = 80, ImageURL = "" });
            AllHeroes.Add(new Hero { ID = 3, Name = "Assassin", Health = 90, ImageURL = "" });
        }
    }
}
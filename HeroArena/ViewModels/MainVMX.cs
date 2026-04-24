using System.Collections.ObjectModel;

namespace HeroArena.ViewModels
{
    public class MainVMX
    {
        // Propriété obligatoire selon les consignes
        public bool DebugFlag { get; set; }

        // Liste des héros pour la ListBox
        public ObservableCollection<Hero> HeroesList { get; set; }

        public MainVMX()
        {
            // Initialisation
            HeroesList = new ObservableCollection<Hero>();

            // Données de test
            HeroesList.Add(new Hero { Name = "Guerrier", Health = 100 });
            HeroesList.Add(new Hero { Name = "Mage", Health = 80 });
            HeroesList.Add(new Hero { Name = "Assassin", Health = 70 });
        }
    }

    // Classe Hero temporaire (à remplacer par votre modèle EF)
    public class Hero
    {
        public string Name { get; set; }
        public int Health { get; set; }
    }
}
namespace HeroArena.Models
{
    public class PlayerHero
    {
        public int PlayerID { get; set; }
        public int HeroID { get; set; }

        public Player Player { get; set; }
        public Hero Hero { get; set; }
    }
}
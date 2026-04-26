namespace HeroArena.Models
{
    public class HeroSpell
    {
        public int HeroID { get; set; }
        public int SpellID { get; set; }

        public Hero Hero { get; set; }
        public Spell Spell { get; set; }
    }
}
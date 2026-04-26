namespace HeroArena.Models
{
    public class Player
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? LoginID { get; set; }


        public Login Login { get; set; }
    }
}
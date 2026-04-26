using HeroArena.Models;
using Microsoft.EntityFrameworkCore;

namespace HeroArena.Data
{
    public class HeroArenaContext : DbContext
    {
        public DbSet<Login> Logins { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Spell> Spells { get; set; }
        public DbSet<HeroSpell> HeroSpells { get; set; }
        public DbSet<PlayerHero> PlayerHeroes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Properties.Settings.Default.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ExerciceHero;Trusted_Connection=True;TrustServerCertificate=True;";
            }
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().ToTable("Login");
            modelBuilder.Entity<Hero>().ToTable("Hero").HasKey(h => h.ID);
            modelBuilder.Entity<Spell>().ToTable("Spell").HasKey(s => s.ID);

            modelBuilder.Entity<Player>(entity => {
                entity.ToTable("Player");
                entity.Property(e => e.LoginID).HasColumnName("LoginID");
            });

            modelBuilder.Entity<HeroSpell>(entity => {
                entity.ToTable("HeroSpell");
                entity.HasKey(hs => new { hs.HeroID, hs.SpellID });
                entity.Property(hs => hs.HeroID).HasColumnName("HeroID");
                entity.Property(hs => hs.SpellID).HasColumnName("SpellID");
            });

            modelBuilder.Entity<PlayerHero>(entity => {
                entity.ToTable("PlayerHero");
                entity.HasKey(ph => new { ph.PlayerID, ph.HeroID });
                entity.Property(ph => ph.PlayerID).HasColumnName("PlayerID");
                entity.Property(ph => ph.HeroID).HasColumnName("HeroID");
            });

            // On ignore la liste automatique pour éviter les erreurs de colonnes fantômes
            modelBuilder.Entity<Hero>().Ignore(h => h.Spells);
        }
    }
}
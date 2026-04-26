using Microsoft.Data.SqlClient; 
using System.Data;             
using HeroArena.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace HeroArena
{
    public class DataStore
    {
        
        private static string connString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ExerciceHero;Integrated Security=True";

        public static ObservableCollection<Hero> GetHeroesFromDb()
        {
            var heroes = new ObservableCollection<Hero>();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                // Cette requête récupère les héros
                string sql = "SELECT ID, Name, Health, ImageURL FROM Hero";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var h = new Hero
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Health = reader.GetInt32(2),
                            ImageURL = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Spells = new List<Spell>() // On prépare la liste de sorts
                        };
                        heroes.Add(h);
                    }
                }

                // Pour chaque héros, on va chercher ses sorts via la table de liaison HeroSpell
                foreach (var hero in heroes)
                {
                    string spellSql = @"SELECT s.ID, s.Name, s.Damage, s.Description 
                                        FROM Spell s 
                                        INNER JOIN HeroSpell hs ON s.ID = hs.SpellID 
                                        WHERE hs.HeroID = @heroId";

                    using (SqlCommand cmd = new SqlCommand(spellSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@heroId", hero.ID);
                        using (SqlDataReader sReader = cmd.ExecuteReader())
                        {
                            while (sReader.Read())
                            {
                                hero.Spells.Add(new Spell
                                {
                                    ID = sReader.GetInt32(0),
                                    Name = sReader.GetString(1),
                                    Damage = sReader.GetInt32(2),
                                    Description = sReader.IsDBNull(3) ? "" : sReader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            return heroes;
        }
    }
}
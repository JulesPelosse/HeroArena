using HeroArena.Data;
using HeroArena.Models;
using HeroArena.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HeroArena.Services
{
    public class DatabaseService
    {
        public static void InitializeDatabase()
        {
            using (var context = new HeroArenaContext())
            {
                // Création de la base si elle n'existe pas
                context.Database.EnsureCreated();

                // Si déjà des données, on ne fait rien pour éviter les doublons
                if (context.Heroes.Any())
                    return;

                // === 1. CRÉATION DES SORTS DE BASE ===
                var slash = new Spell { Name = "Coup d'épée", Damage = 20, Description = "Une attaque rapide" };
                var shield = new Spell { Name = "Bouclier", Damage = 0, Description = "Se protège 1 tour" };
                var fireball = new Spell { Name = "Boule de feu", Damage = 30, Description = "Brûle l'ennemi" };
                var backstab = new Spell { Name = "Coup de poignard", Damage = 25, Description = "Attaque sournoise" };

                // === 2. CRÉATION DES SORTS LÉGENDAIRES ===
                // Sorts partagés (Jules & Ilian)
                var festin = new Spell { Name = "Délicieux Festin", Damage = 0, Description = "Dévore l'ennemi et vole ses PV/Sorts." };
                var bourlet = new Spell { Name = "Bourlet Protecteur", Damage = 0, Description = "Renvoie les dégâts reçus (Rebond)." };

                // Sorts spécifiques Jules
                var cyber = new Spell { Name = "Cyber Attaque", Damage = 50, Description = "Hacke l'ennemi (Stun 2 tours)." };
                var conso = new Spell { Name = "Consommation", Damage = 0, Description = "Récupère 200 PV." };

                // Sorts spécifiques Ilian
                var peda = new Spell { Name = "Tiens tête à la péda", Damage = 0, Description = "Stun l'ennemi pendant 5 tours." };
                var licence = new Spell { Name = "Licence Informatique", Damage = 9999, Description = "Endort la victime à jamais (K.O.)." };

                // Sorts Lloyd D. Saloum
                var barriere = new Spell { Name = "Barrière absolue", Damage = 0, Description = "Immunité totale (5 tours)." };
                var fracture = new Spell { Name = "Fracture du monde", Damage = 500, Description = "Dégâts apocalyptiques." };
                var trouNoir = new Spell { Name = "Trou noir", Damage = 300, Description = "Aspire l'ennemi (Dégâts continus)." };

                context.Spells.AddRange(slash, shield, fireball, backstab, festin, bourlet, cyber, conso, peda, licence, barriere, fracture, trouNoir);
                context.SaveChanges();

                // === 3. CRÉATION DES HÉROS ===
                var warrior = new Hero { Name = "Guerrier", Health = 120, ImageURL = "" };
                var mage = new Hero { Name = "Mage", Health = 80, ImageURL = "" };
                var assassin = new Hero { Name = "Assassin", Health = 90, ImageURL = "" };

                var lloyd = new Hero { Name = "Lloyd D. Saloum", Health = 250, ImageURL = "" };
                var jules = new Hero { Name = "Jules Pelosse", Health = 300, ImageURL = "" };
                var ilian = new Hero { Name = "Ilian PasDeGras", Health = 300, ImageURL = "" };

                context.Heroes.AddRange(warrior, mage, assassin, lloyd, jules, ilian);
                context.SaveChanges();

                // === 4. ASSOCIATION HÉROS-SORTS (Relation Many-to-Many) ===
                var links = new List<HeroSpell>();

                // Guerrier
                links.Add(new HeroSpell { HeroID = warrior.ID, SpellID = slash.ID });
                links.Add(new HeroSpell { HeroID = warrior.ID, SpellID = shield.ID });

                // Jules Pelosse
                links.Add(new HeroSpell { HeroID = jules.ID, SpellID = festin.ID });
                links.Add(new HeroSpell { HeroID = jules.ID, SpellID = bourlet.ID });
                links.Add(new HeroSpell { HeroID = jules.ID, SpellID = cyber.ID });
                links.Add(new HeroSpell { HeroID = jules.ID, SpellID = conso.ID });

                // Ilian PasDeGras
                links.Add(new HeroSpell { HeroID = ilian.ID, SpellID = festin.ID }); // Partagé
                links.Add(new HeroSpell { HeroID = ilian.ID, SpellID = bourlet.ID }); // Partagé
                links.Add(new HeroSpell { HeroID = ilian.ID, SpellID = peda.ID });
                links.Add(new HeroSpell { HeroID = ilian.ID, SpellID = licence.ID });

                // Lloyd D. Saloum
                links.Add(new HeroSpell { HeroID = lloyd.ID, SpellID = barriere.ID });
                links.Add(new HeroSpell { HeroID = lloyd.ID, SpellID = fracture.ID });
                links.Add(new HeroSpell { HeroID = lloyd.ID, SpellID = trouNoir.ID });

                context.HeroSpells.AddRange(links);
                context.SaveChanges();

                // === 5. CRÉATION DES UTILISATEURS ===
                // Compte Admin
                var adminLogin = new Login { Username = "admin", PasswordHash = PasswordHelper.HashPassword("admin123") };
                // Ton compte perso (Hash de "Jules")
                var julesLogin = new Login { Username = "jules", PasswordHash = PasswordHelper.HashPassword("Jules") };

                context.Logins.AddRange(adminLogin, julesLogin);
                context.SaveChanges();

                context.Players.Add(new Player { Name = "Admin Player", LoginID = adminLogin.ID });
                context.Players.Add(new Player { Name = "Jules Player", LoginID = julesLogin.ID });
                context.SaveChanges();
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using HeroArena.Models;

namespace HeroArena.ViewModels
{
    public class CombatVMX : INotifyPropertyChanged
    {
        // --- Propriétés de base ---
        private Hero _playerHero;
        private Hero _enemyHero;
        private int _playerCurrentHP;
        private int _enemyCurrentHP;
        private int _score;
        private string _combatLog;
        private bool _isCombatOver;
        private ObservableCollection<Hero> _allHeroes;
        private Random _random = new Random();

        // --- Gestion dynamique des sorts (Pour Jules et Ilian) ---
        private ObservableCollection<Spell> _playerSpells;
        public ObservableCollection<Spell> PlayerSpells
        {
            get => _playerSpells;
            set { _playerSpells = value; OnPropertyChanged(nameof(PlayerSpells)); }
        }

        // --- Variables d'état du Joueur ---
        private bool _playerIsPoisoned;
        private int _playerBurnDuration;
        private int _playerShield;         // Valeur d'absorption (ex: 9999 pour immunité)
        private int _playerShieldDuration; // Nombre de tours restants pour le bouclier
        private bool _isReflecting;        // État du "Bourlet Protecteur"
        private bool _hasReborn;           // Flag pour la renaissance de Lloyd

        // --- Variables d'état de l'Ennemi ---
        private bool _enemyIsPoisoned;
        private int _enemyBurnDuration;
        private int _enemyStunDuration;      // Pour Cyber Attaque et Péda
        private int _enemyBlackHoleDuration; // Pour Trou Noir de Lloyd

        #region Getters / Setters Standard
        public Hero PlayerHero { get => _playerHero; set { _playerHero = value; OnPropertyChanged(nameof(PlayerHero)); } }
        public Hero EnemyHero { get => _enemyHero; set { _enemyHero = value; OnPropertyChanged(nameof(EnemyHero)); } }
        public int PlayerCurrentHP
        {
            get => _playerCurrentHP;
            set { _playerCurrentHP = value; OnPropertyChanged(nameof(PlayerCurrentHP)); OnPropertyChanged(nameof(PlayerHPPercentage)); }
        }
        public int EnemyCurrentHP
        {
            get => _enemyCurrentHP;
            set { _enemyCurrentHP = value; OnPropertyChanged(nameof(EnemyCurrentHP)); OnPropertyChanged(nameof(EnemyHPPercentage)); }
        }
        public double PlayerHPPercentage => (PlayerHero == null || PlayerHero.Health <= 0) ? 0 : Math.Max(0, Math.Min(100, (double)PlayerCurrentHP / PlayerHero.Health * 100));
        public double EnemyHPPercentage => (EnemyHero == null || EnemyHero.Health <= 0) ? 0 : Math.Max(0, Math.Min(100, (double)EnemyCurrentHP / EnemyHero.Health * 100));
        public int Score { get => _score; set { _score = value; OnPropertyChanged(nameof(Score)); } }
        public string CombatLog { get => _combatLog; set { _combatLog = value; OnPropertyChanged(nameof(CombatLog)); } }
        public bool IsCombatOver { get => _isCombatOver; set { _isCombatOver = value; OnPropertyChanged(nameof(IsCombatOver)); } }
        #endregion

        public CombatVMX(Hero selectedHero, ObservableCollection<Hero> allHeroes)
        {
            _allHeroes = allHeroes ?? new ObservableCollection<Hero>();
            PlayerHero = CloneHero(selectedHero);

            // Initialisation de la collection de sorts pour l'UI
            PlayerSpells = new ObservableCollection<Spell>(PlayerHero.Spells);

            PlayerCurrentHP = PlayerHero.Health;
            _score = 0;
            _hasReborn = false;
            CombatLog = $"⚔️ Le combat commence ! {PlayerHero.Name} se prépare.\n";

            GenerateNextEnemy();
        }

        private void GenerateNextEnemy()
        {
            // Réinitialisation des états ennemis pour le nouveau combat
            _enemyIsPoisoned = false;
            _enemyBurnDuration = 0;
            _enemyStunDuration = 0;
            _enemyBlackHoleDuration = 0;

            var randomTemplate = _allHeroes[_random.Next(_allHeroes.Count)];
            EnemyHero = CloneHero(randomTemplate);

            // Difficulté croissante
            double multiplier = 1.0 + (Score * 0.15);
            EnemyHero.Health = (int)(EnemyHero.Health * multiplier);
            EnemyCurrentHP = EnemyHero.Health;

            CombatLog += $"👿 Un nouveau challenger apparaît : {EnemyHero.Name} (HP: {EnemyCurrentHP}) !\n";
        }

        /// <summary>
        /// Point d'entrée principal déclenché par le clic sur un sort.
        /// </summary>
        public void ExecuteSpell(Spell spell)
        {
            if (IsCombatOver || spell == null) return;

            CombatLog += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
            _isReflecting = false; // Reset du rebond au début de chaque action joueur

            // --- 1. ACTION DU JOUEUR ---
            PlayerTurnAction(spell);

            // Vérification immédiate de la mort de l'ennemi
            if (CheckEnemyDeath()) return;

            // --- 2. ACTION DE L'ENNEMI ---
            // On vérifie si l'ennemi est paralysé (Stun) ou gelé (Ice)
            bool isFrozen = spell.Name.ToLower().Contains("glace") || spell.Name.ToLower().Contains("gel");

            if (_enemyStunDuration > 0)
            {
                CombatLog += $"🚫 {EnemyHero.Name} est étourdi et passe son tour ! ({_enemyStunDuration} tours restants)\n";
                _enemyStunDuration--;
            }
            else if (isFrozen)
            {
                CombatLog += $"❄️ {EnemyHero.Name} est gelé et ne peut pas contre-attaquer ce tour-ci !\n";
            }
            else
            {
                EnemyTurnAction();
            }

            // --- 3. PHASE DE FIN DE TOUR (DoT et décompte des Buffs) ---
            ApplyEndOfRoundEffects();
        }

        private void PlayerTurnAction(Spell spell)
        {
            string spellName = spell.Name.ToLower();
            string heroName = PlayerHero.Name.ToLower();
            int baseDamage = Math.Abs(spell.Damage);

            // --- LOGIQUE : JULES PELOSSE & ILIAN PASDEGRAS ---
            if (spellName.Contains("festin"))
            {
                CombatLog += $"🍔 DÉLICIEUX FESTIN ! Vous dévorez {EnemyHero.Name} !\n";
                // Gain de PV basé sur la vie actuelle de l'ennemi (Cap à 500)
                PlayerCurrentHP = Math.Min(500, PlayerCurrentHP + EnemyCurrentHP);

                // Vol de sort
                if (EnemyHero.Spells != null && EnemyHero.Spells.Count > 0)
                {
                    var stolen = EnemyHero.Spells[_random.Next(EnemyHero.Spells.Count)];
                    PlayerSpells.Add(new Spell { Name = "[VOLÉ] " + stolen.Name, Damage = stolen.Damage, Description = stolen.Description });
                    CombatLog += $"💡 Compétence assimilée : {stolen.Name} !\n";
                }
                EnemyCurrentHP = 0; // Mort instantanée
                return;
            }
            if (spellName.Contains("bourlet"))
            {
                _isReflecting = true;
                CombatLog += "🛡️ BOURLET RÉFLÉCHISSANT : L'ennemi va se frapper tout seul ! CHEH !\n";
            }
            if (spellName.Contains("cyber"))
            {
                EnemyCurrentHP -= 50;
                _enemyStunDuration = 2;
                CombatLog += "💻 CYBER ATTAQUE : -50 HP et système ennemi verrouillé (Stun 2 tours) !\n";
            }
            if (spellName.Contains("peda"))
            {
                _enemyStunDuration = 5;
                CombatLog += "😤 TIENS TÊTE À LA PÉDA : L'ennemi est pétrifié par votre audace (Stun 5 tours) !\n";
            }
            if (spellName.Contains("licence"))
            {
                EnemyCurrentHP = 0;
                CombatLog += "🎓 LICENCE INFORMATIQUE : L'ennemi s'endort pour l'éternité ! L'ennemi a succombé au tunnel du prof 😴\n";
                return;
            }
            if (spellName.Contains("consomation"))
            {
                PlayerCurrentHP = Math.Min(PlayerHero.Health, PlayerCurrentHP + 200);
                CombatLog += "🍕 CONSOMMATION INSTANTANÉE : Vous récupérez 200 PV. Et perdez 50kg. \n";
            }

            // --- LOGIQUE : LLOYD D. SALOUM ---
            if (spellName.Contains("barrière"))
            {
                _playerShield = 9999;
                _playerShieldDuration = 5;
                CombatLog += "💠 BARRIÈRE ABSOLUE DE MANA : Immunité totale activée pour 5 tours !\n";
            }
            if (spellName.Contains("fracture"))
            {
                EnemyCurrentHP -= 500;
                CombatLog += "💥 FRACTURE DU MONDE : Dégâts apocalyptiques de 500 HP (Ignore tout) !\n";
            }
            if (spellName.Contains("trou noir"))
            {
                _enemyBlackHoleDuration = 2;
                EnemyCurrentHP -= 300; // Premier impact
                CombatLog += "🌑 TROU NOIR : L'ennemi est aspiré ! -300 HP (Dégâts continus activés).\n";
            }

            // --- LOGIQUE : CLASSES DE BASE ---

            // MAGE : Bulle
            if (spellName.Contains("bulle"))
            {
                PlayerCurrentHP = Math.Min(PlayerHero.Health, PlayerCurrentHP + 10);
                _playerIsPoisoned = false;
                _playerBurnDuration = 0;
                _playerShield = 9999;
                _playerShieldDuration = 1; // La bulle du mage dure 1 tour complet
                CombatLog += "🫧 BULLE PROTECTRICE : Soin, Purge et Immunité active !\n";
            }
            // ASSASSIN : Exécution
            else if (spellName.Contains("exécution"))
            {
                double lifeRatio = (double)EnemyCurrentHP / EnemyHero.Health;
                if (lifeRatio < 0.20)
                {
                    EnemyCurrentHP = 0;
                    CombatLog += "☠️ EXÉCUTION : L'ennemi est achevé proprement !\n";
                }
                else CombatLog += "⚖️ L'ennemi a encore trop de vie pour être exécuté.\n";
            }
            // PALADIN & SOIN
            else if (spellName.Contains("soin") || spellName.Contains("heal") || spellName.Contains("guérison"))
            {
                PlayerCurrentHP = Math.Min(PlayerHero.Health, PlayerCurrentHP + baseDamage);
                CombatLog += $"✨ Vous utilisez {spell.Name} : +{baseDamage} HP.\n";

                if (heroName.Contains("paladin"))
                {
                    _playerIsPoisoned = false;
                    _playerBurnDuration = 0;
                    CombatLog += "🌟 Grâce du Paladin : Vos malus sont dissipés !\n";
                }
            }
            // BOUCLIERS (Guerrier / Standard)
            else if (spellName.Contains("bouclier") || spellName.Contains("protection"))
            {
                if (heroName.Contains("guerrier")) _playerShield = 30;
                else _playerShield = 20;

                _playerShieldDuration = 1;
                EnemyCurrentHP -= baseDamage; // Si le bouclier fait des dégâts (ex: Coup de bouclier)
                CombatLog += $"🛡️ Posture défensive activée ! Dégâts réduits au prochain tour.\n";
            }
            // ATTAQUES STANDARDS (Feu, Poison, etc.)
            else
            {
                EnemyCurrentHP -= baseDamage;
                CombatLog += $"⚔️ Vous utilisez {spell.Name} : -{baseDamage} HP.\n";

                if (spellName.Contains("feu") || spellName.Contains("flamme"))
                {
                    _enemyBurnDuration = 1;
                    CombatLog += "🔥 L'ennemi brûle !\n";
                }
                if (spellName.Contains("poison") || spellName.Contains("venin"))
                {
                    _enemyIsPoisoned = true;
                    CombatLog += "☠️ L'ennemi est empoisonné !\n";
                }
            }
        }

        private void EnemyTurnAction()
        {
            if (EnemyHero.Spells == null || EnemyHero.Spells.Count == 0) return;

            var spell = EnemyHero.Spells[_random.Next(EnemyHero.Spells.Count)];
            int rawDamage = Math.Abs(spell.Damage);
            string name = spell.Name.ToLower();

            // --- GESTION DU REBOND (Bourlet) ---
            if (_isReflecting)
            {
                EnemyCurrentHP -= rawDamage;
                CombatLog += $"反射 REBOND ! {EnemyHero.Name} se blesse lui-même avec {spell.Name} (-{rawDamage} HP) !\n";
                return;
            }

            // --- GESTION DU SOIN ENNEMI ---
            if (name.Contains("soin") || name.Contains("heal"))
            {
                EnemyCurrentHP = Math.Min(EnemyHero.Health, EnemyCurrentHP + rawDamage);
                CombatLog += $"✨ {EnemyHero.Name} se soigne : +{rawDamage} HP.\n";
                return;
            }

            // --- GESTION DES DÉGÂTS REÇUS PAR LE JOUEUR ---
            int finalDamage = rawDamage;

            if (_playerShield > 0)
            {
                int absorbed = Math.Min(finalDamage, _playerShield);
                finalDamage -= absorbed;

                if (_playerShield >= 9999) CombatLog += "💠 L'attaque s'écrase contre votre Barrière !\n";
                else CombatLog += $"🛡️ Votre bouclier absorbe {absorbed} dégâts.\n";

                // Le bouclier classique (durée 1 ou moins) se brise après l'impact
                if (_playerShieldDuration <= 1) _playerShield = 0;
            }

            if (finalDamage > 0)
            {
                PlayerCurrentHP -= finalDamage;
                CombatLog += $"💥 {EnemyHero.Name} attaque avec {spell.Name} : -{finalDamage} HP.\n";

                // Malus infligés par l'ennemi
                if (name.Contains("feu")) { _playerBurnDuration = 1; CombatLog += "🔥 Vous brûlez !\n"; }
                if (name.Contains("poison")) { _playerIsPoisoned = true; CombatLog += "☠️ Vous êtes empoisonné !\n"; }
            }

            // --- VÉRIFICATION MORT JOUEUR + RENAISSANCE LLOYD ---
            if (PlayerCurrentHP <= 0)
            {
                if (PlayerHero.Name.Contains("Lloyd") && !_hasReborn)
                {
                    _hasReborn = true;
                    PlayerCurrentHP = (int)(PlayerHero.Health * 1.5);
                    CombatLog += "🧬 RENAISSANCE PAR LE MANA ! Lloyd refuse la défaite et revient avec 150% de PV !\n";
                }
                else
                {
                    ExecuteGameOver();
                }
            }
        }

        private void ApplyEndOfRoundEffects()
        {
            if (IsCombatOver) return;

            // --- Dégâts continus Ennemi ---
            if (_enemyIsPoisoned) { EnemyCurrentHP -= 10; CombatLog += "☠️ Le poison ronge l'ennemi (-10 HP).\n"; }
            if (_enemyBurnDuration > 0) { EnemyCurrentHP -= 10; _enemyBurnDuration--; CombatLog += "🔥 La brûlure inflige 10 dégâts à l'ennemi.\n"; }
            if (_enemyBlackHoleDuration > 0) { EnemyCurrentHP -= 300; _enemyBlackHoleDuration--; CombatLog += "🌑 L'attraction du Trou Noir inflige 300 dégâts !\n"; }

            // --- Dégâts continus Joueur ---
            if (_playerIsPoisoned) { PlayerCurrentHP -= 10; CombatLog += "☠️ Le poison vous inflige 10 dégâts.\n"; }
            if (_playerBurnDuration > 0) { PlayerCurrentHP -= 10; _playerBurnDuration--; CombatLog += "🔥 La brûlure vous inflige 10 dégâts.\n"; }

            // --- Décompte des Buffs / Protections ---
            if (_playerShieldDuration > 0)
            {
                _playerShieldDuration--;
                if (_playerShieldDuration == 0)
                {
                    _playerShield = 0;
                    CombatLog += "🛡️ Votre protection magique se dissipe.\n";
                }
            }

            // Vérifications finales de santé
            CheckEnemyDeath();
            if (PlayerCurrentHP <= 0) ExecuteGameOver();
        }

        private bool CheckEnemyDeath()
        {
            if (EnemyCurrentHP <= 0)
            {
                EnemyCurrentHP = 0;
                Score++;
                CombatLog += $"🏆 VICTOIRE ! {EnemyHero.Name} a été vaincu. Score : {Score}\n";
                GenerateNextEnemy();
                return true;
            }
            return false;
        }

        private void ExecuteGameOver()
        {
            PlayerCurrentHP = 0;
            IsCombatOver = true;

            // On nettoie tout pour le prochain !
            _enemyStunDuration = 0;
            _playerShield = 0;
            _isReflecting = false;

            CombatLog += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
            CombatLog += $"💀 GAME OVER ! Score : {Score}.\n";
        }

        private Hero CloneHero(Hero original)
        {
            if (original == null) return null;
            var clone = new Hero
            {
                ID = original.ID,
                Name = original.Name,
                Health = original.Health,
                ImageURL = original.ImageURL,
                Spells = new List<Spell>()
            };
            foreach (var s in original.Spells)
                clone.Spells.Add(new Spell { Name = s.Name, Damage = s.Damage, Description = s.Description });

            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
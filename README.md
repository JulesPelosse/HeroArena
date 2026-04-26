# 🛡️ Hero Arena - Combat de Légendes

**Hero Arena** est une application de combat au tour par tour développée en **C# / WPF** suivant le patron de conception **MVVM**. Affrontez des vagues d'ennemis, gérez vos sorts et grimpez dans le classement avec des héros.

---

## 🚀 Fonctionnalités principales

* **Système de Combat dynamique** : Gestion des points de vie, des barres de progression et des logs de combat détaillés.
* **Héros Légendaires (Custom)** :
    * **Jules Pelosse** : Capable de dévorer ses ennemis pour voler leurs compétences grâce à l'assimilation.
    * **Ilian PasDeGras** : Le maître du stun ("Tiens tête à la péda") et du K.O. instantané ("Licence Informatique").
    * **Lloyd D. Saloum** : Un boss redoutable avec barrière de mana de 5 tours et mécanique de renaissance.
* **Gestion des Sorts** : Système de tri par héros et affichage détaillé (Nom, Dégâts, Description).
* **Sécurité** : Authentification des utilisateurs avec **hachage des mots de passe** (SHA256) en base de données.
* **Paramètres avancés** : Configuration dynamique de la chaîne de connexion (Connection String) et outil d'initialisation de la base.

---

## 🏗️ Architecture du Projet (MVVM)

Le projet respecte scrupuleusement la séparation des responsabilités. Les ViewModels utilisent tous le suffixe obligatoire `VMX` :

* 📂 **Models** : Entités liées à la base de données (`Hero`, `Spell`, `Login`, `Player`, `HeroSpell`).
* 📂 **ViewModels** : Logique métier (`MainVMX`, `CombatVMX`, `LoginVMX`, `SettingsVMX`).
* 📂 **Views** : Interfaces utilisateur XAML (`MainWindow`, `CombatWindow`, `LoginWindow`, `SettingsWindow`).
* 📂 **Data** : Contexte Entity Framework (`HeroArenaContext`).
* 📂 **Helpers & Converters** : Utilitaires de commandes (`RelayCommand`), hachage (`PasswordHelpers`) et convertisseurs XAML (`NullToVisibilityConverter`).
* 📂 **Services** : Logiques de connexion et d'accès aux données (`DataBaseService`).

---

## 🛠️ Installation et Initialisation

### 1. Base de données
Le projet nécessite une instance **SQL Server**.
1.  Ouvrez SQL Server Management Studio (SSMS).
2.  Exécutez le script `init_db.sql` situé à la racine du projet pour créer la structure et injecter les héros/sorts.
    * *Note : Le compte par défaut est `UserTest` avec le mot de passe `1234`.*

### 2. Configuration de l'application
1.  Lancez la solution `HeroArena.sln` dans Visual Studio.
2.  Allez dans la fenêtre **Settings** pour configurer votre chaîne de connexion.
3.  Utilisez le bouton **Initialiser la base** pour vérifier la connectivité et la présence des données.

---

## 📦 Dépendances et Packages

Le projet utilise les bibliothèques suivantes :
* `Microsoft.EntityFrameworkCore` : Mapping Objet-Relationnel (ORM).
* `Microsoft.EntityFrameworkCore.SqlServer` : Support de SQL Server.
* `Microsoft.EntityFrameworkCore.Proxies` : Activation du Lazy Loading pour les relations entre héros et sorts.

---

## 📝 Notes de développement (Cas exceptionnels)

Conformément aux consignes, les mécaniques suivantes ont été implémentées :
* **Optimisation DB** : Utilisation d'une table de liaison `HeroSpell` (Many-to-Many) pour partager des sorts communs entre Jules et Ilian sans duplication dans la table `Spell`.
* **Mécanique de Vol** : Jules Pelosse et Illian peuvent assimiler des sorts ennemis en plein combat, mis à jour via une `ObservableCollection` pour éviter tout crash d'interface.
* **États de statut** : Implémentation complexe des malus (Poison, Brûlure, Gel) et des bonus (Réflexion de dégâts, Immunité temporaire).

---

**Développé par :** Jules Pelosse 🎓  
**Projet :** Hero Arena - 2026

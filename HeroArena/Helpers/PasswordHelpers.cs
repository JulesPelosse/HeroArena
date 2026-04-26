using System;
using System.Security.Cryptography;
using System.Text;

namespace HeroArena.Helpers
{
    public static class PasswordHelper
    {

        /// Hash un mot de passe avec SHA256

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        /// Vérifie si le mot de passe correspond au hash

        public static bool VerifyPassword(string password, string hash)
        {
            string hashedInput = HashPassword(password);
            return hashedInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
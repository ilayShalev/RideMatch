using System;
using System.Security.Cryptography;
using System.Text;

namespace RideMatch.Utilities.Security
{
    public static class PasswordHasher
    {
        // Hashes a password
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Generate a random salt
            string salt = GenerateSalt();

            // Combine password and salt
            string saltedPassword = password + salt;

            // Compute SHA256 hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                // Convert to base64 string
                string hashString = Convert.ToBase64String(hashBytes);

                // Format: hash:salt
                return $"{hashString}:{salt}";
            }
        }

        // Verifies a password against a hash
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (string.IsNullOrEmpty(hash))
                return false;

            // Split hash and salt
            string[] parts = hash.Split(':');
            if (parts.Length != 2)
                return false;

            string storedHash = parts[0];
            string salt = parts[1];

            // Combine provided password with stored salt
            string saltedPassword = password + salt;

            // Compute hash of salted password
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                string computedHash = Convert.ToBase64String(hashBytes);

                // Compare computed hash with stored hash
                return storedHash == computedHash;
            }
        }

        // Generates a salt for password hashing
        private static string GenerateSalt()
        {
            // Generate 16 random bytes
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Convert to base64 string
            return Convert.ToBase64String(saltBytes);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Utilities.Security
{
    public static class PasswordHasher
    {
        // Hashes a password
        public static string HashPassword(string password);

        // Verifies a password against a hash
        public static bool VerifyPassword(string password, string hash);

        // Generates a salt for password hashing
        private static string GenerateSalt();
    }
}

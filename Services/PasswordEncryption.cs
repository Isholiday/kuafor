using System.Security.Cryptography;
using backend.Models;

namespace backend.Services {
    public class PasswordEncryption {
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static User HashPassword(RegisterViewModel model) {
            using var pbkdf2 = new Rfc2898DeriveBytes(model.Password!, [], Iterations, HashAlgorithm);
            var user = new User {
                Email = model.Email,
                Username = model.Username,
                Password = Convert.ToBase64String(pbkdf2.GetBytes(KeySize)),
            };
            return user;
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash) {
            using var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, [], Iterations, HashAlgorithm);
            string enteredHash = Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
            return enteredHash == storedHash;
        }
    }
}

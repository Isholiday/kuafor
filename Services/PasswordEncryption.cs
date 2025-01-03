using System.Security.Cryptography;
using backend.Models;
using backend.ViewModels;

namespace backend.Services {
    public static class PasswordEncryption {
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static User HashPassword(RegisterViewModel model) {
            using var pbkdf2 = new Rfc2898DeriveBytes(model.Password!, [], Iterations, HashAlgorithm);
            return new User {
                Email = model.Email,
                Username = model.Username,
                Password = Convert.ToBase64String(pbkdf2.GetBytes(KeySize)),
            };
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash) {
            using var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, [], Iterations, HashAlgorithm);
            var enteredHash = Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
            return enteredHash == storedHash;
        }
    }
}

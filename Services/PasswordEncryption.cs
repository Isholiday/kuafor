using System.Security.Cryptography;
using backend.Models;

namespace backend.Services {
    public class PasswordEncryption {
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static User HashPassword(User model) {
            using var pbkdf2 = new Rfc2898DeriveBytes(model.Password!, [], Iterations, HashAlgorithm);
            model.Password = Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
            return model;
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash) {
            using var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, [], Iterations, HashAlgorithm);
            string enteredHash = Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
            return enteredHash == storedHash;
        }
    }
}

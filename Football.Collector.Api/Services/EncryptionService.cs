using Football.Collector.Interfaces;
using System;
using System.Security.Cryptography;

namespace Football.Collector.Services
{
    public class EncryptionService: IEncryptionService
    {
        public string GetPasswordHash(string password)
        {
            byte[] salt;

            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            var passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }

        public bool ValidatePassword(string clearPassword, string passwordHash)
        {
            if (string.IsNullOrEmpty(clearPassword) || string.IsNullOrEmpty(passwordHash))
            {
                return false;
            }

            byte[] hashBytes = Convert.FromBase64String(passwordHash);
            byte[] salt = new byte[16];

            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(clearPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            bool valid = true;

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    valid = false;
                }
            }

            return valid;
        }
    }
}

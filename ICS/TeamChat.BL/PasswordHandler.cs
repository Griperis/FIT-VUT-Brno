using System;
using System.Security.Cryptography;

namespace TeamChat.BL
{
    public class PasswordHandler
    {
        public string HashPassword(string password)
        {
            byte[] salt;

            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            var hash = pbkdf2.GetBytes(20);
            var hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);


            return Convert.ToBase64String(hashBytes);
        }

        public bool IsCorrectPassword(string userPassword, string databasePassword)
        {
            var salt = new byte[16];
            var hashBytes = Convert.FromBase64String(databasePassword);

            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(userPassword, salt, 10000);

            var hash = pbkdf2.GetBytes(20);
            var isOk = true;
            for (var i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    isOk = false;
                }
            }

            return isOk;
        }
    }
}
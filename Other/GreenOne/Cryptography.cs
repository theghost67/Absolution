using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GreenOne
{
    /// <summary>
    /// Статический класс, содержащий методы для шифрования и дешифрования строк.
    /// </summary>
    public static class Cryptography
    {
        #region Variables
        const int ITERATIONS = 2;
        const int KEY_SIZE = 256;

        const string HASH = "SHA1";
        const string SALT = "aselrias38490a32";
        const string VECTOR = "8947az34awl34kjq";

        static readonly byte[] _vectorBytes;
        static readonly byte[] _saltBytes;
        #endregion

        #region Functions
        static Cryptography()
        {
            _vectorBytes = Encoding.ASCII.GetBytes(VECTOR);
            _saltBytes = Encoding.ASCII.GetBytes(SALT);
        }

        public static string Encrypt(string value, string password)
        {
            return Encrypt<AesManaged>(value, password);
        }
        public static string Encrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] encrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes passwordBytes = new(password, _saltBytes, HASH, ITERATIONS);
                byte[] keyBytes = passwordBytes.GetBytes(KEY_SIZE / 8);

                cipher.Mode = CipherMode.CBC;

                using ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, _vectorBytes);
                using MemoryStream to = new();
                using CryptoStream writer = new(to, encryptor, CryptoStreamMode.Write);

                writer.Write(valueBytes, 0, valueBytes.Length);
                writer.FlushFinalBlock();
                encrypted = to.ToArray();
                cipher.Clear();
            }
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string value, string password)
        {
            return Decrypt<AesManaged>(value, password);
        }
        public static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = Convert.FromBase64String(value);
            byte[] decrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes passwordBytes = new PasswordDeriveBytes(password, _saltBytes, HASH, ITERATIONS);
                byte[] keyBytes = passwordBytes.GetBytes(KEY_SIZE / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, _vectorBytes);
                    using MemoryStream from = new MemoryStream(valueBytes);
                    using CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read);

                    decrypted = new byte[valueBytes.Length];
                    reader.Read(decrypted, 0, decrypted.Length);
                }
                catch (Exception)
                {
                    return string.Empty;
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted);
        }
        #endregion
    }
}


using System;
using System.Security.Cryptography;
using System.Text;

using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business
{
    public class MSSecurity
    {
        public static string GenerateSaltValue()
        {
            int i = 0;
            int code = 0;
            string salt = "";

            Random random = new Random(unchecked((int)DateTime.Now.Ticks));

            for (i = 1; i <= 10; i++)
            {
                code = random.Next(48, 122);
                salt = salt + ((char)code);
            }

            return salt;
        }


        public static string GenerateHashWithSalt(string password, string salt)
        {
            string sHashWithSalt = password.Trim() + salt;

            byte[] saltedHashBytes = Encoding.UTF8.GetBytes(sHashWithSalt);

            var key = "lkszdjhaswdahgdjhug";

            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));

            byte[] hash = hmac.ComputeHash(saltedHashBytes);

            return Convert.ToBase64String(hash);
        }


        private static byte[] GetPasswordBytes()
        {
            var key = "sadhgj6123hhdajdkqjnzqfjlka7Z23";

            var ba = Encoding.UTF8.GetBytes(key);

            return System.Security.Cryptography.SHA256.Create().ComputeHash(ba);
        }


        public static string EncryptData(string text)
        {
            return AES.Encrypt(text, GetPasswordBytes());
        }


        public static string DecryptString(string text)
        {
            return AES.Decrypt(text, GetPasswordBytes());
        }



    }
}


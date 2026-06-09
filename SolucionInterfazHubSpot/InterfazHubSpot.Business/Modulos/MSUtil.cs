
using System;
using System.Text;

using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business
{
    public static class MSUtil
    {

        public static string GetIdentif()
        {
            DateTime oNow = DateTime.Now;

            string strFechaHora = oNow.ToString("yyyyMMddHHmmss");

            string strTicks = oNow.Ticks.ToString();

            string strIdent = strFechaHora + strTicks;

            return strIdent;
        }


        public static string GetDownloadKey(string key)
        {
            var enc = EncryptData(key);

            var res = System.Web.HttpUtility.UrlEncode(enc);

            return res;
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



    }
}


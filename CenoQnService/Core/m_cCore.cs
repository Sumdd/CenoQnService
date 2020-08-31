using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CenoQnService
{
    public class m_cCore
    {
        #region ***加密解密
        public class Encrypt
        {
            // 创建Key
            public string GenerateKey()
            {
                DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();
                return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
            }

            private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            /// <summary>
            ///  加密字符串
            /// </summary>
            /// <param name="sInputString"></param>
            /// <param name="sKey"></param>
            /// <returns></returns>
            public static string EncryptString(string encryptString, string encryptKey)
            {
                try
                {
                    byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                    byte[] rgbIV = Keys;
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                    DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                    MemoryStream mStream = new MemoryStream();
                    CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    return Convert.ToBase64String(mStream.ToArray());
                }
                catch
                {
                    return encryptString;
                }
            }
            /// <summary>
            /// 加密字符串
            /// </summary>
            /// <param name="encryptString"></param>
            /// <returns></returns>
            public static string EncryptString(string encryptString)
            {
                try
                {
                    byte[] rgbKey = Encoding.UTF8.GetBytes("cenosoft");
                    byte[] rgbIV = Keys;
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                    DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                    MemoryStream mStream = new MemoryStream();
                    CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    return Convert.ToBase64String(mStream.ToArray());
                }
                catch
                {
                    return encryptString;
                }
            }

            /// <summary>
            /// 解密字符串
            /// </summary>
            /// <param name="sInputString"></param>
            /// <param name="sKey"></param>
            /// <returns></returns>
            public static string DecryptString(string decryptString, string decryptKey)
            {
                try
                {
                    byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey);
                    byte[] rgbIV = Keys;
                    byte[] inputByteArray = Convert.FromBase64String(decryptString);
                    DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                    MemoryStream mStream = new MemoryStream();
                    CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    return Encoding.UTF8.GetString(mStream.ToArray());
                }
                catch
                {
                    return decryptString;
                }
            }

            /// <summary>
            /// MD5加密
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string StrToMD5(string str)
            {
                byte[] data = Encoding.GetEncoding("GB2312").GetBytes(str);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] OutBytes = md5.ComputeHash(data);

                string OutString = "";
                for (int i = 0; i < OutBytes.Length; i++)
                {
                    OutString += OutBytes[i].ToString("x2");
                }
                return OutString.ToUpper();
            }
        }
        #endregion
    }
}
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CenoQnService
{
    public class m_cDigest
    {
        //SHA256
        public static string m_fSHA256(string data)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] SHA256Data = Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING).GetBytes(data);
                byte[] by = sha256.ComputeHash(SHA256Data);
                return Hex.ToHexString(by).ToUpper();
            }
        }
        //MD5
        public static string m_fMD5(string data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] MD5Data = Encoding.GetEncoding(m_cCfg.SYSTEM_ENCODING).GetBytes(data);
                byte[] by = md5.ComputeHash(MD5Data);
                return Hex.ToHexString(by).ToUpper();
            }
        }
    }
}
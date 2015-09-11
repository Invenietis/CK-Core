using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CodeCake
{
    /// <summary>
    /// This is the same algorithm and configuration as https://github.com/appveyor/secure-file.
    /// </summary>
    public static class SecureFile
    {
        private static string Salt = "{E4E66F59-CAF2-4C39-A7F8-46097B1C461B}";

        public static void Encrypt( string fileName, string outFileName, string secret )
        {
            var alg = GetRijndael( secret );

            using( var inStream = File.OpenRead( fileName ) )
            {
                using( var outStream = File.Create( outFileName ) )
                {
                    using( var cryptoStream = new CryptoStream( outStream, alg.CreateEncryptor(), CryptoStreamMode.Write ) )
                    {
                        inStream.CopyTo( cryptoStream );
                    }
                }
            }
        }

        public static void Decrypt( string fileName, string outFileName, string secret )
        {
            var alg = GetRijndael( secret );

            using( var inStream = File.OpenRead( fileName ) )
            {
                using( var outStream = File.Create( outFileName ) )
                {
                    using( var cryptoStream = new CryptoStream( outStream, alg.CreateDecryptor(), CryptoStreamMode.Write ) )
                    {
                        inStream.CopyTo( cryptoStream );
                    }
                }
            }
        }

        public static Rijndael GetRijndael( string secret )
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes( secret, Encoding.UTF8.GetBytes( Salt ), 10000 );

            Rijndael alg = Rijndael.Create();
            alg.Key = pbkdf2.GetBytes( 32 );
            alg.IV = pbkdf2.GetBytes( 16 );

            return alg;
        }
    }
}

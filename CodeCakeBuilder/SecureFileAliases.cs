using Cake.Core;
using Cake.Core.IO;
using CodeCake;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.Common
{
    public static class SecureFileAliases
    {

        public static TemporaryFile SecureFileUncrypt( this ICakeContext cake, FilePath file, string secret )
        {
            string extension = file.GetExtension();
            if( extension == "enc" ) extension = file.GetFilenameWithoutExtension().GetExtension();
            var f = new TemporaryFile( extension );
            SecureFile.Decrypt( file.FullPath, f.Path, secret );
            return f;
        }
        public static void SecureFileCrypt( this ICakeContext cake, FilePath file, FilePath encryptedFile, string secret )
        {
            SecureFile.Encrypt( file.FullPath, encryptedFile.FullPath, secret );
        }
    }
}

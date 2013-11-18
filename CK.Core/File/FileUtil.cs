#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\File\FileUtil.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Helper functions related to file system.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Combination of <see cref="FileAttributes"/> that can not exist: it can be used to 
        /// tag inexisting files among other existing (i.e. valid) file attributes.
        /// </summary>
        static readonly public FileAttributes InexistingFile = FileAttributes.Normal | FileAttributes.Offline;

        /// <summary>
        /// Returns files in a directory according to multiple file masks (separated by ';'). 
        /// </summary>
        /// <param name="path">Path of the directory to read.</param>
        /// <param name="multiFileMask">File masks, for example: <i>*.gif;*.jpg;*.png</i>.</param>
        /// <returns>List of files (without duplicates).</returns>
        static public string[] GetFiles( string path, string multiFileMask )
        {
            string[] m = multiFileMask.Split( ';' );
            if( m.Length > 1 )
            {
                List<string> l = new List<string>();
                foreach( string oneMask in m )
                {
                    if( oneMask.Length > 0 )
                    {
                        if( oneMask == "*" || oneMask == "*.*" )
                            return Directory.GetFiles( path );
                        string[] fs = Directory.GetFiles( path, oneMask );
                        foreach( string f in fs )
                        {
                            int pos = l.BinarySearch( f, StringComparer.InvariantCulture );
                            if( pos < 0 ) l.Insert( ~pos, f );
                        }
                    }
                }
                return l.ToArray();
            }
            if( m.Length > 0 && m[0] != "*" && m[0] != "*.*" )
                return Directory.GetFiles( path, m[0] );
            return Directory.GetFiles( path );
        }

        /// <summary>
        /// Canonicalizes the path: all '/' and '\' are mapped to <see cref="Path.DirectorySeparatorChar"/> 
        /// (and <see cref="Path.AltDirectorySeparatorChar"/> will also be transformed).
        /// </summary>
        /// <param name="path">The path to standardize (must be not be null). It is trimmed and if the path is empty, the empty string is returned.</param>
        /// <param name="ensureTrailingBackslash">
        /// Ensures that the normalized path will end with a <see cref="Path.DirectorySeparatorChar"/>.
        /// It should be true for path to directories because we consider that a directory path SHOULD end with 
        /// the slash as often as possible.
        /// When <paramref name="path"/> is empty, this is not applied to preserve the fact that the string is empty.
        /// </param>
        /// <returns>A standardized path, whatever the actual <c>Path.DirectorySeparatorChar</c> is
        /// on the current platform.</returns>
        static public string NormalizePathSeparator( string path, bool ensureTrailingBackslash )
        {
            if( path == null ) throw new ArgumentNullException( "path" );
            path = path.Trim();
            if( path.Length == 0 ) return path;
            if( Path.DirectorySeparatorChar != '/' && Path.AltDirectorySeparatorChar != '/' )
                path = path.Replace( '/', Path.DirectorySeparatorChar );
            if( Path.DirectorySeparatorChar != '\\' && Path.AltDirectorySeparatorChar != '\\' )
                path = path.Replace( '\\', Path.DirectorySeparatorChar );
            path = path.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
            if( ensureTrailingBackslash && path[path.Length - 1] != Path.DirectorySeparatorChar )
            {
                path += Path.DirectorySeparatorChar;   
            }
            return path;
        }

        static char[] _invalidPathChars = Path.GetInvalidPathChars();
        static char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Gets the <see cref="Path.DirectorySeparatorChar"/> as a string.
        /// </summary>
        public static readonly string DirectorySeparatorString = new String( Path.DirectorySeparatorChar, 1 );

        /// <summary>
        /// Gets the <see cref="Path.AltDirectorySeparatorChar"/> as a string.
        /// </summary>
        public static readonly string AltDirectorySeparatorString = new String( Path.AltDirectorySeparatorChar, 1 );

        /// <summary>
        /// A display format for <see cref="DateTime"/> that supports round-trips, is readable and can be used in path 
        /// or url (the DateTime should be in UTC since <see cref="DateTime.Kind"/> is ignored).
        /// </summary>
        public static readonly string FileNameUniqueTimeUtcFormat = @"yyyy-MM-dd HH\hmm.ss.fffffff";

        /// <summary>
        /// Finds the first character index of any characters that are invalid in a path.
        /// This method (and <see cref="IndexOfInvalidFileNameChars"/>) avoid the allocation of 
        /// the array each time <see cref="Path.GetInvalidPathChars"/> is called.
        /// </summary>
        /// <param name="path">Path to check. Can not be null.</param>
        /// <returns>A negative value if not found.</returns>
        public static int IndexOfInvalidPathChars( string path )
        {
            return path.IndexOfAny( _invalidPathChars );
        }

        /// <summary>
        /// Finds the first character index of any characters that are invalid in a file name.
        /// This method (and <see cref="IndexOfInvalidPathChars"/>) avoid the allocation of 
        /// the array each time <see cref="Path.GetInvalidFileNameChars"/> is called.
        /// </summary>
        /// <param name="path">Path to check. Can not be null.</param>
        /// <returns>A negative value if not found.</returns>
        public static int IndexOfInvalidFileNameChars( string path )
        {
            return path.IndexOfAny( _invalidFileNameChars );
        }

        /// <summary>
        /// Creates a new necessarily unique file and writes bytes content in a directory that must exist.
        /// The file name is based on a <see cref="DateTime"/>, with an eventual uniquifier if a file already exists with the same name.
        /// </summary>
        /// <param name="pathPrefix">The path prefix. Must not be null. Must be a valid path and may ends with a prefix for the file name itself.</param>
        /// <param name="fileSuffix">Suffix for the file name. Must not be null. Typically an extension (like ".txt").</param>
        /// <param name="time">The time that will be used to create the file name. It should be an UTC time.</param>
        /// <param name="content">The bytes to write. Can be null or empty if the file must only be created.</param>
        /// <param name="withUTF8Bom">True to write the UTF8 Byte Order Mask (the preamble).</param>
        /// <param name="maxTryBeforeGuid">Maximum value for short hexa uniquifier before using a base 64 guid suffix. Must between 0 and 15 (included).</param>
        /// <returns>The full path name of the created file.</returns>
        public static string WriteUniqueTimedFile( string pathPrefix, string fileSuffix, DateTime time, byte[] content, bool withUTF8Bom, int maxTryBeforeGuid = 3 )
        {
            string fullLogFilePath;
            using( var f = CreateAndOpenUniqueTimedFile( pathPrefix, fileSuffix, time, FileAccess.Write, FileShare.Read, 8, FileOptions.SequentialScan | FileOptions.WriteThrough, maxTryBeforeGuid ) )
            {
                Debug.Assert( Encoding.UTF8.GetPreamble().Length == 3 );
                if( withUTF8Bom ) f.Write( Encoding.UTF8.GetPreamble(), 0, 3 );
                if( content != null && content.Length > 0 ) f.Write( content, 0, content.Length );
                fullLogFilePath = f.Name;
            }
            return fullLogFilePath;
        }

        /// <summary>
        /// Creates and opens a new necessarily unique file in a directory that must exist.
        /// The file name is based on a <see cref="DateTime"/>, with an eventual uniquifier if a file already exists with the same name.
        /// You can use <see cref="FileStream.Name"/> to obtain the file name.
        /// </summary>
        /// <param name="pathPrefix">The path prefix. Must not be null. Must be a valid path and may ends with a prefix for the file name itself.</param>
        /// <param name="fileSuffix">Suffix for the file name. Must not be null. Typically an extension (like ".txt").</param>
        /// <param name="time">The time that will be used to create the file name. It should be an UTC time.</param>
        /// <param name="access">
        /// A constant that determines how the file can be accessed by the FileStream object. 
        /// It can only be <see cref="FileAccess.Write"/> or <see cref="FileAccess.ReadWrite"/> (when set to <see cref="FileAccess.Read"/> a <see cref="ArgumentException"/> is thrown).
        /// This sets the CanRead and CanWrite properties of the FileStream object. 
        /// CanSeek is true if path specifies a disk file.
        /// </param>
        /// <param name="share">
        /// A constant that determines how the file will be shared by processes.
        /// </param>
        /// <param name="bufferSize">
        /// A positive Int32 value greater than 0 indicating the buffer size. For bufferSize values between one and eight, the actual buffer size is set to eight bytes.
        /// </param>
        /// <param name="options">Specifies additional file options.</param>
        /// <param name="maxTryBeforeGuid">Maximum value for short hexadecimal uniquifier before using a base 64 guid suffix. Must between 0 and 15 (included).</param>
        /// <returns>An opened <see cref="FileStream"/>.</returns>
        public static FileStream CreateAndOpenUniqueTimedFile( string pathPrefix, string fileSuffix, DateTime time, FileAccess access, FileShare share, int bufferSize, FileOptions options, int maxTryBeforeGuid = 3 )
        {
            if( pathPrefix == null ) throw new ArgumentNullException( "pathPrefix" );
            if( fileSuffix == null ) throw new ArgumentNullException( "fileSuffix" );
            if( maxTryBeforeGuid < 0 || maxTryBeforeGuid > 15 ) throw new ArgumentOutOfRangeException( "maxTryBeforeGuid" );
            if( access == FileAccess.Read ) throw new ArgumentException( R.FileUtilNoReadOnlyWhenCreateFile, "access" );
            FileStream f;
            string pOrigin = pathPrefix + time.ToString( FileNameUniqueTimeUtcFormat ) + fileSuffix;
            string p = pOrigin;
            int counter = 0;
            for( ; ; )
            {
                if( TryCreateNew( p, access, share, bufferSize, options, out f ) ) break;
                if( counter >= maxTryBeforeGuid )
                {
                    if( counter == maxTryBeforeGuid+1 ) throw new CKException( R.FileUtilUnableToCreateUniqueTimedFile );
                    if( counter == maxTryBeforeGuid )
                    {
                        Debug.Assert( Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).Length == 24 );
                        Debug.Assert( Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).EndsWith( "==" ) );
                        // Use http://en.wikipedia.org/wiki/Base64#URL_applications encoding.
                        string dedup = Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).Remove( 22 ).Replace( '+', '-' ).Replace( '/', '_' );
                        p = pOrigin.Insert( pOrigin.Length - fileSuffix.Length, "-" + dedup );
                    }
                }
                else p = pOrigin.Insert( pOrigin.Length - fileSuffix.Length, "-" + Util.Converter.HexChars[counter] );
                ++counter;
            }
            return f;
        }

        static bool TryCreateNew( string path, FileAccess access, FileShare share, int bufferSize, FileOptions options, out FileStream f )
        {
            f = null;
            try
            {
                if( File.Exists( path ) ) return false;
                f = new FileStream( path, FileMode.CreateNew, access, share, bufferSize, options );
                return true;
            }
            catch( IOException ex )
            {
                if( ex is PathTooLongException || ex is DirectoryNotFoundException ) throw;
            }
            return false;
        }


        /// <summary>
        /// Recursively copy a directory. 
        /// Throws an IOException, if a same file exists in the target directory.
        /// </summary>
        /// <param name="src">The source directory.</param>
        /// <param name="target">The target directory.</param>
        /// <param name="withHiddenFiles">False to skip hidden files.</param>
        /// <param name="withHiddenFolders">False to skip hidden folders.</param>
        /// <param name="fileFilter">Optional predicate for directories.</param>
        /// <param name="dirFilter">Optional predicate for files.</param>
        public static void CopyDirectory( DirectoryInfo src, DirectoryInfo target, bool withHiddenFiles = true, bool withHiddenFolders = true, Func<FileInfo,bool> fileFilter = null, Func<DirectoryInfo,bool> dirFilter = null )
        {
            if( src == null ) throw new ArgumentNullException( "src" );
            if( target == null ) throw new ArgumentNullException( "target" );
            if( !target.Exists ) target.Create();
            DirectoryInfo[] dirs = src.GetDirectories();
            foreach( DirectoryInfo d in dirs )
            {
                if( (withHiddenFolders || ((d.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden))
                    && (dirFilter == null || dirFilter( d )) )
                {
                    CopyDirectory( d, new DirectoryInfo( Path.Combine( target.FullName, d.Name ) ), withHiddenFiles, withHiddenFolders );
                }
            }
            FileInfo[] files = src.GetFiles();
            foreach( FileInfo f in files )
            {
                if( (withHiddenFiles || ((f.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden))
                    && (fileFilter == null || fileFilter( f )) )
                {
                    f.CopyTo( Path.Combine( target.FullName, f.Name ) );
                }
            }
        }

        /// <summary>
        /// Waits for a file to be writable. Do not open the file.
        /// Waits approximately the number of seconds given before leaving and returning false.
        /// </summary>
        /// <param name="path">The path of the file to write to.</param>
        /// <param name="nbMaxSecond">Maximum number of seconds to wait before returning false.</param>
        /// <returns>True if the file has been correctly opened in write mode.</returns>
        static public bool WaitForWriteAcccess( string path, int nbMaxSecond )
        {
            if( path == null ) throw new ArgumentNullException( "path" );
            for( ; ; )
            {
                System.Threading.Thread.Sleep( 10 );
                if( !File.Exists( path ) ) return true;
                try
                {
                    using( Stream s = File.OpenWrite( path ) ) { return true; }
                }
                catch
                {
                    if( --nbMaxSecond < 0 ) return false;
                    System.Threading.Thread.Sleep( 990 );
                }
            }
        }
    }

}

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
                ArrayList l = new ArrayList();
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
                return (string[])l.ToArray( typeof( string ) );
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
        /// Detects whether the given path is on a case sensitive volume or not.
        /// </summary>
        /// <param name="path">Path to a file.</param>
        /// <returns>True if this path must be treated as case sensitive.</returns>
        public static bool IsVolumeCaseSensitive( string path )
        {
            if( path == null || path.Length == 0 ) throw new ArgumentException( "path" );

            if( !OSVersionInfo.PInvokeSupported ) return OSVersionInfo.IsUnix;

            path = Path.GetFullPath( path );
            if( path[path.Length - 1] != Path.DirectorySeparatorChar ) path += Path.DirectorySeparatorChar;

            StringBuilder volumeLabel = new StringBuilder( 256 );
            UInt32 volumeFlags = new UInt32();
            StringBuilder fileSystemName = new StringBuilder( 256 );
            UInt32 serialNumber = 0;
            UInt32 maxComponentLength = 0;

            GetVolumeInformation( path,
                volumeLabel,
                (UInt32)volumeLabel.Capacity,
                ref serialNumber,
                ref maxComponentLength,
                ref volumeFlags,
                fileSystemName,
                (UInt32)fileSystemName.Capacity );

            return (volumeFlags & (UInt32)VolumeFlags.CaseSensitive) == (UInt32)VolumeFlags.CaseSensitive;
        }

        /// <summary>
        /// Recursively copy a directory.
        /// </summary>
        /// <param name="src">The source directory.</param>
        /// <param name="target">The target directory.</param>
        /// <param name="withHiddenFiles">False to skip hidden files.</param>
        /// <param name="withHiddenFolders">False to skip hidden folders.</param>
        /// <param name="fileFilter">Optional predicate for directories.</param>
        /// <param name="dirFilter">Optional predicate for files.</param>
        public static void CopyDirectory( DirectoryInfo src, DirectoryInfo target, bool withHiddenFiles = true, bool withHiddenFolders = true, Predicate<FileInfo> fileFilter = null, Predicate<DirectoryInfo> dirFilter = null )
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
                    && (fileFilter == null && fileFilter( f )) )
                {
                    f.CopyTo( Path.Combine( target.FullName, f.Name ) );
                }
            }
        }

        /// <summary>
        /// Waits for a file to be writable. Do not open the file.
        /// Waits approximately the number of seconds given before leaving and returning false.
        /// </summary>
        /// <param name="file">The file to write to.</param>
        /// <param name="nbMaxSecond">Maximum number of seconds to wait before returning false.</param>
        /// <returns>True if the file has been correctly opened in write mode.</returns>
        static public bool WaitForWriteAcccess( FileInfo file, int nbMaxSecond )
        {
            for( ; ; )
            {
                System.Threading.Thread.Sleep( 100 );
                if( !file.Exists ) return true;
                try
                {
                    using( Stream s = file.OpenWrite() ) { return true; }
                }
                catch
                {
                    if( --nbMaxSecond < 0 ) return false;
                    System.Threading.Thread.Sleep( 900 );
                }
            }
        }

        /// <summary>
        /// Supporting flags that may be set on a file system (descriptions are from Win32 api).
        /// </summary>
        [Flags]
        enum VolumeFlags
        {
            /// <summary>
            /// None.
            /// </summary>
            None = 0x0,
            /// <summary>
            /// The file system supports case-sensitive file names.
            /// </summary>
            CaseSensitive = 0x00000001,
            /// <summary>
            /// The file system preserves the case of file names when it places a name on disk.
            /// </summary>
            PreservesCase = 0x00000002,
            /// <summary>
            /// The file system supports Unicode in file names as they appear on disk.
            /// </summary>
            SupportsUnicodeOnVolume = 0x00000004,
            /// <summary>
            /// The file system preserves and enforces ACLs. For example, NTFS preserves and enforces ACLs, and FAT does not.
            /// </summary>
            PersistentAcls = 0x00000008,
            /// <summary>
            /// The specified volume is a compressed volume; for example, a DoubleSpace volume.
            /// </summary>
            Compressed = 0x00008000,
            /// <summary>
            /// The specified volume is read-only.
            /// </summary>
            ReadOnly = 0x00080000,
            /// <summary>
            /// The file system supports the Encrypted File System (EFS).
            /// </summary>
            SupportsEncryption = 0x00020000,
            /// <summary>
            /// The file system supports file-based compression.
            /// </summary>
            SupportsFileCompression = 0x00000010,
            /// <summary>
            /// The file system supports named streams.
            /// </summary>
            SupportsNamedStreams = 0x00040000,
            /// <summary>
            /// The file system supports object identifiers.
            /// </summary>
            SupportsObjectIds = 0x00010000,
            /// <summary>
            /// The file system supports disk quotas.
            /// </summary>
            SupportsQuotas = 0x00000020,
            /// <summary>
            /// The file system supports reparse points.
            /// </summary>
            SupportsReparsePoints = 0x00000080,
            /// <summary>
            /// The file system supports sparse files.
            /// </summary>
            SupportsSparseFiles = 0x00000040,
        };

        [DllImport( "kernel32.dll" )]
        private static extern long GetVolumeInformation( string pathName, StringBuilder volumeNameBuffer, UInt32 volumeNameSize, ref UInt32 volumeSerialNumber, ref UInt32 maximumComponentLength, ref UInt32 fileSystemFlags, StringBuilder fileSystemNameBuffer, UInt32 fileSystemNameSize );


    }

}

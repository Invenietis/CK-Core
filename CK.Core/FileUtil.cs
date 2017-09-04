using CK.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Helper functions related to file system.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Combination of <see cref="FileAttributes"/> that can not exist: it can be used to 
        /// tag non existing files among other existing (i.e. valid) file attributes.
        /// </summary>
        static readonly public FileAttributes InexistingFile = FileAttributes.Normal | FileAttributes.Offline;
        
        /// <summary>
        /// The file header for gzipped files.
        /// </summary>
        static readonly public byte[] GzipFileHeader = new byte[] { 0x1f, 0x8b };

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
        /// <returns>
        /// A standardized path, whatever the actual <c>Path.DirectorySeparatorChar</c> is
        /// on the current platform.
        /// </returns>
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

        static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();
        static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();

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
        /// Use <see cref="MatchFileNameUniqueTimeUtcFormat"/> or <see cref="TryParseFileNameUniqueTimeUtcFormat"/> to parse it (it uses the correct <see cref="DateTimeStyles"/>).
        /// It is: @"yyyy-MM-dd HH\hmm.ss.fffffff"
        /// </summary>
        public static readonly string FileNameUniqueTimeUtcFormat = @"yyyy-MM-dd HH\hmm.ss.fffffff";

        /// <summary>
        /// The time returned by <see cref="File.GetLastWriteTimeUtc"/> when the file does not exist.
        /// From MSDN: If the file described in the path parameter does not exist, this method returns 12:00 midnight, January 1, 1601 A.D. (C.E.) Coordinated Universal Time (UTC).
        /// </summary>
        public static readonly DateTime MissingFileLastWriteTimeUtc = new DateTime( 1601, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Matches a DateTime in the <see cref="FileNameUniqueTimeUtcFormat"/> format.
        /// </summary>
        /// <param name="this">This <see cref="StringMatcher"/>.</param>
        /// <param name="time">Result time on success; otherwise <see cref="Util.UtcMinValue"/>.</param>
        /// <returns>True if the time has been matched.</returns>
        public static bool MatchFileNameUniqueTimeUtcFormat( this StringMatcher @this, out DateTime time )
        {
            time = Util.UtcMinValue;
            Debug.Assert( FileNameUniqueTimeUtcFormat.Replace( "\\", "" ).Length == 27 );
            return @this.Length >= 27 
                    && DateTime.TryParseExact( @this.Text.Substring( @this.StartIndex, 27 ), FileUtil.FileNameUniqueTimeUtcFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out time )
                ? @this.Forward( 27 )
                : @this.SetError();
        }

        /// <summary>
        /// Tries to parse a string formatted with the <see cref="FileNameUniqueTimeUtcFormat"/>.
        /// The string must contain only the time unless <paramref name="allowSuffix"/> is true.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="time">Result time on success.</param>
        /// <param name="allowSuffix">True to accept a string that starts with the time and contains more text.</param>
        /// <returns>True if the string has been successfully parsed.</returns>
        public static bool TryParseFileNameUniqueTimeUtcFormat( string s, out DateTime time, bool allowSuffix = false )
        {
            var m = new StringMatcher( s );
            return m.MatchFileNameUniqueTimeUtcFormat( out time ) && (allowSuffix || m.IsEnd);
        }

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
        /// <param name="time">The time that will be used to create the file name. It must be an UTC time.</param>
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
        /// <param name="maxTryBeforeGuid">
        /// Maximum value for short hexadecimal uniquifier before using a base 64 guid suffix. Must greater than 0.</param>
        /// <returns>An opened <see cref="FileStream"/>.</returns>
        public static FileStream CreateAndOpenUniqueTimedFile( string pathPrefix, string fileSuffix, DateTime time, FileAccess access, FileShare share, int bufferSize, FileOptions options, int maxTryBeforeGuid = 512 )
        {
            if( access == FileAccess.Read ) throw new ArgumentException( Impl.CoreResources.FileUtilNoReadOnlyWhenCreateFile, "access" );
            FileStream f = null;
            FindUniqueTimedFile( pathPrefix, fileSuffix, time, maxTryBeforeGuid, p => TryCreateNew( p, access, share, bufferSize, options, out f ) );
            return f;
        }

        static bool TryCreateNew( string timedPath, FileAccess access, FileShare share, int bufferSize, FileOptions options, out FileStream f )
        {
            f = null;
            try
            {
                if( File.Exists( timedPath ) ) return false;
                f = new FileStream( timedPath, FileMode.CreateNew, access, share, bufferSize, options );
                return true;
            }
            catch( IOException ex )
            {
                if( ex is PathTooLongException || ex is DirectoryNotFoundException ) throw;
            }
            return false;
        }

        /// <summary>
        /// Moves (renames) a file to a necessarily unique named file.
        /// The file name is based on a <see cref="DateTime"/>, with an eventual uniquifier if a file already exists with the same name.
        /// </summary>
        /// <param name="sourceFilePath">Path of the file to move.</param>
        /// <param name="pathPrefix">The path prefix. Must not be null. Must be a valid path and may ends with a prefix for the file name itself.</param>
        /// <param name="fileSuffix">Suffix for the file name. Must not be null. Typically an extension (like ".txt").</param>
        /// <param name="time">The time that will be used to create the file name. It must be an UTC time.</param>
        /// <param name="maxTryBeforeGuid">
        /// Maximum value for short hexadecimal uniquifier before using a base 64 guid suffix. Must greater than 0.
        /// </param>
        /// <returns>An opened <see cref="FileStream"/>.</returns>
        public static string MoveToUniqueTimedFile( string sourceFilePath, string pathPrefix, string fileSuffix, DateTime time, int maxTryBeforeGuid = 512 )
        {
            if( sourceFilePath == null ) throw new ArgumentNullException( "sourceFilePath" );
            if( !File.Exists( sourceFilePath ) ) throw new FileNotFoundException( Impl.CoreResources.FileMustExist, sourceFilePath );
            return FindUniqueTimedFile( pathPrefix, fileSuffix, time, maxTryBeforeGuid, p => TryMoveTo( sourceFilePath, p ) );
        }

        /// <summary>
        /// Gets a path to a necessarily unique named file.
        /// The file name is based on a <see cref="DateTime"/>, with an eventual uniquifier if a file already exists with the same name.
        /// </summary>
        /// <param name="pathPrefix">The path prefix. Must not be null. Must be a valid path and may ends with a prefix for the file name itself.</param>
        /// <param name="fileSuffix">Suffix for the file name. Must not be null. Typically an extension (like ".txt").</param>
        /// <param name="time">The time that will be used to create the file name. It must be an UTC time.</param>
        /// <param name="maxTryBeforeGuid">
        /// Maximum value for short hexadecimal uniquifier before using a base 64 guid suffix. Must greater than 0.
        /// </param>
        /// <returns>A string to a necessarily unique named file path.</returns>
        public static string EnsureUniqueTimedFile( string pathPrefix, string fileSuffix, DateTime time, int maxTryBeforeGuid = 512 )
        {
            return FindUniqueTimedFile( pathPrefix, fileSuffix, time, maxTryBeforeGuid, p => TryCreateFile( p ) );
        }

        static bool TryCreateFile( string path )
        {
            try
            {
                if( File.Exists( path ) ) return false;
                using( File.Create( path ) ) { } // Dispose immediately
                return true;
            }
            catch( IOException ex )
            {
                if( ex is PathTooLongException || ex is DirectoryNotFoundException ) throw;
            }
            return false;
        }

        static bool TryMoveTo( string sourceFilePath, string timedPath )
        {
            try
            {
                if( File.Exists( timedPath ) ) return false;
                File.Move( sourceFilePath, timedPath );
                return true;
            }
            catch( IOException ex )
            {
                if( ex is PathTooLongException || ex is DirectoryNotFoundException ) throw;
            }
            return false;
        }

        static string FindUniqueTimedFile( string pathPrefix, string fileSuffix, DateTime time, int maxTryBeforeGuid, Func<string, bool> tester )
        {
            if( pathPrefix == null ) throw new ArgumentNullException( "pathPrefix" );
            if( fileSuffix == null ) throw new ArgumentNullException( "fileSuffix" );
            if( maxTryBeforeGuid < 0 ) throw new ArgumentOutOfRangeException( "maxTryBeforeGuid" );

            DateTimeStamp timeStamp = new DateTimeStamp( time );
            int counter = 0;
            string result = pathPrefix + timeStamp.ToString() + fileSuffix;
            for( ; ; )
            {
                if( tester( result ) ) break;
                if( counter < maxTryBeforeGuid )
                {
                    timeStamp = new DateTimeStamp( timeStamp, timeStamp );
                    result = pathPrefix + timeStamp.ToString() + fileSuffix;
                }
                else
                {
                    if( counter == maxTryBeforeGuid + 1 ) throw new Exception( Impl.CoreResources.FileUtilUnableToCreateUniqueTimedFile );
                    if( counter == maxTryBeforeGuid )
                    {
                        result = pathPrefix + FormatTimedUniqueFilePart( time ) + fileSuffix;
                    }
                }
                ++counter;
            }
            return result;
        }

        /// <summary>
        /// Formats a string that is file name compatible from the given time and a <see cref="Guid.NewGuid()"/>
        /// where time uses <see cref="FileNameUniqueTimeUtcFormat"/> and the new Guid is 
        /// encoded with http://en.wikipedia.org/wiki/Base64#URL_applications.
        /// </summary>
        /// <param name="time">The date time to use.</param>
        /// <returns>A string with the time and a new guid in a file system compatible format.</returns>
        public static string FormatTimedUniqueFilePart( DateTime time )
        {
            Debug.Assert( Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).Length == 24 );
            Debug.Assert( Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).EndsWith( "==" ) );
            // Use http://en.wikipedia.org/wiki/Base64#URL_applications encoding.
            string dedup = Convert.ToBase64String( Guid.NewGuid().ToByteArray() ).Remove( 22 ).Replace( '+', '-' ).Replace( '/', '_' );
            return time.ToString( FileNameUniqueTimeUtcFormat, CultureInfo.InvariantCulture ) + "-" + dedup;
        }

        /// <summary>
        /// Recursively copy a directory, creates it if it does not already exists. 
        /// Throws an IOException, if a same file exists in the target directory.
        /// </summary>
        /// <param name="src">The source directory.</param>
        /// <param name="target">The target directory.</param>
        /// <param name="withHiddenFiles">False to skip hidden files.</param>
        /// <param name="withHiddenFolders">False to skip hidden folders.</param>
        /// <param name="fileFilter">Optional predicate for directories.</param>
        /// <param name="dirFilter">Optional predicate for files.</param>
        public static void CopyDirectory( DirectoryInfo src, DirectoryInfo target, bool withHiddenFiles = true, bool withHiddenFolders = true, Func<FileInfo, bool> fileFilter = null, Func<DirectoryInfo, bool> dirFilter = null )
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
        /// Waits for a file to be writable or does not exist (if it does not exist, it can be created!).
        /// The file is opened and close.
        /// Waits the number of <paramref name="nbMaxMilliSecond"/> before leaving and returning false: when 0 (the default),
        /// there is no wai. A nbMaxMilliSecond below 20 ~ 30 milliseconds is not accurate: even with nbMaxMilliSecond = 1
        /// this method will return true if the file becomes writable during the next 10 or 20 milliseconds.
        /// </summary>
        /// <param name="path">The path of the file to write to.</param>
        /// <param name="nbMaxMilliSecond">Maximum number of milliseconds to wait before returning false.</param>
        /// <returns>True if the file has been correctly opened (and closed) in write mode.</returns>
        static public bool CheckForWriteAccess( string path, int nbMaxMilliSecond = 0 )
        {
            if( path == null ) throw new ArgumentNullException( "path" );
            DateTime start = DateTime.UtcNow;
            if( !File.Exists( path ) ) return true;
            try
            {
                using( Stream s = File.OpenWrite( path ) ) { return true; }
            }
            catch
            {
                int waitTime = nbMaxMilliSecond / 100;
                if( nbMaxMilliSecond <= 0 ) return false;
                long stop = start.AddMilliseconds( nbMaxMilliSecond ).Ticks;
                for( ; ; )
                {
                    if( waitTime > 0 ) Thread.Sleep( waitTime );
                    if( !File.Exists( path ) ) return true;
                    try
                    {
                        using( Stream s = File.OpenWrite( path ) ) { return true; }
                    }
                    catch
                    {
                        if( DateTime.UtcNow.Ticks > stop ) return false;
                        if( waitTime < 20 ) waitTime += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Compresses a file to another file asynchronously, using GZip at the given compression level.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationPath">The destination path. If it doesn't exist, it will be created. If it exists, it will be replaced.</param>
        /// <param name="cancellationToken">Optional cancellation token for the task.</param>
        /// <param name="deleteSourceFileOnSuccess">If set to <c>true</c>, will delete source file if no error occurred during compression.</param>
        /// <param name="level">Compression level to use.</param>
        /// <param name="bufferSize">Size of the buffer, in bytes.</param>
        public static async Task CompressFileToGzipFileAsync( string sourceFilePath, string destinationPath, CancellationToken cancellationToken = default(CancellationToken), bool deleteSourceFileOnSuccess = true, CompressionLevel level = CompressionLevel.Optimal, int bufferSize = 64*1024 )
        {
            using( FileStream source = new FileStream( sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, FileOptions.Asynchronous|FileOptions.SequentialScan ) )
            {
                using( FileStream destination = new FileStream( destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan ) )
                {
                    // GZipStream writes the GZipFileGeader.
                    using( GZipStream gZipStream = new GZipStream( destination, level ) )
                    {
                        await source.CopyToAsync( gZipStream, bufferSize, cancellationToken );
                    }
                }
            }

            if( !cancellationToken.IsCancellationRequested && deleteSourceFileOnSuccess )
            {
                File.Delete( sourceFilePath );
            }
        }

        /// <summary>
        /// Compresses a file to another file, using GZip at the given compression level.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destinationPath">The destination path. If it doesn't exist, it will be created. If it exists, it will be replaced.</param>
        /// <param name="deleteSourceFileOnSuccess">if set to <c>true</c>, will delete source file if no errors occured during compression.</param>
        /// <param name="level">Compression level to use.</param>
        /// <param name="bufferSize">Size of the buffer, in bytes.</param>
        public static void CompressFileToGzipFile( string sourceFilePath, string destinationPath, bool deleteSourceFileOnSuccess, CompressionLevel level = CompressionLevel.Optimal, int bufferSize = 64*1024 )
        {
            using( FileStream source = new FileStream( sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, useAsync: false ) )
            {
                using( FileStream destination = new FileStream( destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: false ) )
                {
                    using( GZipStream gZipStream = new GZipStream( destination, level ) )
                    {
                        source.CopyTo( gZipStream, bufferSize );
                    }
                }
            }
            if( deleteSourceFileOnSuccess )
            {
                File.Delete( sourceFilePath );
            }
        }

    }

}

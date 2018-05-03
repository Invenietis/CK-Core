using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Globalization;
using FluentAssertions;
using System.Collections.Concurrent;

namespace CK.Core.Tests
{

    public class FileUtilTests
    {
        [Test]
        public void NormalizePathSeparator_uses_current_environment()
        {
            if( Path.DirectorySeparatorChar == '\\' )
            {
                Action a = () => FileUtil.NormalizePathSeparator( null, true );
                a.Should().Throw<ArgumentNullException>();
                a = () => FileUtil.NormalizePathSeparator( null, false );
                a.Should().Throw<ArgumentNullException>();

                FileUtil.NormalizePathSeparator( "", true ).Should().Be( "" );
                FileUtil.NormalizePathSeparator( "", false ).Should().Be( "" );

                FileUtil.NormalizePathSeparator( @"/\C", false ).Should().Be( @"\\C" );
                FileUtil.NormalizePathSeparator( @"/\C/", true ).Should().Be( @"\\C\" );
                FileUtil.NormalizePathSeparator( @"/\C\", true ).Should().Be( @"\\C\" );
                FileUtil.NormalizePathSeparator( @"/\C", true ).Should().Be( @"\\C\" );

                FileUtil.NormalizePathSeparator( @"/", false ).Should().Be( @"\" );
                FileUtil.NormalizePathSeparator( @"/a", true ).Should().Be( @"\a\" );
            }
        }

        [Test]
        public void WriteUniqueTimedFile()
        {
            string prefix = Path.Combine( TestHelper.TestFolder, "Unique " );
            string f1 = FileUtil.WriteUniqueTimedFile( prefix, ".txt", DateTime.UtcNow, Encoding.UTF8.GetBytes( "Hello..." ), false );
            string f2 = FileUtil.WriteUniqueTimedFile( prefix, ".txt", DateTime.UtcNow, Encoding.UTF8.GetBytes( "...World!" ), false );

            File.ReadAllText( f1 ).Should().Be( "Hello..." );
            File.ReadAllText( f2 ).Should().Be( "...World!" );

            Action a = () => FileUtil.WriteUniqueTimedFile( prefix, String.Empty, DateTime.UtcNow, null, true, -1 );
            a.Should().Throw<ArgumentOutOfRangeException>();
            a = () => FileUtil.WriteUniqueTimedFile( prefix, null, DateTime.UtcNow, null, true );
            a.Should().Throw<ArgumentNullException>();
            a = () => FileUtil.WriteUniqueTimedFile( null, String.Empty, DateTime.UtcNow, null, true );
            a.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void UniqueTimedFile_is_27_characters_long_as_a_string_or_50_with_the_GUID_uniquifier()
        {
            DateTime.UtcNow.ToString( FileUtil.FileNameUniqueTimeUtcFormat, CultureInfo.InvariantCulture ).Length
                       .Should().Be( 27, "FileNameUniqueTimeUtcFormat => 27 characters long." );
            FileUtil.FormatTimedUniqueFilePart( DateTime.UtcNow ).Length
                       .Should().Be( 50, "TimedUniqueFile and its Guid => 50 characters long." );
            DateTimeStamp.MaxValue.ToString().Length
                       .Should().Be( 32, "DateTimeStamp FileNameUniqueTimeUtcFormat and the uniquifier: max => 32 characters long." );
        }


        [Test]
        public void WriteUniqueTimedFile_automatically_numbers_files()
        {
            TestHelper.CleanupTestFolder();

            DateTime now = DateTime.UtcNow;
            var content = Encoding.UTF8.GetBytes( String.Format( "Clash @{0}...", now ) );
            string prefix = Path.Combine( TestHelper.TestFolder, "Clash " );
            List<string> files = new List<string>();
            for( int i = 0; i < 10; ++i )
            {
                files.Add( FileUtil.WriteUniqueTimedFile( prefix, String.Empty, now, content, true, 6 ) );
            }
            files.Count.Should().Be( 10 );
            files.ForEach( f => f.Should().StartWith( prefix ) );
            files.ForEach( f => File.Exists( f ).Should().BeTrue() );
            files[1].Should().Be( files[0] + "(1)" );
            files[2].Should().Be( files[0] + "(2)" );
            files[3].Should().Be( files[0] + "(3)" );
            files[4].Should().Be( files[0] + "(4)" );
            files[5].Should().Be( files[0] + "(5)" );
            files[6].Should().Be( files[0] + "(6)" );
            for( int i = 7; i < 10; ++i )
            {
                files[i].Length.Should().Be( files[0].Length + 1 + 22, "Ends with Url compliant Base64 GUID." );
            }
            files.SequenceEqual( files.Distinct() ).Should().BeTrue();
        }

        [Test]
        public void WriteUniqueTimedFile_clash_never_happen()
        {
            TestHelper.CleanupTestFolder();
            DateTime now = DateTime.UtcNow;
            string prefix = Path.Combine( TestHelper.TestFolder, "Clash " );
            var files = new string[100];
            Parallel.ForEach( Enumerable.Range( 0, 100 ), i =>
            {
                files[i] = FileUtil.WriteUniqueTimedFile( prefix, String.Empty, now, null, false, 0 );
            } );
            files.Should().NotContainNulls();
            files.Should().OnlyContain( f => f.StartsWith( prefix ) );
            files.Should().OnlyContain( f => File.Exists( f ) );
            var winner = files.MaxBy( f => -f.Length );
            files.Where( f => f.Length == winner.Length + 1 + 22 ).Should().HaveCount( 99, "Ends with Url compliant Base64 GUID." );
        }

        [Test]
        public void CreateUniqueTimedFolder_simple_test()
        {
            TestHelper.CleanupTestFolder();
            DateTime now = DateTime.UtcNow;
            var prefix = Path.Combine( TestHelper.TestFolder,"F/Simple/F-" );
            var f1 = FileUtil.CreateUniqueTimedFolder( prefix, String.Empty, now );
            var f2 = FileUtil.CreateUniqueTimedFolder( prefix, String.Empty, now );
            f1.Should().NotBe( f2 );
            Directory.Exists( f1 ).Should().BeTrue();
            Directory.Exists( f2 ).Should().BeTrue();
        }

        [Test]
        public void CreateUniqueTimedFolder_clash_never_happen()
        {
            TestHelper.CleanupTestFolder();
            DateTime now = DateTime.UtcNow;
            var prefixes = new[] {
                Path.Combine( TestHelper.TestFolder, "F-Clash/FA" ),
                Path.Combine( TestHelper.TestFolder, "F-Clash/FB" ),
                Path.Combine( TestHelper.TestFolder, "F-Clash/FA/F1" ) };
            var folders = new string[100];
            Parallel.ForEach( Enumerable.Range( 0, 100 ), i =>
            {
                folders[i] = FileUtil.CreateUniqueTimedFolder( prefixes[i % 3], String.Empty, now );
            } );
            folders.Should().NotContainNulls();
            folders.Should().OnlyContain( f => f.StartsWith( prefixes[0] ) || f.StartsWith( prefixes[1] ) || f.StartsWith( prefixes[2] ) );
            folders.Should().OnlyContain( f => Directory.Exists( f ) );
        }

        [Test]
        public void test_CopyDirectory_helper()
        {
            TestHelper.CleanupTestFolder();
            DirectoryInfo copyDir = new DirectoryInfo( Path.Combine( TestHelper.TestFolder, "Cpy" ) );
            DirectoryInfo testDir = new DirectoryInfo( Path.Combine( TestHelper.TestFolder, "Src" ) );

            copyDir.Create();
            testDir.Create();

            CleanupDir( copyDir.FullName );

            CreateFiles( testDir.FullName, "azerty.png" );
            CreateHiddenFiles( testDir.FullName, "hiddenAzerty.gif" );

            FileUtil.CopyDirectory( testDir, copyDir );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png", "hiddenAzerty.gif" );

            Action a = () => FileUtil.CopyDirectory( testDir, copyDir );
            a.Should().Throw<IOException>();

            CleanupDir( copyDir.FullName );

            FileUtil.CopyDirectory( testDir, copyDir, false );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );

            CleanupDir( copyDir.FullName );

            DirectoryInfo recursiveDir = Directory.CreateDirectory( testDir.FullName + @"\recDir" );
            CreateFiles( recursiveDir.FullName, "REC.png" );
            CreateHiddenFiles( recursiveDir.FullName, "hiddenREC.gif" );

            FileUtil.CopyDirectory( testDir, copyDir );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( Path.Combine( copyDir.FullName, recursiveDir.Name ), Directory.GetFiles( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), "REC.png", "hiddenREC.gif" );

            CleanupDir( copyDir.FullName );

            recursiveDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileUtil.CopyDirectory( testDir, copyDir, false, false );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            Directory.Exists( Path.Combine( copyDir.FullName, recursiveDir.Name ) ).Should().BeFalse();

            CleanupDir( copyDir.FullName );

            recursiveDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileUtil.CopyDirectory( testDir, copyDir, false, true );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            AssertContains( Path.Combine( copyDir.FullName, recursiveDir.Name ), Directory.GetFiles( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), "REC.png" );

            CleanupDir( copyDir.FullName );

            FileUtil.CopyDirectory( testDir, copyDir, true, true, x => { return x.Name == "azerty.png"; }, x => { return x.Name != recursiveDir.Name; } );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            Directory.Exists( Path.Combine( copyDir.FullName, recursiveDir.Name ) ).Should().BeFalse();

            // Exception Test
            a = () => FileUtil.CopyDirectory( null, testDir );
            a.Should().Throw<ArgumentNullException>();
            a = () => FileUtil.CopyDirectory( testDir, null );
            a.Should().Throw<ArgumentNullException>();

            Thread.Sleep( 100 );
            TestHelper.CleanupTestFolder();
        }

        static void CleanupDir( string path )
        {
            if( Directory.Exists( path ) ) Directory.Delete( path, true );
            Directory.CreateDirectory( path );
        }

        [Test]
        public void GetLastWriteTimeUtc_returns_FileUtil_MissingFileLastWriteTimeUtc()
        {
            // From MSDN: If the file described in the path parameter does not exist, this method returns 12:00 midnight, January 1, 1601 A.D. (C.E.) Coordinated Universal Time (UTC).
            File.GetLastWriteTimeUtc( Path.Combine( TestHelper.TestFolder, "KExistePAS.txt" ) ).Should().Be( new DateTime( 1601, 1, 1, 0, 0, 0, DateTimeKind.Utc ) );
            File.GetLastWriteTimeUtc( "I:\\KExistePAS.txt" ).Should().Be( FileUtil.MissingFileLastWriteTimeUtc );
        }

        [Test]
        public void CheckForWriteAccess_is_immediately_true_when_file_does_not_exist_or_is_writeable()
        {
            Action a = () => FileUtil.CheckForWriteAccess( null, 0 );
            a.Should().Throw<ArgumentNullException>();
            TestHelper.CleanupTestFolder();
            string path = Path.Combine( TestHelper.TestFolder, "Locked.txt" );
            FileUtil.CheckForWriteAccess( path, 0 ).Should().BeTrue( "If the file does not exist, it is writeable." );
            File.WriteAllText( path, "Locked" );
            FileUtil.CheckForWriteAccess( path, 0 ).Should().BeTrue( "The is writeable: no need to wait." );
        }


        [TestCase( 100, 300, true )]
        [TestCase( 0, 100, true )]
        public void CheckForWriteAccess_is_not_exact_but_works( int lockTimeMilliSecond, int nbMaxMilliSecond, bool result )
        {
            TestHelper.CleanupTestFolder();
            string path = Path.Combine( TestHelper.TestFolder, "Locked.txt" );
            object startLock = new object();
            Task.Factory.StartNew( () =>
             {
                 using( FileStream fs = File.OpenWrite( path ) )
                 {
                     Thread.Sleep( 2 );
                     lock( startLock ) Monitor.Pulse( startLock );
                     Thread.Sleep( lockTimeMilliSecond );
                 }
             } );
            lock( startLock ) Monitor.Wait( startLock );
            FileUtil.CheckForWriteAccess( path, nbMaxMilliSecond ).Should().Be( result );
            TestHelper.CleanupTestFolder();
        }

        [Test]
        public void test_IndexOfInvalidFileNameChars()
        {
            FileUtil.IndexOfInvalidFileNameChars( "" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidFileNameChars( "a" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidFileNameChars( "ab" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidFileNameChars( "abcde" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidFileNameChars( "a<" ).Should().Be( 1 );
            FileUtil.IndexOfInvalidFileNameChars( "a:" ).Should().Be( 1 );
            FileUtil.IndexOfInvalidFileNameChars( "ab<" ).Should().Be( 2 );
            FileUtil.IndexOfInvalidFileNameChars( "<a" ).Should().Be( 0 );
            FileUtil.IndexOfInvalidFileNameChars( "abc>" ).Should().Be( 3 );
            FileUtil.IndexOfInvalidFileNameChars( "abc|" ).Should().Be( 3 );
            FileUtil.IndexOfInvalidFileNameChars( "abc\"" ).Should().Be( 3 );
        }

        [Test]
        public void test_IndexOfInvalidPathChars()
        {
            FileUtil.IndexOfInvalidPathChars( "" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidPathChars( "a" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidPathChars( "ab" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidPathChars( "abcde" ).Should().Be( -1 );
            FileUtil.IndexOfInvalidPathChars( "a|" ).Should().Be( 1 );
            FileUtil.IndexOfInvalidPathChars( "ab|" ).Should().Be( 2 );
            FileUtil.IndexOfInvalidPathChars( "|a" ).Should().Be( 0 );
            FileUtil.IndexOfInvalidPathChars( "abc|" ).Should().Be( 3 );
            FileUtil.IndexOfInvalidPathChars( "abc|" ).Should().Be( 3 );
            FileUtil.IndexOfInvalidPathChars( "abc\0-" ).Should().Be( 3 );
        }

        private void AssertContains( string pathDir, string[] result, params string[] values )
        {
            result.Length.Should().Be( values.Length );
            foreach( string s in values )
                result.Should().Contain( Path.Combine( pathDir, s ) );
        }

        private void CreateFiles( string path, params string[] values )
        {
            foreach( string s in values )
            {
                File.Create( Path.Combine( path, s ) ).Dispose();
            }
        }

        private void CreateHiddenFiles( string path, params string[] values )
        {
            foreach( string s in values )
            {
                File.Create( Path.Combine( path, s ) ).Dispose();
                File.SetAttributes( Path.Combine( path, s ), FileAttributes.Hidden );
            }

        }
    }
}

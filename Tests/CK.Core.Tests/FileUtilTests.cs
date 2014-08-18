#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\FileUtilTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core.Tests;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category("File")]
    public class FileUtilTests
    {
        [Test]
        public void PathNormalizationTest()
        {
            Assert.Throws<ArgumentNullException>( () => FileUtil.NormalizePathSeparator( null, true ) );
            Assert.Throws<ArgumentNullException>( () => FileUtil.NormalizePathSeparator( null, false ) );

            Assert.That( FileUtil.NormalizePathSeparator( "", true ), Is.EqualTo( "" ) );
            Assert.That( FileUtil.NormalizePathSeparator( "", false ), Is.EqualTo( "" ) );

            Assert.That( FileUtil.NormalizePathSeparator( @"/\C", false ), Is.EqualTo( @"\\C" ) );
            Assert.That( FileUtil.NormalizePathSeparator( @"/\C/", true ), Is.EqualTo( @"\\C\" ) );
            Assert.That( FileUtil.NormalizePathSeparator( @"/\C\", true ), Is.EqualTo( @"\\C\" ) );
            Assert.That( FileUtil.NormalizePathSeparator( @"/\C", true ), Is.EqualTo( @"\\C\" ) );

            Assert.That( FileUtil.NormalizePathSeparator( @"/", false ), Is.EqualTo( @"\" ) );
            Assert.That( FileUtil.NormalizePathSeparator( @"/a", true ), Is.EqualTo( @"\a\" ) );
        }

        [Test]
        public void WriteUniqueTimedFile()
        {
            string prefix = Path.Combine( TestHelper.TestFolder, "Unique " );
            string f1 = FileUtil.WriteUniqueTimedFile( prefix, ".txt", DateTime.UtcNow, Encoding.UTF8.GetBytes( "Hello..." ), false );
            string f2 = FileUtil.WriteUniqueTimedFile( prefix, ".txt", DateTime.UtcNow, Encoding.UTF8.GetBytes( "...World!" ), false );

            Assert.That( File.ReadAllText( f1 ), Is.EqualTo( "Hello..." ) );
            Assert.That( File.ReadAllText( f2 ), Is.EqualTo( "...World!" ) );

            Assert.Throws<ArgumentOutOfRangeException>( () => FileUtil.WriteUniqueTimedFile( prefix, String.Empty, DateTime.UtcNow, null, true, -1 ) );
            Assert.Throws<ArgumentNullException>( () => FileUtil.WriteUniqueTimedFile( prefix, null, DateTime.UtcNow, null, true ) );
            Assert.Throws<ArgumentNullException>( () => FileUtil.WriteUniqueTimedFile( null, String.Empty, DateTime.UtcNow, null, true ) );
        }

        [Test]
        public void WriteUniqueTimedFileClash()
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
            Assert.That( files.Count, Is.EqualTo( 10 ) );
            Assert.That( files.All( f => f.StartsWith( prefix ) ) );
            Assert.That( files.All( f => File.Exists( f ) ) );
            Assert.That( files[1], Is.EqualTo( files[0] + "(1)" ) );
            Assert.That( files[2], Is.EqualTo( files[0] + "(2)" ) );
            Assert.That( files[3], Is.EqualTo( files[0] + "(3)" ) );
            Assert.That( files[4], Is.EqualTo( files[0] + "(4)" ) );
            Assert.That( files[5], Is.EqualTo( files[0] + "(5)" ) );
            Assert.That( files[6], Is.EqualTo( files[0] + "(6)" ) );
            for( int i = 7; i < 10; ++i )
            {
                Assert.That( files[i].Length, Is.EqualTo( files[0].Length + 1 + 22 ), "Ends with Url compliant Base64 GUID." );
            }
            Assert.That( files.SequenceEqual( files.Distinct() ) );
        }

        [Test]
        public void WriteUniqueTimedFileClashQuick()
        {
            TestHelper.CleanupTestFolder();
            DateTime now = DateTime.UtcNow;
            string prefix = Path.Combine( TestHelper.TestFolder, "Clash " );
            List<string> files = new List<string>();
            for( int i = 0; i < 10; ++i )
            {
                files.Add( FileUtil.WriteUniqueTimedFile( prefix, String.Empty, now, null, false, 0 ) );
            }
            Assert.That( files.Count, Is.EqualTo( 10 ) );
            Assert.That( files.All( f => f.StartsWith( prefix ) ) );
            Assert.That( files.All( f => File.Exists( f ) ) );
            for( int i = 1; i < 10; ++i )
            {
                Assert.That( files[i].Length, Is.EqualTo( files[0].Length + 1 + 22 ), "Ends with Url compliant Base64 GUID." );
            }
            Assert.That( files.SequenceEqual( files.Distinct() ) );
        }

        [Test]
        public void CopyDirectoryTest()
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

            Assert.Throws<IOException>( () => FileUtil.CopyDirectory( testDir, copyDir ) );

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
            Assert.That( Directory.Exists( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), Is.False );

            CleanupDir( copyDir.FullName );

            recursiveDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileUtil.CopyDirectory( testDir, copyDir, false, true );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            AssertContains( Path.Combine( copyDir.FullName, recursiveDir.Name ), Directory.GetFiles( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), "REC.png" );

            CleanupDir( copyDir.FullName );

            FileUtil.CopyDirectory( testDir, copyDir, true, true, a => { return a.Name == "azerty.png"; }, a => { return a.Name != recursiveDir.Name; } );
            AssertContains( testDir.FullName, Directory.GetFiles( testDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            Assert.That( Directory.Exists( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), Is.False );

            //Exception Test
            Assert.Throws<ArgumentNullException>( () => FileUtil.CopyDirectory( null, testDir ) );
            Assert.Throws<ArgumentNullException>( () => FileUtil.CopyDirectory( testDir, null) );

            TestHelper.CleanupTestFolder();
        }

        static void CleanupDir( string path )
        {
            if( Directory.Exists( path ) ) Directory.Delete( path, true );
            Directory.CreateDirectory( path );
        }

        [Test]
        public void GetLastWriteTimeUtcTest()
        {
            // From MSDN: If the file described in the path parameter does not exist, this method returns 12:00 midnight, January 1, 1601 A.D. (C.E.) Coordinated Universal Time (UTC).
            Assert.That( File.GetLastWriteTimeUtc( Path.Combine( TestHelper.TestFolder, "KExistePAS.txt" ) ), Is.EqualTo( new DateTime( 1601, 1, 1, 0, 0, 0, DateTimeKind.Utc ) ) );
            Assert.That( File.GetLastWriteTimeUtc( "I:\\KExistePAS.txt" ), Is.EqualTo( FileUtil.MissingFileLastWriteTimeUtc ) );
        }

        [Test]
        public void GetFilesTest()
        {
            TestHelper.CleanupTestFolder();
            DirectoryInfo testFolder = new DirectoryInfo( TestHelper.TestFolder );

            CreateFiles( testFolder.FullName, "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.png;*.jpg;*.gif" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "azerty.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "azer*.gif" ), "azerty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "azer*.*if" ), "azerty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "azerty.*g" ), "azerty.jpg", "azerty.png" );
                            
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.png;*.jpg" ), "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.png;*.gif" ), "azerty.gif", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.gif" ), "azerty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, string.Empty ) );
                             
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*;*.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.png;*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.png;*.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
                            
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, ";;" ) );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, ";*;" ), "azerty.gif", "azerty.jpg", "azerty.png" );
                            
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "a" ) );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "a.z" ) );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "" ) );

            TestHelper.CleanupTestFolder();

            CreateFiles( testFolder.FullName, "az.gif", "rty.jpg", "arty.gif", "raz.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.jpg;*.gif" ), "az.gif", "rty.jpg", "arty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "a*.gif" ), "az.gif", "arty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "r*.*" ), "rty.jpg", "raz.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "r*.*;a*.gif" ), "raz.png", "az.gif", "rty.jpg", "arty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "r*.png" ), "raz.png" );

            TestHelper.CleanupTestFolder();
        }

        [Test]
        public void WaitForWriteAccessTest()
        {
            Assert.Throws<ArgumentNullException>( () => FileUtil.WaitForWriteAcccess( null, 0 ) );

            TestHelper.CleanupTestFolder();

            string path = Path.Combine( TestHelper.TestFolder, "Locked.txt" );
            Assert.That( FileUtil.WaitForWriteAcccess( path, 1 ), Is.True );
            
            object startLock = new object();
            Task.Factory.StartNew( () =>
                {
                    FileStream fs = File.Create( path );
                    Thread.Sleep( 2 );
                    lock( startLock ) Monitor.Pulse( startLock );
                    Thread.Sleep( 1000 );
                    fs.Close();
                } );
            lock( startLock ) Monitor.Wait( startLock );
            Assert.That( FileUtil.WaitForWriteAcccess( path, 0 ), Is.False );
            Assert.That( FileUtil.WaitForWriteAcccess( path, 2 ), Is.True );

            TestHelper.CleanupTestFolder();
        }

        [Test]
        public void IndexOfInvalidFileNameCharsTest()
        {
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "a" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "ab" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "abcde" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "a<" ), Is.EqualTo( 1 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "a:" ), Is.EqualTo( 1 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "ab<" ), Is.EqualTo( 2 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "<a" ), Is.EqualTo( 0 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "abc>" ), Is.EqualTo( 3 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "abc|" ), Is.EqualTo( 3 ) );
            Assert.That( FileUtil.IndexOfInvalidFileNameChars( "abc\"" ), Is.EqualTo( 3 ) );
        }

        [Test]
        public void IndexOfInvalidPathCharsTest()
        {
            Assert.That( FileUtil.IndexOfInvalidPathChars( "" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "a" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "ab" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "abcde" ), Is.EqualTo( -1 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "a<" ), Is.EqualTo( 1 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "ab<" ), Is.EqualTo( 2 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "<a" ), Is.EqualTo( 0 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "abc>" ), Is.EqualTo( 3 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "abc|" ), Is.EqualTo( 3 ) );
            Assert.That( FileUtil.IndexOfInvalidPathChars( "abc\"" ), Is.EqualTo( 3 ) );
        }

        private void AssertContains( string pathDir, string[] result, params string[] values )
        {
            Assert.That( result.Length, Is.EqualTo( values.Length ) );
            foreach( string s in values )
                Assert.That( result.Contains<string>( Path.Combine( pathDir, s ) ), Is.True );
        }

        private void CreateFiles( string path, params string[] values )
        {
            foreach( string s in values )
            {
                File.Create( Path.Combine( path, s ) ).Close();
            }
        }

        private void CreateHiddenFiles( string path, params string[] values )
        {
            
            foreach( string s in values )
            {
                File.Create( Path.Combine( path, s ) ).Close();
                File.SetAttributes( Path.Combine( path, s ), FileAttributes.Hidden );
            }

        }
    }
}
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
* Copyright © 2007-2012, 
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
        public void CopyDirectoryTest()
        {
            TestHelper.CleanupTestDir();
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

            TestHelper.CleanupTestDir();
        }

        static void CleanupDir( string path )
        {
            if( Directory.Exists( path ) ) Directory.Delete( path, true );
            Directory.CreateDirectory( path );
        }

        [Test]
        public void GetFilesTest()
        {
            TestHelper.CleanupTestDir();
            DirectoryInfo testFolder = TestHelper.TestFolderDir;

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

            TestHelper.CleanupTestDir();

            CreateFiles( testFolder.FullName, "az.gif", "rty.jpg", "arty.gif", "raz.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "*.jpg;*.gif" ), "az.gif", "rty.jpg", "arty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "a*.gif" ), "az.gif", "arty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "r*.*" ), "rty.jpg", "raz.png" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "r*.*;a*.gif" ), "raz.png", "az.gif", "rty.jpg", "arty.gif" );
            AssertContains( testFolder.FullName, FileUtil.GetFiles( testFolder.FullName, "r*.png" ), "raz.png" );

            TestHelper.CleanupTestDir();
        }

        [Test]
        public void WaitForWriteAccessTest()
        {
            Assert.Throws<ArgumentNullException>( () => FileUtil.WaitForWriteAcccess( null, 0 ) );

            TestHelper.CleanupTestDir();

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

            TestHelper.CleanupTestDir();
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
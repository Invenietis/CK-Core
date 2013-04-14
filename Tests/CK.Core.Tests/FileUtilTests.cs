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
using System.Threading.Tasks;
using CK.Core.Tests;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class FileUtilTests
    {
        readonly DirectoryInfo _testFolderInfo = TestHelper.TestFolderDir;

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
            DirectoryInfo copyDir = TestHelper.CopyFolderDir;

            TestHelper.CleanupTestDir();
            TestHelper.CleanupCopyDir();

            CreateFiles( _testFolderInfo.FullName, "azerty.png" );
            CreateHiddenFiles( _testFolderInfo.FullName, "hiddenAzerty.gif" );

            FileUtil.CopyDirectory( _testFolderInfo, copyDir );
            AssertContains( _testFolderInfo.FullName, Directory.GetFiles( _testFolderInfo.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png", "hiddenAzerty.gif" );

            Assert.Throws<IOException>( () => FileUtil.CopyDirectory( _testFolderInfo, copyDir ) );

            TestHelper.CleanupCopyDir();

            FileUtil.CopyDirectory( _testFolderInfo, copyDir, false );
            AssertContains( _testFolderInfo.FullName, Directory.GetFiles( _testFolderInfo.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );

            TestHelper.CleanupCopyDir();


            DirectoryInfo recursiveDir = Directory.CreateDirectory( _testFolderInfo.FullName + "//recursiveDir" );
            CreateFiles( recursiveDir.FullName, "REC.png" );
            CreateHiddenFiles( recursiveDir.FullName, "hiddenREC.gif" );

            FileUtil.CopyDirectory( _testFolderInfo, copyDir );
            AssertContains( _testFolderInfo.FullName, Directory.GetFiles( _testFolderInfo.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( Path.Combine( copyDir.FullName, recursiveDir.Name ), Directory.GetFiles( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), "REC.png", "hiddenREC.gif" );

            TestHelper.CleanupCopyDir();

            recursiveDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileUtil.CopyDirectory( _testFolderInfo, copyDir, false, false );
            AssertContains( _testFolderInfo.FullName, Directory.GetFiles( _testFolderInfo.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            Assert.That( Directory.Exists( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), Is.False );

            TestHelper.CleanupCopyDir();

            recursiveDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileUtil.CopyDirectory( _testFolderInfo, copyDir, false, true );
            AssertContains( _testFolderInfo.FullName, Directory.GetFiles( _testFolderInfo.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            AssertContains( Path.Combine( copyDir.FullName, recursiveDir.Name ), Directory.GetFiles( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), "REC.png" );

            TestHelper.CleanupCopyDir();

            FileUtil.CopyDirectory( _testFolderInfo, copyDir, true, true, a => { return a.Name == "azerty.png"; }, a => { return a.Name != recursiveDir.Name; } );
            AssertContains( _testFolderInfo.FullName, Directory.GetFiles( _testFolderInfo.FullName ), "azerty.png", "hiddenAzerty.gif" );
            AssertContains( recursiveDir.FullName, Directory.GetFiles( recursiveDir.FullName ), "REC.png", "hiddenREC.gif" );
            AssertContains( copyDir.FullName, Directory.GetFiles( copyDir.FullName ), "azerty.png" );
            Assert.That( Directory.Exists( Path.Combine( copyDir.FullName, recursiveDir.Name ) ), Is.False );

            //Exception Test
            Assert.Throws<ArgumentNullException>( () => FileUtil.CopyDirectory( null, _testFolderInfo ) );
            Assert.Throws<ArgumentNullException>( () => FileUtil.CopyDirectory( _testFolderInfo, null) );

            TestHelper.CleanupTestDir();
            TestHelper.CleanupCopyDir();
        }

        [Test]
        public void GetFilesTest()
        {
            TestHelper.CleanupTestDir();

            CreateFiles( _testFolderInfo.FullName, "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.png;*.jpg;*.gif" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "azerty.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "azer*.gif" ), "azerty.gif" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "azer*.*if" ), "azerty.gif" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "azerty.*g" ), "azerty.jpg", "azerty.png" );
                            
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.png;*.jpg" ), "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.png;*.gif" ), "azerty.gif", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.gif" ), "azerty.gif" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, string.Empty ) );
                             
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*;*.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.png;*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.png;*.*" ), "azerty.gif", "azerty.jpg", "azerty.png" );
                            
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, ";;" ) );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, ";*;" ), "azerty.gif", "azerty.jpg", "azerty.png" );
                            
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "a" ) );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "a.z" ) );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "" ) );

            TestHelper.CleanupTestDir();

            CreateFiles( _testFolderInfo.FullName, "az.gif", "rty.jpg", "arty.gif", "raz.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "*.jpg;*.gif" ), "az.gif", "rty.jpg", "arty.gif" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "a*.gif" ), "az.gif", "arty.gif" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "r*.*" ), "rty.jpg", "raz.png" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "r*.*;a*.gif" ), "raz.png", "az.gif", "rty.jpg", "arty.gif" );
            AssertContains( _testFolderInfo.FullName, FileUtil.GetFiles( _testFolderInfo.FullName, "r*.png" ), "raz.png" );

            TestHelper.CleanupTestDir();
        }

        [Test]
        public void WaitForWriteAccessTest()
        {
            TestHelper.CleanupTestDir();

            string path = Path.Combine( _testFolderInfo.FullName, "testWriteAccess" );
            Assert.That( FileUtil.WaitForWriteAcccess( new FileInfo( "Nothing" ), 1 ), Is.True );
            Task.Factory.StartNew( () =>
                {
                    FileStream fs = File.Create( path );
                    System.Threading.Thread.Sleep( 1000 );
                    fs.Close();
                } );
            Assert.That( FileUtil.WaitForWriteAcccess( new FileInfo( path ), 0 ), Is.False );
            Assert.That( FileUtil.WaitForWriteAcccess( new FileInfo( path ), 2 ), Is.True );

            Assert.Throws<NullReferenceException>( () => FileUtil.WaitForWriteAcccess( null, 0 ) );

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
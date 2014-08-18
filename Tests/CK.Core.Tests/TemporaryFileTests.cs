#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\TemporaryFileTests.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [Category("File")]
    public class TemporaryFileTests
    {
        [Test]
        public void TemporaryFileSimpleTest()
        {
            string path = string.Empty;
            using( TemporaryFile temporaryFile = new TemporaryFile( true, null ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );

            using( TemporaryFile temporaryFile = new TemporaryFile() )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );

            using( TemporaryFile temporaryFile = new TemporaryFile( true ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );
        }

        [Test]
        public void TemporaryFileExtensionTest()
        {
            string path = string.Empty;
            using( TemporaryFile temporaryFile = new TemporaryFile( " " ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );

            using( TemporaryFile temporaryFile = new TemporaryFile( true, "." ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( path.EndsWith( ".tmp." ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );

            using( TemporaryFile temporaryFile = new TemporaryFile( true, "tst" ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( path.EndsWith( ".tmp.tst" ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );

            using( TemporaryFile temporaryFile = new TemporaryFile( true, ".tst" ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( path.EndsWith( ".tmp.tst" ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
            }
            Assert.That( File.Exists( path ), Is.False );
        }

        [Test]
        public void TemporaryFileDetachTest()
        {
            string path = string.Empty;
            using( TemporaryFile temporaryFile = new TemporaryFile( true, null ) )
            {
                path = temporaryFile.Path;
                Assert.That( File.Exists( temporaryFile.Path ), Is.True );
                Assert.That( (File.GetAttributes( temporaryFile.Path ) & FileAttributes.Temporary) == FileAttributes.Temporary, Is.True );
                temporaryFile.Detach();
            }
            Assert.That( File.Exists( path ), Is.True );
            File.Delete( path );
        }
    }
}

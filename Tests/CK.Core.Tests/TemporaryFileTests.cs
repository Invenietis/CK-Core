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

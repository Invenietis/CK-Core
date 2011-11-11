using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class FileUtilTests
    {
        [Test]
        public void PathNormalization()
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

    }
}

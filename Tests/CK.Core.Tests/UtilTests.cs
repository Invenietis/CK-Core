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

    [TestFixture]
    public class UtilTests
    {
        [TestCase( 0 )]
        [TestCase( 1 )]
        [TestCase( 2 )]
        [TestCase( 3 )]
        [TestCase( 4 )]
        [TestCase( 5 )]
        [TestCase( 50 )]
        [TestCase( 3710 )]
        [TestCase( 3711 )]
        [TestCase( 3712 )]
        [TestCase( 3713 )]
        [TestCase( 3714 )]
        [TestCase( 3715 )]
        public void GetRandomBase64UrlString( int len )
        {
            Util.GetRandomBase64UrlString( len ).Length.Should().Be( len );
        }

        [Test]
        public void compute_sqlserver_epoch_ticks()
        {
            var t = Util.SqlServerEpoch;
            t.Ticks.Should().Be( new DateTime(1900,1,1).Ticks );
            t.Kind.Should().Be( DateTimeKind.Unspecified );
        }
    }
}

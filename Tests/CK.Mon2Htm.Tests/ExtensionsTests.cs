using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;

namespace CK.Mon2Htm.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "HtmlGenerator" )]
    public class ExtensionsTests
    {
        [Test]
        public void DateTimeStampToAndFromBytes()
        {
            DateTimeStamp t = new DateTimeStamp( DateTime.UtcNow, GetRandomByte() );

            byte[] bytes = t.ToBytes();

            Assert.That( bytes.Length == 9 );

            DateTimeStamp t2 = MonitoringExtensions.CreateDateTimeStampFromBytes( bytes );

            Assert.That( t == t2 );

            Assert.Throws( typeof( ArgumentException ), () => { MonitoringExtensions.CreateDateTimeStampFromBytes( BitConverter.GetBytes( DateTime.MaxValue.ToBinary() ) ); } );
        }

        [Test]
        public void DateTimeStampToAndFromBase64String()
        {
            DateTimeStamp t = new DateTimeStamp( DateTime.UtcNow, GetRandomByte() );

            string s = t.ToBase64String();

            DateTimeStamp t2 = MonitoringExtensions.CreateDateTimeStampFromBase64( s );

            Assert.That( t == t2 );
        }

        private static byte GetRandomByte()
        {
            Random random = new Random();
            byte[] bytes = new byte[1];
            random.NextBytes( bytes );

            return bytes[0];
        }
    }
}

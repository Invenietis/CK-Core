using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class UriHelperTests
    {

        void TestAssume( string result, string u, string param, string newValue )
        {
            Assert.AreEqual( result, UriHelper.AssumeUrlParameter( u, param, newValue ) );
            string prefix = "http://";
            Assert.AreEqual( new Uri( prefix + result ), new Uri( prefix + u ).AssumeUrlParameter( param, newValue ) );
        }

        [Test]
        public void AssumeUrlParameter()
        {
            TestAssume( "test.com?kilo=1", "test.com", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?", "kilo", "1" );
            TestAssume( "test.com?a=1&z=2&kilo=1", "test.com?a=1&z=2", "kilo", "1" );
            TestAssume( "test.com?a=kilo&kilo=1", "test.com?a=kilo", "kilo", "1" );
            TestAssume( "test.com?kilo=1&z=2", "test.com?kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?kilo=1&z=2", "test.com?kilo=kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?kilo=1&ki=1", "test.com?kilo=&ki=1", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?kilo=j", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?kilo", "kilo", "1" );
            TestAssume( "test.com?kilo=1", "test.com?kilo=", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1&z=2", "test.com?a=z&kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1&z=2", "test.com?a=z&kilo=kilo&z=2", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1&ki=1", "test.com?a=z&kilo=&ki=1", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1", "test.com?a=z&kilo=j", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1", "test.com?a=z&kilo", "kilo", "1" );
            TestAssume( "test.com?a=z&kilo=1", "test.com?a=z&kilo=", "kilo", "1" );

            Assert.That( UriHelper.RemoveUrlParameter( "test.com?a=z&kilo=1", "a" ) == "test.com?kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?a=z&kilo=1", "kilo" ) == "test.com?a=z" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?a=z&toto=6&kilo=1", "toto" ) == "test.com?a=z&kilo=1" );

            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "a" ) == "test.com?&kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "kilo" ) == "test.com?&a=z" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&toto=6&kilo=1", "toto" ) == "test.com?&a=z&kilo=1" );

            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "edhoe" ) == "test.com?&a=z&kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", "" ) == "test.com?&a=z&kilo=1" );
            Assert.That( UriHelper.RemoveUrlParameter( "test.com?&a=z&kilo=1", null ) == "test.com?&a=z&kilo=1" );
        }

    }
}

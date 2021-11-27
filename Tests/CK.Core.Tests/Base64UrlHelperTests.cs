using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class Base64UrlHelperTests
    {
        [TestCase( 0 )]
        [TestCase( 1 )]
        [TestCase( 2 )]
        [TestCase( 3 )]
        [TestCase( 4 )]
        [TestCase( 257 )]
        [TestCase( 3712 )]
        [TestCase( 5454 )]
        public void convert_tests( int size )
        {
            var bytes = RandomNumberGenerator.GetBytes( size );
            var s = Base64UrlHelper.ToBase64UrlString( bytes );
            var back = Base64UrlHelper.FromBase64UrlString( s );
            back.ToArray().Should().BeEquivalentTo( bytes );
        }

        [TestCase( "°" )]
        [TestCase( "+" )]
        [TestCase( "/" )]
        [TestCase( "YQ==" )]
        public void invalid_base64UrlString_must_throw( string s )
        {
            FluentActions.Invoking( () => Base64UrlHelper.FromBase64UrlString( s ) ).Should().Throw<ArgumentException>();
        }

    }
}

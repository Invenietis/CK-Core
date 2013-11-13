using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    [Category("String.Util")]
    public class StringUtilTests
    {
        [Test]
        public void TestNamedFormat()
        {
            string format = "Hi {{username}}, my name is {{me}}. How are you ? If you want to contact me just send me an email to {{me}}@gmail.com. Bye !";
            string formatted = Util.String.NamedFormat( format, new { username = "somerandomuser", me = "thesendername" } );

            Assert.That( formatted, Is.EqualTo( "Hi somerandomuser, my name is thesendername. How are you ? If you want to contact me just send me an email to thesendername@gmail.com. Bye !" ) );
        }

        [Test]
        public void TestWrongNamedFormat()
        {
            var o = new { };

            Assert.Throws<ArgumentException>( () =>
            {
                Util.String.NamedFormat( "Hi {{0wrongname}}", o );
            } );
            Assert.Throws<ArgumentException>( () =>
            {
                Util.String.NamedFormat( "Hi {{wrong name}}", o );
            } );
            Assert.Throws<ArgumentException>( () =>
            {
                Util.String.NamedFormat( "Hi {{wrong-name}}", o );
            } );
            Assert.Throws<ArgumentException>( () =>
            {
                Util.String.NamedFormat( "Hi {{}}", o );
            } );
        }

        [Test]
        public void TestNullValueObject()
        {
            Assert.Throws<ArgumentNullException>( () =>
            {
                Util.String.NamedFormat( "Hi {{name}}", null );
            } );
        }

        [Test]
        public void TestMissingProperty()
        {
            Assert.Throws<ArgumentException>( () =>
            {
                Util.String.NamedFormat( "Hi {{name}}", new { propName = "toto" } );
            } );
        }
    }
}

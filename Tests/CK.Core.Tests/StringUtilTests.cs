using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [Test]
        public void PerfTest()
        {
            Stopwatch sw = new Stopwatch();

            string namedFormat = "aaa {{x}} bbb {{y}} ccc {{x}} ddd";
            string format = "aaa {0} bbb {1} ccc {0} ddd";
            object values = new { x = "XXX", y = "YYY" };

            long normalTime = 0;
            long namedTime = 0;
            
            // first run not measured
            string.Format( format, "XXX", "YYY" );
            Util.String.NamedFormat( namedFormat, values );

            sw.Start();
            for( int i = 0; i < 100000; i++ )
            {
                string.Format( format, "XXX", "YYY" );
            }
            sw.Stop();
            normalTime = sw.ElapsedMilliseconds;
            Console.WriteLine( "Normal format time : {0} ms", normalTime );
            
            sw.Restart();
            for( int i = 0; i < 100000; i++ )
            {
                Util.String.NamedFormat( namedFormat, values );
            }
            sw.Stop();
            namedTime = sw.ElapsedMilliseconds;
            Console.WriteLine( "Named format time : {0} ms", namedTime );
        }
    }
}

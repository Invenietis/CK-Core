using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class CriticalErrorCollectorTests
    {

        [Test]
        public void ValidArguments()
        {
            CriticalErrorCollector c = new CriticalErrorCollector();
            Assert.Throws<ArgumentNullException>( () => c.Add( null, "" ) );
            Assert.DoesNotThrow( () => c.Add( new Exception( "A" ), null ) );
            c.Add( new Exception( "B" ), "Comment" );
            
            var errors = c.ToArray();
            Assert.That( errors[0].ToString(), Is.EqualTo( " - A" ) );
            Assert.That( errors[1].ToString(), Is.EqualTo( "Comment - B" ) );
        }

    }
}

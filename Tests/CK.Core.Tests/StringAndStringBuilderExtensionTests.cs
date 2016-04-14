using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class StringAndStringBuilderExtensionTests
    {
        [Test]
        public void concat_method_uses_StringBuilder_AppendStrings_inside()
        {
            var strings = new string[] { "A", "Hello", "B", "World", null, "End" };
            var s = strings.Concatenate( "|+|" );
            Assert.That( s, Is.EqualTo( "A|+|Hello|+|B|+|World|+||+|End" ) );
        }

        [Test]
        public void StringBuilder_AppendStrings_method_does_not_skip_null_entries()
        {
            var strings = new string[] { "A", "Hello", "B", "World", null, "End" };
            var b = new StringBuilder();
            b.AppendStrings( strings, "|+|" );
            Assert.That( b.ToString(), Is.EqualTo( "A|+|Hello|+|B|+|World|+||+|End" ) );
        }

    }
}

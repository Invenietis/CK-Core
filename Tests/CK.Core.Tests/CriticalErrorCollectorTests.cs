using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CK.Core.Tests
{

    public class CriticalErrorCollectorTests
    {

        [Fact]
        public void simple_add_exception_to_CriticalErrorCollector()
        {
            CriticalErrorCollector c = new CriticalErrorCollector();
            Should.Throw<ArgumentNullException>(() => c.Add(null, ""));
            c.Add(new Exception("A"), null);
            c.Add(new Exception("B"), "Comment");

            var errors = c.ToArray();
            errors[0].ToString().Should().Be(" - A");
            errors[1].ToString().Should().Be("Comment - B");
        }

    }
}

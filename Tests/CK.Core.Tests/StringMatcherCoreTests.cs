using CK.Text;
using FluentAssertions;
using System;
using NUnit.Framework;

namespace CK.Core.Tests
{

    public class StringMatcherCoreTests
    {
        [Test]
        public void matching_FileNameUniqueTimeUtcFormat()
        {
            DateTime t = DateTime.UtcNow;
            string s = t.ToString(FileUtil.FileNameUniqueTimeUtcFormat);
            var m = new StringMatcher("X" + s + "Y");
            m.MatchChar('X').Should().BeTrue();
            DateTime parsed;
            m.MatchFileNameUniqueTimeUtcFormat(out parsed).Should().BeTrue();
            parsed.Should().Be(t);
            m.MatchChar('Y').Should().BeTrue();

            m = new StringMatcher(s.Insert(2, "X"));
            m.MatchFileNameUniqueTimeUtcFormat(out parsed).Should().BeFalse();
            int i;
            m.MatchInt32(out i).Should().BeTrue();
            i.Should().Be(20);
        }

        [Test]
        public void matching_DateTimeStamp()
        {
            DateTimeStamp t = DateTimeStamp.UtcNow;
            CheckDateTimeStamp(t);
            CheckDateTimeStamp(new DateTimeStamp(t.TimeUtc, 67));
        }

        private static void CheckDateTimeStamp(DateTimeStamp t)
        {
            string s = t.ToString();
            var m = new StringMatcher("X" + s + "Y");
            m.MatchChar('X').Should().BeTrue();
            DateTimeStamp parsed;
            m.MatchDateTimeStamp(out parsed).Should().BeTrue();
            parsed.Should().Be(t);
            m.MatchChar('Y').Should().BeTrue();

            m = new StringMatcher(s.Insert(2, "X"));
            m.MatchDateTimeStamp(out parsed).Should().BeFalse();
            m.ErrorMessage.Should().NotBeNull();
            int i;
            m.MatchInt32(out i).Should().BeTrue();
            i.Should().Be(20);
        }

        public void match_methods_must_set_an_error()
        {
            var m = new StringMatcher("A");

            DateTimeStamp ts;
            CheckMatchError(m, () => m.MatchDateTimeStamp(out ts));
            DateTime dt;
            CheckMatchError(m, () => m.MatchFileNameUniqueTimeUtcFormat(out dt));
            CheckMatchError(m, () => m.MatchText("B"));
        }

        private static void CheckMatchError(StringMatcher m, Func<bool> fail)
        {
            int idx = m.StartIndex;
            int len = m.Length;
            fail().Should().BeFalse();
            m.IsError.Should().BeTrue();
            m.ErrorMessage.Should().NotBeNullOrEmpty();
            m.StartIndex.Should().Be(idx, "Head must not move on error.");
            m.Length.Should().Be(len, "Length must not change on error.");
            m.ClearError();
        }
    }
}

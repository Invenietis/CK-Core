
using FluentAssertions;
using System;

namespace CK.Reflection.Tests
{
    public static class Should
    {
        public static void Throw<T>(Action a) where T : Exception => a.ShouldThrow<T>();
    }


#if !CSPROJ
    class TestAttribute : Xunit.FactAttribute
    {
    }
#endif

#if !NET451
    class  ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
#endif
}


using System;

namespace CK.Reflection.Tests
{
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

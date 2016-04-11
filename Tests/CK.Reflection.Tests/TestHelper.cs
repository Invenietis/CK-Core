
using System;

namespace CK.Reflection.Tests
{
    class TestAttribute : Xunit.FactAttribute
    {
    }

#if !NET451    
    class  ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
#endif
}

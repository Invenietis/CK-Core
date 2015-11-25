using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if !NET451 && !NET46 

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Internal empty stub for .Net core.
    /// </summary>
    class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
}

#endif
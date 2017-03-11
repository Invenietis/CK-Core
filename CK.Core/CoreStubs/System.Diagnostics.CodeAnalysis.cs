using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NETSTANDARD1_3 || NETSTANDARD1_6

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Internal empty stub for .Net core.
    /// </summary>
    public class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
}

#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Runtime.CompilerServices
{
    //This attribute is used to enable the CallerMemberName feature on a .NET 4.0 solution.
    //Make sure you are using VS 2012 or higher. If not, CallerMemberName won't inject the name of the caller where it is used.
#if NET40
    [AttributeUsage( AttributeTargets.Parameter, AllowMultiple = false, Inherited = true )]
    public sealed class CallerMemberNameAttribute : Attribute
    {
    }
#endif
}

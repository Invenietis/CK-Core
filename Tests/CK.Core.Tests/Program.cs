using NUnitLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    public static class Program
    {
        public static int Main( string[] arguments )
        {
            //Debugger.Launch();
            int result;
#if DNX451 || DNX46
            result = new AutoRun().Execute( arguments );
#else
            result = new AutoRun().Execute( typeof(Program).GetTypeInfo().Assembly, Console.Out, Console.In, arguments );
#endif
            return result;
        }
    }
}

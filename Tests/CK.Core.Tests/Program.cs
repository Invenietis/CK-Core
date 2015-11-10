using NUnitLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    public class Program
    {
        public int Main( string[] arguments )
        {
            int result;
#if DNX451
            result = new AutoRun().Execute( arguments );
#else
            var sw = new StringWriter();
            var sr = new StringReader("");
            result = new AutoRun().Execute( typeof(Program).GetTypeInfo().Assembly, sw, sr, arguments );
#endif
            return result;
        }
    }
}

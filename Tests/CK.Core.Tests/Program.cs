using NUnitLite;
using System.Reflection;

namespace CK.StObj.Engine.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new AutoRun(Assembly.GetEntryAssembly()).Execute(args);
        }
    }
}

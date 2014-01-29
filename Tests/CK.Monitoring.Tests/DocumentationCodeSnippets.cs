using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    public class DocumentationCodeSnippets
    {
        public void CodeConfiguration()
        {
            CK.Core.SystemActivityMonitor.RootLogPath = @"C:\Test\Logs";
            CK.Monitoring.GrandOutput.EnsureActiveDefaultWithDefaultSettings();
        }


    }
}

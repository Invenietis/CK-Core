using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Resources;
using System.Reflection;
using CK.Context;
using CK.Storage;
using System.Xml;

namespace PluginConfig
{
    [TestFixture]
    public class ReadFiles : TestBase
    {
        [Test]
        public void ReadSystemConfig()
        {
            IContext c = CreateContext();
            using( var s = TestBase.OpenFileResourceStream( "System.config.ck" ) )
            using( var reader = SimpleStructuredReader.CreateReader( s, c ) )
            {
                Assert.Throws<XmlException>( () => c.ConfigManager.Extended.LoadSystemConfig( reader ) );
            }
        }

    }
}

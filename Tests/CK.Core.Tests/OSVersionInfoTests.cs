using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class OSVersionInfoTests
    {
        [Test]
        public void OSVersionInfoTest()
        {
            object o = null;
            Assert.DoesNotThrow( () => o = OSVersionInfo.OSVersion.ToString() );
            Assert.DoesNotThrow( () => o = OSVersionInfo.OSVersion.ServicePack );
            Assert.DoesNotThrow( () => o = OSVersionInfo.ProcessBits );
            Assert.DoesNotThrow( () => o = OSVersionInfo.OSBits );
            Assert.DoesNotThrow( () => o = OSVersionInfo.ProcessorBits );
            Assert.DoesNotThrow( () => o = OSVersionInfo.OSLevel );
            Assert.DoesNotThrow( () => o = OSVersionInfo.OSLevelDisplayName );
        }
    }
}

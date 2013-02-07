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
            Assert.DoesNotThrow( () => o = OSVersionInfo.ProgramBits );
            Assert.DoesNotThrow( () => o = OSVersionInfo.OSBits );
            Assert.DoesNotThrow( () => o = OSVersionInfo.ProcessorBits );
            Assert.DoesNotThrow( () => o = OSVersionInfo.Edition );
            Assert.DoesNotThrow( () => o = OSVersionInfo.Name );
            Assert.DoesNotThrow( () => o = OSVersionInfo.ServicePack );
            Assert.DoesNotThrow( () => o = OSVersionInfo.IsWindowsVistaOrGreater );
            Assert.DoesNotThrow( () => o = OSVersionInfo.BuildVersion );
            Assert.DoesNotThrow( () => o = OSVersionInfo.VersionString );
            Assert.DoesNotThrow( () => o = OSVersionInfo.Version );
            Assert.DoesNotThrow( () => o = OSVersionInfo.MajorVersion );
            Assert.DoesNotThrow( () => o = OSVersionInfo.MinorVersion );
            Assert.DoesNotThrow( () => o = OSVersionInfo.RevisionVersion );
        }
    }
}

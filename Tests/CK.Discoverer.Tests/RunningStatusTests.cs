using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Plugin;

namespace Discoverer
{
    [TestFixture]
    public partial class RunningStatusTests
    {
        [Test]
        public void LighterAndGreaterTests()
        {
            //-- Disabled
            Assert.That( RunningStatus.Disabled < RunningStatus.Stopped );
            Assert.That( RunningStatus.Disabled < RunningStatus.Starting );
            Assert.That( RunningStatus.Disabled < RunningStatus.Stopping );
            Assert.That( RunningStatus.Disabled < RunningStatus.Started );

            Assert.False( RunningStatus.Disabled > RunningStatus.Stopped );
            Assert.False( RunningStatus.Disabled > RunningStatus.Starting );
            Assert.False( RunningStatus.Disabled > RunningStatus.Stopping );
            Assert.False( RunningStatus.Disabled > RunningStatus.Started );

            //-- Stopped
            Assert.False( RunningStatus.Stopped < RunningStatus.Disabled );
            Assert.That( RunningStatus.Stopped < RunningStatus.Started );
            Assert.That( RunningStatus.Stopped < RunningStatus.Starting );
            Assert.That( RunningStatus.Stopped < RunningStatus.Stopping );

            Assert.That( RunningStatus.Stopped > RunningStatus.Disabled );
            Assert.False( RunningStatus.Stopped > RunningStatus.Started );
            Assert.False( RunningStatus.Stopped > RunningStatus.Starting );
            Assert.False( RunningStatus.Stopped > RunningStatus.Stopping );

            //--Stopping
            Assert.False( RunningStatus.Stopping < RunningStatus.Disabled );
            Assert.False( RunningStatus.Stopping < RunningStatus.Stopped );
            Assert.False( RunningStatus.Stopping < RunningStatus.Starting );
            Assert.That( RunningStatus.Stopping < RunningStatus.Started );

            Assert.That( RunningStatus.Stopping > RunningStatus.Disabled );
            Assert.That( RunningStatus.Stopping > RunningStatus.Stopped );
            Assert.False( RunningStatus.Stopping > RunningStatus.Starting );
            Assert.False( RunningStatus.Stopping > RunningStatus.Started );

            //-- Starting
            Assert.False( RunningStatus.Starting < RunningStatus.Disabled );
            Assert.False( RunningStatus.Starting < RunningStatus.Stopped );
            Assert.False( RunningStatus.Starting > RunningStatus.Stopping );
            Assert.That( RunningStatus.Starting < RunningStatus.Started );

            Assert.That( RunningStatus.Starting > RunningStatus.Disabled );
            Assert.That( RunningStatus.Starting > RunningStatus.Stopped );
            Assert.False( RunningStatus.Starting > RunningStatus.Stopping );
            Assert.False( RunningStatus.Starting > RunningStatus.Started );

            //--Started
            Assert.That( RunningStatus.Started > RunningStatus.Disabled );
            Assert.That( RunningStatus.Started > RunningStatus.Stopped );
            Assert.That( RunningStatus.Started > RunningStatus.Starting );
            Assert.That( RunningStatus.Started > RunningStatus.Stopping );

            Assert.False( RunningStatus.Started < RunningStatus.Disabled );
            Assert.False( RunningStatus.Started < RunningStatus.Stopped );
            Assert.False( RunningStatus.Started < RunningStatus.Starting );
            Assert.False( RunningStatus.Started < RunningStatus.Stopping );  
        }
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\RunningStatusTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
#pragma warning disable 1718

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

        [Test]
        public void LighterAndGreaterOrEqualTests()
        {
            //-- Disabled
            Assert.That( RunningStatus.Disabled <= RunningStatus.Disabled );
            Assert.That( RunningStatus.Disabled <= RunningStatus.Stopped );
            Assert.That( RunningStatus.Disabled <= RunningStatus.Starting );
            Assert.That( RunningStatus.Disabled <= RunningStatus.Stopping );
            Assert.That( RunningStatus.Disabled <= RunningStatus.Started );

            Assert.That( RunningStatus.Disabled >= RunningStatus.Disabled );
            Assert.False( RunningStatus.Disabled >= RunningStatus.Stopped );
            Assert.False( RunningStatus.Disabled >= RunningStatus.Starting );
            Assert.False( RunningStatus.Disabled >= RunningStatus.Stopping );
            Assert.False( RunningStatus.Disabled >= RunningStatus.Started );

            //-- Stopped
            Assert.That( RunningStatus.Stopped <= RunningStatus.Stopped );
            Assert.False( RunningStatus.Stopped <= RunningStatus.Disabled );
            Assert.That( RunningStatus.Stopped <= RunningStatus.Started );
            Assert.That( RunningStatus.Stopped <= RunningStatus.Starting );
            Assert.That( RunningStatus.Stopped <= RunningStatus.Stopping );

            Assert.That( RunningStatus.Stopped >= RunningStatus.Stopped );
            Assert.That( RunningStatus.Stopped >= RunningStatus.Disabled );
            Assert.False( RunningStatus.Stopped >= RunningStatus.Started );
            Assert.False( RunningStatus.Stopped >= RunningStatus.Starting );
            Assert.False( RunningStatus.Stopped >= RunningStatus.Stopping );

            //--Stopping
            Assert.That( RunningStatus.Stopping <= RunningStatus.Stopping );
            Assert.False( RunningStatus.Stopping <= RunningStatus.Disabled );
            Assert.False( RunningStatus.Stopping <= RunningStatus.Stopped );
            Assert.False( RunningStatus.Stopping <= RunningStatus.Starting );
            Assert.That( RunningStatus.Stopping <= RunningStatus.Started );

            Assert.That( RunningStatus.Stopping >= RunningStatus.Stopping );
            Assert.That( RunningStatus.Stopping >= RunningStatus.Disabled );
            Assert.That( RunningStatus.Stopping >= RunningStatus.Stopped );
            Assert.False( RunningStatus.Stopping >= RunningStatus.Starting );
            Assert.False( RunningStatus.Stopping >= RunningStatus.Started );

            //-- Starting
            Assert.That( RunningStatus.Starting <= RunningStatus.Starting );
            Assert.False( RunningStatus.Starting <= RunningStatus.Disabled );
            Assert.False( RunningStatus.Starting <= RunningStatus.Stopped );
            Assert.False( RunningStatus.Starting >= RunningStatus.Stopping );
            Assert.That( RunningStatus.Starting <= RunningStatus.Started );

            Assert.That( RunningStatus.Starting >= RunningStatus.Starting );
            Assert.That( RunningStatus.Starting >= RunningStatus.Disabled );
            Assert.That( RunningStatus.Starting >= RunningStatus.Stopped );
            Assert.False( RunningStatus.Starting >= RunningStatus.Stopping );
            Assert.False( RunningStatus.Starting >= RunningStatus.Started );

            //--Started
            Assert.That( RunningStatus.Started >= RunningStatus.Started );
            Assert.That( RunningStatus.Started >= RunningStatus.Disabled );
            Assert.That( RunningStatus.Started >= RunningStatus.Stopped );
            Assert.That( RunningStatus.Started >= RunningStatus.Starting );
            Assert.That( RunningStatus.Started >= RunningStatus.Stopping );

            Assert.That( RunningStatus.Started <= RunningStatus.Started );
            Assert.False( RunningStatus.Started <= RunningStatus.Disabled );
            Assert.False( RunningStatus.Started <= RunningStatus.Stopped );
            Assert.False( RunningStatus.Started <= RunningStatus.Starting );
            Assert.False( RunningStatus.Started <= RunningStatus.Stopping );
        }
#pragma warning restore 1718
    }
}

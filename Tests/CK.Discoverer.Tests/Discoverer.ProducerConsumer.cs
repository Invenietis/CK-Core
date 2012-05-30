#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Discoverer.ProducerConsumer.cs) is part of CiviKey. 
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

using System.Collections.Generic;
using NUnit.Framework;
using CK.Plugin;
using System.Reflection;
using System.IO;
using System.Linq;
using CK.Core;
using CK.Plugin.Discoverer;
using System;

namespace Discoverer
{
	public partial class Discoverer
	{
        [Test]
        public void TestConsumers()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();

            TestBase.CopyPluginToTestDir( "ServiceProducer.Model.dll" );
            TestBase.CopyPluginToTestDir( "ServiceProducer.dll" );
            TestBase.CopyPluginToTestDir( "ServiceConsumer.dll" );
            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.AllAssemblies.Count, Is.EqualTo( 3 ) );
            Assert.That( discoverer.AllPlugins.Count, Is.EqualTo( 3 ) );

            IPluginInfo c0 = discoverer.FindPlugin( new Guid("{2294F5BD-C511-456F-8E6B-A39A84FBAE51}") );

        }
	}
}

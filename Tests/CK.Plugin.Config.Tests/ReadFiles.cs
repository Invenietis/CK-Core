#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Config.Tests\ReadFiles.cs) is part of CiviKey. 
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

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\Injection\Plugin01.cs) is part of CiviKey. 
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
using CK.Plugin;
using CK.Plugin.Config;
using NUnit.Framework;

namespace Injection
{
    [Plugin( "{7E0A35E0-0A49-461A-BDC7-7C0083CC5DC9}" )]
    public class Plugin01 : IPlugin
    {
        bool _serviceHasBeenStopped;
        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires=RunningRequirement.MustExistTryStart )]
        public IService<IService02> ServiceWrapped { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            Assert.That( Configuration != null );
            Assert.That( ServiceWrapped == null );
            return true;
        }

        public void Start()
        {
            Assert.That( ServiceWrapped != null );
            Assert.That( ServiceWrapped.Status == RunningStatus.Started );

            IObjectPluginConfig config = Configuration[ServiceWrapped.Service.SomeObject];
            Assert.That( (string)config["testKey"] == "testValue" );

            string key = "newKey";
            object value = "newValue";

            ServiceWrapped.Service.UpdateEditedConfig( key, value );

            Assert.That( config[key] == value );

            ServiceWrapped.ServiceStatusChanged += ( o, e ) =>
            {
                _serviceHasBeenStopped = true;
            };
        }

        public void Teardown()
        {
            Assert.That( _serviceHasBeenStopped );
        }

        public void Stop()
        {
            
        }
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\Injection\Plugin02.cs) is part of CiviKey. 
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
    public interface IService02 : IDynamicService
    {
        object SomeObject { get; }
        int SomeMethod( int i );

        void UpdateEditedConfig( string key, object value );
    }

    [Plugin( "{0BE75C1D-BDAD-4782-9D47-95D91EF828D4}" )]
    public class Plugin02 : IPlugin, IService02
    {
        object _obj;
        IObjectPluginConfig _config;

        public IPluginConfigAccessor Configuration { get; set; }

        [RequiredService]
        public IConfigContainer ConfigContainer { get; set; }

        [RequiredService( Required = false )]
        public IConfigContainer OptionalContainer { get; set; }

        [ConfigurationAccessor( "{7E0A35E0-0A49-461A-BDC7-7C0083CC5DC9}" )]
        public IPluginConfigAccessor EditedConfiguration { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            _obj = new object();

            Assert.That( Configuration != null );
            Assert.That( EditedConfiguration != null );

            _config = EditedConfiguration[this.SomeObject];
            UpdateEditedConfig( "testKey", "testValue" );

            return true;
        }

        public void Start()
        {
            Assert.That( ConfigContainer != null );
            Assert.That( OptionalContainer != null );
        }

        public void Teardown()
        {
            
        }

        public void Stop()
        {
            
        }

        #region Service02 Members

        public int SomeMethod( int i )
        {
            return i;
        }

        public object SomeObject { get { return _obj; } }

        public void UpdateEditedConfig(string key, object value)
        {
            _config[key] = value;
        }

        #endregion
    }
}

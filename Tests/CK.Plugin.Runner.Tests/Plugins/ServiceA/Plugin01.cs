#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\ServiceA\Plugin01.cs) is part of CiviKey. 
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
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Core;

namespace CK.Tests.Plugin
{
	[Plugin( Plugin01.PluginIdString, Version=Plugin01.PluginIdVersion, PublicName=PluginPublicName, 
        Categories = new string[] { "Advanced", "Test" },
        IconUri="Resources/test.png",
        RefUrl="http://www.testUrl.com" )]
	public class Plugin01 : IPlugin, IServiceA
	{
        const string PluginIdString = "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}";
        const string PluginIdVersion = "1.1.0";
        const string PluginPublicName = "Plugin01";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

		bool _started;

		public bool CanStart( out string message )
		{
			message = null;
			return true;
		}

		public void Start()
		{
			_started = true;
		}

		public void Stop()
		{
			_started = false;
		}

        public IPluginConfigAccessor Configuration { get; set; }

		#region IServiceA Members

		public bool HasBeenStarted 
		{
			get { return _started; }
		}

		public int Add( int a, int b )
		{
			return a + b;
		}

        public event EventHandler<EventArgs> HasStarted;

		#endregion

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            
        }

        #endregion

        public void Method()
        {

        }

    }
}

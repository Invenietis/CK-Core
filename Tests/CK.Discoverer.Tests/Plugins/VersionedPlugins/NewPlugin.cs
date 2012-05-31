#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Plugins\VersionedPlugins\NewPlugin.cs) is part of CiviKey. 
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

namespace CK.Tests.Plugin
{
    [Plugin( "{B66A9240-D40C-4a7a-912C-E97E745571F8}",
        PublicName = "VersionnedPlugin",
        Categories = new string[] { "Advanced" },
        Version = "2.0.0" )]
	public class NewPlugin : IPlugin
	{
		bool _started;

        //public ICVKLivePluginProxy Proxy { get; set; }

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

		#region IServiceA Members

		public bool HasBeenStarted 
		{
			get { return _started; }
		}

		public int Add( int a, int b )
		{
			return a + b;
		}

		#endregion

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            throw new NotImplementedException();
        }

        public void Teardown()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

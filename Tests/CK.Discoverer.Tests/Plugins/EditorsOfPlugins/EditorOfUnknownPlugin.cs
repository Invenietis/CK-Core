#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Plugins\EditorsOfPlugins\EditorOfUnknownPlugin.cs) is part of CiviKey. 
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

namespace CK.Tests.Plugin.EditorsOfPlugins
{
    [Plugin( "{84EA06CD-9E2D-4656-9BFE-88B1173D2806}", PublicName = "EditorOfUnknownPlugin", Categories = new string[] { "Test", "MySuperEditor", "Other", "Advanced" }, Version = "1.1.0")]
	public class EditorOfUnknownPlugin : IPlugin
	{
        //public ICVKLivePluginProxy Proxy { get; set; }

        [ConfigurationAccessor( "{B2E39E6E-B875-4715-B0E6-4943A27FC403}" )]
        public IPluginConfigAccessor Configuration { get; set; }

		public bool CanStart( out string message )
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

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

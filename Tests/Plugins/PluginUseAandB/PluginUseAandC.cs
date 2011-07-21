#region LGPL License
/*----------------------------------------------------------------------------
* This file (TestPlugins\PluginUseAandB\PluginUseAandC.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
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
	[Plugin( Id="{2D53F27E-9F5E-4bbf-80D8-8BFFCFACD3AA}", 
        PublicName="PluginUseAandC", 
        Categories=new string[] { "Test" },
        DefaultPluginStatus = ConfigPluginStatus.Manual,
        Version="1.1.0" )]
	public class PluginUseAandC : IPlugin
	{
		[Service( Requires=RunningRequirement.MustExistAndRun )]
		public IServiceA ServiceA { get; set; }

        [Service( Requires = RunningRequirement.OptionalTryStart )]
		public IServiceC ServiceC { get; set; }

		public bool CanStart( out string message )
		{
			message = null;
			return true;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}


        #region IPlugin Membres

        public bool Setup( ISetupInfo info )
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


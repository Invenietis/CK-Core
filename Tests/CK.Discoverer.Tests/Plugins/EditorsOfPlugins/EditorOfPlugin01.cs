#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Plugins\EditorsOfPlugins\EditorOfPlugin01.cs) is part of CiviKey. 
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
	[Plugin( "{8BE8C487-4052-46b0-9225-708BC2A1E033}", PublicName="EditorOfPlugin01", Categories=new string[] { "Advanced", "Test", "Other", "MySuperEditor" }, Version="1.1.0")]
	public class EditorOfPlugin01 : IPlugin
	{
        [DynamicService( Requires = RunningRequirement.Optional )]
        public IServiceA ServiceA { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IServiceA ServiceA1 { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IServiceA ServiceA2 { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IServiceA ServiceA3 { get; set; }

        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IServiceA ServiceA4 { get; set; }

        [ConfigurationAccessor( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" )]
        public IPluginConfigAccessor EditedPluginConfiguration { get; set; }

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

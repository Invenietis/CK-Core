#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Plugins\ServiceC\PluginServiceC.cs) is part of CiviKey. 
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
	[Plugin( "{C24EE3EA-F078-4974-A346-B34208221B35}", 
        PublicName="PluginServiceC", 
        Categories=new string[] { "Test" },
        Version="1.0.0" )]
	public class PluginServiceC : IPlugin, IServiceC
	{
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

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            //Nothing to do.
        }

		string IServiceC.VeryUsefulMethod( string s )
		{
			char[] c = s.ToLowerInvariant().ToCharArray();
			Array.Reverse( c );
			return new string( c );
		}
    }
}

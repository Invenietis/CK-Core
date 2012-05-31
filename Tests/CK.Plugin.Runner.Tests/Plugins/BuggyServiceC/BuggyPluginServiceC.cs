#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\BuggyServiceC\BuggyPluginServiceC.cs) is part of CiviKey. 
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
    [Plugin( "{73FC9CFD-213C-4EC6-B002-452646B9D225}", 
        PublicName="BuggyPluginServiceC", 
        Categories=new string[] { "Test" },
        Version="1.0.0" )]
	public class SetupBuggyPluginServiceC : IPlugin, IServiceC
	{
		public void Start()
		{
            
		}

		public void Stop()
		{
		}

        public bool Setup( IPluginSetupInfo info )
        {
            throw new Exception( "p0wn!" );
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

    [Plugin( "{FFB94881-4F59-4B97-B16E-CF3081A6E668}",
        PublicName = "BuggyPluginServiceC",
        Categories = new string[] { "Test" },
        Version = "1.0.0" )]
    public class StartBuggyPluginServiceC : IPlugin, IServiceC
    {
        public void Start()
        {
            throw new Exception( "p0wn!" );
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

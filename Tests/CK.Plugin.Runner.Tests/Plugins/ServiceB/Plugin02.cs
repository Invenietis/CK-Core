#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\ServiceB\Plugin02.cs) is part of CiviKey. 
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
	[Plugin( "{E64F17D5-DCAB-4A07-8CEE-901A87D8295E}", 
        PublicName="Plugin02", 
        Categories=new string[] { "Advanced", "Test", "Other" },
        Version="1.2.3" )]
	public class Plugin01 : IPlugin, IServiceB
	{
		bool _started;

		public void Start()
		{
			_started = true;
		}

		public void Stop()
		{
			_started = false;
		}

		#region IServiceB Members

		public bool HasBeenStarted
		{
			get { return _started; }
		}

		public int Mult( int a, int b )
		{
			return a * b;
		}

        public int Substract(int a, int b, out bool isAboveZero)
        {
            throw new NotImplementedException();
        }

		#endregion

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        #endregion


        
    }
}

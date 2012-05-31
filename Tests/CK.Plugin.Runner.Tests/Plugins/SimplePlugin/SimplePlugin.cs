#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\SimplePlugin\SimplePlugin.cs) is part of CiviKey. 
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

namespace SimplePlugin
{
    /// <summary>
    /// Simple plugin without any config accessor, service reference, or service implementation.
    /// Used by :
    ///     * CK.Plugin.Runner.Tests.Apply
    /// </summary>
    [Plugin( pluginId, Version=version )]
    public class SimplePlugin : IPlugin
    {
        const string pluginId = "{EEAEC976-2AFC-4A68-BFAD-68E169677D52}";
        const string version = "1.0.0";

        public bool HasBeenSarted { get; private set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            HasBeenSarted = true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        public void Stop()
        {
            HasBeenSarted = false;
        }
    }
}

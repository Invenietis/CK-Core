#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\DiscoveredInfo.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Plugin.Discoverer
{
    internal abstract class DiscoveredInfo : IDiscoveredInfo
    {
        PluginDiscoverer _discoverer;
        string _errorMessage;
        int _version;

        public bool HasError
        {
            get { return ErrorMessage != null && ErrorMessage.Length > 0; }
        }

        public int LastChangedVersion
        {
            get { return _version; }
        }

        public bool HasChanged
        {
            get { return _version == _discoverer.CurrentVersion; }
        }

        public string ErrorMessage 
        { 
            get { return _errorMessage; } 
        }

        internal PluginDiscoverer Discoverer { get { return _discoverer; } }

        protected DiscoveredInfo( PluginDiscoverer discoverer )
        {
            _discoverer = discoverer;
            _version = _discoverer.CurrentVersion;
        }

        protected void Initialize( Runner.DiscoveredInfo info )
        {
            _errorMessage = info.ErrorMessage;
        }

        protected bool Merge( Runner.DiscoveredInfo info, bool hasChanged )
        {
            if( hasChanged || _errorMessage != info.ErrorMessage )
            {
                _errorMessage = info.ErrorMessage;
                _version = _discoverer.CurrentVersion;
                return true;
            }
            return false;
        }
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Context.Tests\TestContextHost.cs) is part of CiviKey. 
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
using System.IO;

namespace CK.Context.Tests
{
    class TestContextHost : AbstractContextHost
    {
        string _name;

        public TestContextHost( string name )
        {
            _name = name;
        }

        public Uri SystemConfigAddress
        {
            get { return new Uri( Path.Combine( TestBase.AppFolder, "Config-Sys-" + _name ) ); }
        }

        public Uri DefaultUserConfigAddress
        {
            get { return new Uri( Path.Combine( TestBase.AppFolder, "Config-Usr-" + _name ) ); }
        }

        public KeyValuePair<string, Uri> DefaultContextProfile
        {
            get { return new KeyValuePair<string, Uri>( "Default-" + _name, new Uri( Path.Combine( TestBase.AppFolder, "Ctx-" + _name ) ) ); }
        }


        public override Uri GetSystemConfigAddress()
        {
            return SystemConfigAddress;
        }

        protected override Uri GetDefaultUserConfigAddress( bool saving )
        {
            return DefaultUserConfigAddress;
        }

        protected override KeyValuePair<string, Uri> GetDefaultContextProfile( bool saving )
        {
            return DefaultContextProfile;
        }
    }
}

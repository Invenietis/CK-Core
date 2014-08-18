#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Performance\FakeHandlerConfiguration.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    [HandlerType( typeof( FakeHandler ) )]
    class FakeHandlerConfiguration : HandlerConfiguration
    {
        public FakeHandlerConfiguration( string name )
            : base( name )
        {
            ExtraLoad = -1;
        }

        protected override void Initialize( IActivityMonitor m, XElement xml )
        {
            int s = xml.GetAttributeInt( "ExtraLoad", -1 );
            ExtraLoad = s < 0 ? -1 : s;
        }

        public int ExtraLoad { get; set; }
    }
}

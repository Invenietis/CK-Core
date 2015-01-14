#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core.PowershellExtensions\Cmdlets\NewActivityMonitorCmdlet.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using CK.Core.PowershellExtensions.Impl;

namespace CK.Core.PowershellExtensions.Cmdlets
{
    [Cmdlet( VerbsCommon.New, "ActivityMonitor" )]
    [CLSCompliant( false )]
    public class NewActivityMonitorCmdlet : Cmdlet
    {
        [Parameter( Position = 0 )]
        public SwitchParameter ConsoleOutput
        {
            get { return _console; }
            set { _console = value; }
        }
        bool _console;

        protected override void ProcessRecord()
        {
            WriteObject( new PowershellActivityMonitor( _console ) );
        }
    }
}

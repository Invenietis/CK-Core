#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\LogExternalEntry.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    class LogExternalEntry : LogEventArgs, ILogExternalEntry
    {
        int _depth;
        string _msg;
        object _extraData;

        internal LogExternalEntry( int lsn, int depth, string message, object extraData )
        {
            LSN = -lsn;
            _depth = depth;
            _msg = message;
            _extraData = extraData;
        }

        public override LogEntryType EntryType
        {
            get { return LogEntryType.External; }
        }

        public override int Depth
        {
            get { return _depth; }
        }

        public string Message
        {
            get { return _msg; }
        }

        public object ExtraData
        {
            get { return _extraData; }
        }

    }
}

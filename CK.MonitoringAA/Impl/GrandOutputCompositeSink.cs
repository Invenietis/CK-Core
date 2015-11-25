#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Impl\GrandOutputCompositeSink.cs) is part of CiviKey. 
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    internal class GrandOutputCompositeSink : IGrandOutputSink
    {
        IGrandOutputSink[] _sinks;

        public void Add( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            Util.InterlockedAdd( ref _sinks, sink );
        }

        public void Remove( IGrandOutputSink sink )
        {
            if( sink == null ) throw new ArgumentNullException( "sink" );
            Util.InterlockedRemove( ref _sinks, sink );
        }

        void IGrandOutputSink.Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            var sinks = _sinks;
            if( sinks != null )
            {
                foreach( var l in sinks )
                {
                    try
                    {
                        l.Handle( logEvent, parrallelCall );
                    }
                    catch( Exception exCall )
                    {
                        ActivityMonitor.CriticalErrorCollector.Add( exCall, l.GetType().FullName );
                        Util.InterlockedRemove( ref _sinks, l );
                    }
                }
            }
        }
    }
}

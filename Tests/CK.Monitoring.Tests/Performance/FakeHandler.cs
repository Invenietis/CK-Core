#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Performance\FakeHandler.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Monitoring.GrandOutputHandlers
{
    class FakeHandler : HandlerBase
    {
        int _extraLoad;

        public static int TotalHandleCount;
        public static int HandlePerfTraceCount;
        public static int SizeHandled;

        public FakeHandler( FakeHandlerConfiguration config )
            : base( config )
        {
            _extraLoad = config.ExtraLoad;
        }

        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            ++TotalHandleCount;
            if( logEvent.Entry.LogType == LogEntryType.Line && logEvent.Entry.Text.StartsWith( "PerfTrace:" ) ) ++HandlePerfTraceCount;
            ComputeSize( logEvent, true );
            for( int i = 0; i < _extraLoad; ++i ) ComputeSize( logEvent, false );
        }

        void ComputeSize( GrandOutputEventInfo logEvent, bool increment )
        {
            using( MemoryStream m = new MemoryStream() )
            using( BinaryWriter w = new BinaryWriter( m ) )
            {
                logEvent.Entry.WriteLogEntry( w );
                if( increment ) Interlocked.Add( ref SizeHandled, (int)m.Position );
            }
        }

    }
}

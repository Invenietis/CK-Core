#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\BinaryFile.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Binary file handler.
    /// </summary>
    public sealed class BinaryFile : HandlerBase
    {
        readonly MonitorBinaryFileOutput _file;

        /// <summary>
        /// Initializes a new <see cref="BinaryFileConfiguration"/> bound to its <see cref="BinaryFileConfiguration"/>.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public BinaryFile( BinaryFileConfiguration config )
            : base( config )
        {
            if( config == null ) throw new ArgumentNullException( "config" );
            _file = new MonitorBinaryFileOutput( config.Path, config.MaxCountPerFile, config.UseGzipCompression );
            _file.FileWriteThrough = config.FileWriteThrough;
            _file.FileBufferSize = config.FileBufferSize;
        }

        /// <summary>
        /// Initialization of the handler: computes the path.
        /// </summary>
        /// <param name="m"></param>
        public override void Initialize( IActivityMonitor m )
        {
            using( m.OpenTrace().Send( "Initializing BinaryFile handler '{0}' (MaxCountPerFile = {1}).", Name, _file.MaxCountPerFile ) )
            {
                _file.Initialize( m );
            }
        }

        /// <summary>
        /// Writes a log entry (that can actually be a <see cref="IMulticastLogEntry"/>).
        /// </summary>
        /// <param name="logEvent">The log entry.</param>
        /// <param name="parrallelCall">True if this is a parrallel call.</param>
        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            _file.Write( logEvent.Entry );
        }

        /// <summary>
        /// Closes the file if it is opened.
        /// </summary>
        /// <param name="m">The monitor to use to track activity.</param>
        public override void Close( IActivityMonitor m )
        {
            m.Info().Send( "Closing file for BinaryFile handler '{0}'.", Name );
            _file.Close();
        }

    }

}

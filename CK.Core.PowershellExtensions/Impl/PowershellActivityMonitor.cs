#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core.PowershellExtensions\Impl\PowershellActivityMonitor.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.PowershellExtensions.Impl
{
    internal class PowershellActivityMonitor : IPowershellActivityMonitor
    {
        ActivityMonitor _monitor;
        TemporaryFile _underlyingFile;

        public PowershellActivityMonitor( bool createConsoleClient = false )
        {
            _monitor = new ActivityMonitor();
            _underlyingFile = new TemporaryFile();

            if( createConsoleClient )
                _monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );

            _monitor.Output.RegisterClient( new ActivityMonitorTextWriterClient( s => File.AppendAllText( _underlyingFile.Path, s ) ) );
        }

        #region IPowershellActivityMonitor members

        public void Clear()
        {
            _underlyingFile.Dispose();
            _underlyingFile = new TemporaryFile();
        }

        public IEnumerable<string> ReadAllLines()
        {
            return File.ReadLines( _underlyingFile.Path );
        }

        public void WriteError( string log )
        {
            _monitor.Error().Send( log );
        }

        public void WriteFatal( string log )
        {
            _monitor.Fatal().Send( log );
        }

        public void WriteInfo( string log )
        {
            _monitor.Info().Send( log );
        }

        public void WriteTrace( string log )
        {
            _monitor.Trace().Send( log );
        }

        public void WriteWarn( string log )
        {
            _monitor.Warn().Send( log );
        }

        #endregion

        #region IActivityMonitor members

        public LogFilter ActualFilter
        {
            get { return _monitor.ActualFilter; }
        }

        public CKTrait AutoTags
        {
            get { return _monitor.AutoTags; }
            set { _monitor.AutoTags = value; }
        }

        public void CloseGroup( DateTimeStamp logTimeUtc, object userConclusion = null )
        {
            _monitor.CloseGroup( logTimeUtc, userConclusion );
        }

        public LogFilter MinimalFilter
        {
            get { return _monitor.MinimalFilter; }
            set { _monitor.MinimalFilter = value; }
        }

        public IActivityMonitorOutput Output
        {
            get { return _monitor.Output; }
        }

        public void SetTopic( string newTopic, string fileName = null, int lineNumber = 0 )
        {
            _monitor.SetTopic( newTopic, fileName, lineNumber );
        }

        public string Topic
        {
            get { return _monitor.Topic; }
        }

        public void UnfilteredLog( ActivityMonitorLogData data )
        {
            _monitor.UnfilteredLog( data );
        }

        public IDisposableGroup UnfilteredOpenGroup( ActivityMonitorGroupData data )
        {
            return _monitor.UnfilteredOpenGroup( data );
        }

        public DateTimeStamp LastLogTime
        {
            get { return _monitor.LastLogTime; }
        }

        #endregion
    }
}

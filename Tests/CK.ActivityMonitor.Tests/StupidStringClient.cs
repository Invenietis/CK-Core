#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\StupidStringClient.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using CK.Core;

namespace CK.Core.Tests.Monitoring
{
    public class StupidStringClient : IActivityMonitorClient
    {
        public class Entry
        {
            public readonly LogLevel Level;
            public readonly CKTrait Tags;
            public readonly string Text;
            public readonly Exception Exception;
            public readonly DateTimeStamp LogTime;

            public Entry( ActivityMonitorLogData d )
            {
                Level = d.Level;
                Tags = d.Tags;
                Text = d.Text;
                Exception = d.Exception;
                LogTime = d.LogTime;
            }
            
            public Entry( IActivityLogGroup d )
            {
                Level = d.GroupLevel;
                Tags = d.GroupTags;
                Text = d.GroupText;
                Exception = d.Exception;
                LogTime = d.LogTime;
            }
        }
        public readonly List<Entry> Entries;
        public StringWriter Writer { get; private set; }
        public bool WriteTags { get; private set; }
        public bool WriteConclusionTraits { get; private set; }

        int _curLevel;

        public StupidStringClient( bool writeTags = false, bool writeConclusionTraits = false )
        {
            _curLevel = -1;
            Entries = new List<Entry>();
            Writer = new StringWriter();
            WriteTags = writeTags;
            WriteConclusionTraits = writeConclusionTraits;
        }


        #region IActivityMonitorClient members

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
        }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            var level = data.Level & LogLevel.Mask;

            if( data.Text == ActivityMonitor.ParkLevel )
            {
                if( _curLevel != -1 )
                {
                    OnLeaveLevel( (LogLevel)_curLevel );
                }
                _curLevel = -1;
            }
            else
            {
                if( _curLevel == (int)level )
                {
                    OnContinueOnSameLevel( data );
                }
                else
                {
                    if( _curLevel != -1 )
                    {
                        OnLeaveLevel( (LogLevel)_curLevel );
                    }
                    OnEnterLevel( data );
                    _curLevel = (int)level;
                }
            }
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            if( _curLevel != -1 )
            {
                OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }

            OnGroupOpen( group );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _curLevel != -1 )
            {
                OnLeaveLevel( (LogLevel)_curLevel );
                _curLevel = -1;
            }

            OnGroupClose( group, conclusions );
        }

        #endregion IActivityMonitorClient members

        void OnEnterLevel( ActivityMonitorLogData data )
        {
            Entries.Add( new Entry( data ) );
            Writer.WriteLine();
            Writer.Write( data.MaskedLevel.ToString() + ": " + data.Text );
            if( WriteTags ) Writer.Write( "-[{0}]", data.Tags.ToString() );
            if( data.Exception != null ) Writer.Write( "Exception: " + data.Exception.Message );
        }

        void OnContinueOnSameLevel( ActivityMonitorLogData data )
        {
            Entries.Add( new Entry( data ) );
            Writer.Write( data.Text );
            if( WriteTags ) Writer.Write( "-[{0}]", data.Tags.ToString() );
            if( data.Exception != null ) Writer.Write( "Exception: " + data.Exception.Message );
        }

        void OnLeaveLevel( LogLevel level )
        {
            Writer.Flush();
        }

        void OnGroupOpen( IActivityLogGroup g )
        {
            Entries.Add( new Entry( g ) );
            Writer.WriteLine();
            Writer.Write( new String( '+', g.Depth ) );
            Writer.Write( "{1} ({0})", g.MaskedGroupLevel, g.GroupText );
            if( g.Exception != null ) Writer.Write( "Exception: " + g.Exception.Message );
            if( WriteTags ) Writer.Write( "-[{0}]", g.GroupTags.ToString() );
        }

        void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            Writer.WriteLine();
            Writer.Write( new String( '-', g.Depth ) );
            if( WriteConclusionTraits )
            {
                Writer.Write( String.Join( ", ", conclusions.Select( c => c.Text + "-/[/"+ c.Tag.ToString() +"/]/" ) ) );
            }
            else
            {
                Writer.Write( String.Join( ", ", conclusions.Select( c => c.Text ) ) );
            }
        }

        public override string ToString()
        {
            return Writer.ToString();
        }
    }

}

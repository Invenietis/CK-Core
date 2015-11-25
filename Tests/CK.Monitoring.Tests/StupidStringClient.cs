using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CK.Core;

namespace CK.Monitoring.Tests
{
    public class StupidStringClient : ActivityMonitorTextHelperClient
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

            public override string ToString()
            {
                return String.Format( "{0} - {1} - {2} - {3}", LogTime, Level, Text, Exception != null ? Exception.ToString() : "<no exception>" );
            }
        }
        public readonly List<Entry> Entries;
        public StringWriter Writer { get; private set; }
        public bool WriteTags { get; private set; }
        public bool WriteConclusionTraits { get; private set; }

        public StupidStringClient( bool writeTags = false, bool writeConclusionTraits = false )
        {
            Entries = new List<Entry>();
            Writer = new StringWriter();
            WriteTags = writeTags;
            WriteConclusionTraits = writeConclusionTraits;
        }

        protected override void OnEnterLevel( ActivityMonitorLogData data )
        {
            Entries.Add( new Entry( data ) );
            Writer.WriteLine();
            Writer.Write( data.MaskedLevel.ToString() + ": " + data.Text );
            if( WriteTags ) Writer.Write( "-[{0}]", data.Tags.ToString() );
            if( data.Exception != null ) Writer.Write( "Exception: " + data.Exception.Message );
        }

        protected override void OnContinueOnSameLevel( ActivityMonitorLogData data )
        {
            Entries.Add( new Entry( data ) );
            Writer.Write( data.Text );
            if( WriteTags ) Writer.Write( "-[{0}]", data.Tags.ToString() );
            if( data.Exception != null ) Writer.Write( "Exception: " + data.Exception.Message );
        }

        protected override void OnLeaveLevel( LogLevel level )
        {
            Writer.Flush();
        }

        protected override void OnGroupOpen( IActivityLogGroup g )
        {
            Entries.Add( new Entry( g ) );
            Writer.WriteLine();
            Writer.Write( new String( '+', g.Depth ) );
            Writer.Write( "{1} ({0})", g.MaskedGroupLevel, g.GroupText );
            if( g.Exception != null ) Writer.Write( "Exception: " + g.Exception.Message );
            if( WriteTags ) Writer.Write( "-[{0}]", g.GroupTags.ToString() );
        }

        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
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

        public string ToStringFromWriter()
        {
            return Writer.ToString();
        }
        
        public override string ToString()
        {
            return String.Join( Environment.NewLine, Entries );
        }
    }

}

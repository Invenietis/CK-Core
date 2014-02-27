using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    class JsonLogPageSerializer
    {
        readonly string _outputFile;
        readonly SimpleJsonWriter _writer;

        private JsonLogPageSerializer( string outputFile )
        {
            Debug.Assert( !String.IsNullOrWhiteSpace( outputFile ) );

            _outputFile = outputFile;
            _writer = new SimpleJsonWriter();
        }

        private void WriteToFile()
        {
            File.WriteAllText( _outputFile, _writer.GetOutput(), Encoding.Unicode );
        }

        public static void SerializeLogPage( IStructuredLogPage page, string outputFile )
        {
            JsonLogPageSerializer s = new JsonLogPageSerializer( outputFile );
            SimpleJsonWriter w = s._writer;

            w.OpenObject();

            w.WriteProperty( "PageNumber", page.PageNumber );

            w.WritePropertyStart( "OpenGroupsAtStart" );
            w.OpenArray();
            foreach( var e in page.OpenGroupsAtStart ) s.WriteLogEntry( e );
            w.CloseArray();

            w.WritePropertyStart( "OpenGroupsAtEnd" );
            w.OpenArray();
            foreach( var e in page.OpenGroupsAtEnd ) s.WriteLogEntry( e );
            w.CloseArray();

            w.WritePropertyStart( "Entries" );
            w.OpenArray();
            foreach( var e in page.Entries ) s.WriteLogEntry( e );
            w.CloseArray();

            w.CloseObject();

            s.WriteToFile();
        }

        public static void SerializeMonitorIndex( MonitorIndexInfo index, string outputFile, Func<int, string> getLogPageJsonPath )
        {
            JsonLogPageSerializer s = new JsonLogPageSerializer( outputFile );
            SimpleJsonWriter w = s._writer;

            w.OpenObject();

            w.WriteProperty( "MonitorGuid", index.MonitorGuid.ToString() );
            w.WriteProperty( "MonitorTitle", index.MonitorTitle );
            w.WriteProperty( "PageCount", index.PageCount );
            w.WriteProperty( "PageLength", index.PageLength );
            w.WriteProperty( "TotalEntryCount", index.TotalEntryCount );
            w.WriteProperty( "TotalTraceCount", index.TotalTraceCount );
            w.WriteProperty( "TotalInfoCount", index.TotalInfoCount );
            w.WriteProperty( "TotalWarnCount", index.TotalWarnCount );
            w.WriteProperty( "TotalErrorCount", index.TotalErrorCount );
            w.WriteProperty( "TotalFatalCount", index.TotalFatalCount );

            w.WritePropertyStart( "Groups" );
            w.OpenArray();
            foreach( var g in index.Groups ) s.WriteMonitorGroupReference( g );
            w.CloseArray();

            w.WritePropertyStart( "Pages" );
            w.OpenArray();
            foreach( var g in index.Pages ) s.WriteMonitorPageReference( g, getLogPageJsonPath );
            w.CloseArray();

            w.CloseObject();

            s.WriteToFile();
        }

        public static void SerializeMonitorList( IEnumerable<MonitorIndexInfo> monitors, string outputFile, Func<Guid, string> getMonitorJsonPath )
        {
            JsonLogPageSerializer s = new JsonLogPageSerializer( outputFile );
            SimpleJsonWriter w = s._writer;

            w.OpenArray();

            foreach( var info in monitors )
            {
                w.OpenObject();

                w.WriteProperty( "MonitorGuid", info.MonitorGuid.ToString() );
                w.WriteProperty( "MonitorTitle", info.MonitorTitle );
                w.WriteProperty( "Path", getMonitorJsonPath( info.MonitorGuid ) );

                w.WriteProperty( "PageCount", info.PageCount );

                w.WriteProperty( "TotalEntryCount", info.TotalEntryCount );

                w.WriteProperty( "TotalWarnCount", info.TotalWarnCount );
                w.WriteProperty( "TotalErrorCount", info.TotalErrorCount );
                w.WriteProperty( "TotalFatalCount", info.TotalFatalCount );

                w.CloseObject();
            }

            w.CloseArray();
            s.WriteToFile();
        }

        void WriteMonitorPageReference( MonitorPageReference p, Func<int, string> getLogPageJsonPath )
        {
            _writer.OpenObject();

            _writer.WriteProperty( "EntryCount", p.EntryCount );
            _writer.WriteProperty( "PageLength", p.PageLength );
            _writer.WriteProperty( "FirstEntryTimestamp", p.FirstEntryTimestamp.ToString() );
            _writer.WriteProperty( "LastEntryTimestamp", p.LastEntryTimestamp.ToString() );
            _writer.WriteProperty( "Path", getLogPageJsonPath( p.PageNumber ) );

            _writer.CloseObject();
        }

        void WriteMonitorGroupReference( MonitorGroupReference g )
        {
            _writer.OpenObject();

            _writer.WriteProperty( "OpenGroupTimestamp", g.OpenGroupTimestamp.ToString() );
            _writer.WriteProperty( "CloseGroupTimestamp", g.CloseGroupTimestamp.ToString() );
            _writer.WriteProperty( "HighestLogLevel", (g.HighestLogLevel & LogLevel.Mask).ToString() );

            _writer.WritePropertyStart( "OpenGroupEntry" );
            WriteLogEntry( g.OpenGroupEntry );

            _writer.CloseObject();
        }
        void WriteLogEntry( IPagedLogEntry e )
        {
            _writer.OpenObject();

            WriteLogEntryContents( e );

            if(e.LogType == LogEntryType.OpenGroup)
            {
                _writer.WritePropertyStart( "Children" );
                _writer.OpenArray();
                foreach( var child in e.Children)
                {
                    WriteLogEntry( child );
                }
                _writer.CloseArray();


            }
            if( e.GroupStartsOnPage > 0 ) _writer.WriteProperty( "GroupStartsOnPage", e.GroupStartsOnPage );
            if( e.GroupEndsOnPage > 0 ) _writer.WriteProperty( "GroupEndsOnPage", e.GroupEndsOnPage );


            _writer.CloseObject();
        }
        void WriteLogEntry( ILogEntry e )
        {
            _writer.OpenObject();
            WriteLogEntryContents( e );
            _writer.CloseObject();
        }

        void WriteLogEntryContents( ILogEntry e )
        {
            _writer.WriteProperty( "LogType", e.LogType.ToString() );
            _writer.WriteProperty( "LogTime", e.LogTime.ToString() );

            if( e.LogLevel != LogLevel.None ) _writer.WriteProperty( "LogLevel", (e.LogLevel & LogLevel.Mask).ToString() );
            if( e.LogLevel != LogLevel.None ) _writer.WriteProperty( "IsFiltered", ((e.LogLevel & LogLevel.IsFiltered) == LogLevel.IsFiltered).ToString() );

            if( e.Text != null ) _writer.WriteProperty( "Text", e.Text );
            if( e.FileName != null ) _writer.WriteProperty( "FileName", e.FileName );

            if( e.LineNumber > 0 ) _writer.WriteProperty( "LineNumber", e.LineNumber.ToString() );

            if( !e.Tags.IsEmpty )
            {
                _writer.WriteProperty( "Tags", e.Tags.ToString() );
            }
            if( e.Conclusions != null )
            {
                _writer.WritePropertyStart( "Conclusions" );
                _writer.OpenArray();
                foreach( var c in e.Conclusions ) WriteConclusion( c );
                _writer.CloseArray();
            }
            if( e.Exception != null )
            {
                _writer.WritePropertyStart( "Exception" );
                WriteException( e.Exception );
            }
        }

        private void WriteConclusion( ActivityLogGroupConclusion c )
        {
            _writer.OpenObject();

            _writer.WriteProperty( "Text", c.Text );
            _writer.WriteProperty( "Tag", c.Tag.ToString() );

            _writer.CloseObject();
        }

        void WriteException( CKExceptionData ex )
        {
            _writer.OpenObject();

            _writer.WriteProperty( "ExceptionTypeAssemblyQualifiedName", ex.ExceptionTypeAssemblyQualifiedName );
            _writer.WriteProperty( "ExceptionTypeName", ex.ExceptionTypeName );
            _writer.WriteProperty( "Message", ex.Message );
            if( ex.StackTrace != null ) _writer.WriteProperty( "StackTrace", ex.StackTrace );

            if( ex.InnerException != null ) { _writer.WritePropertyStart( "InnerException" ); WriteException( ex.InnerException ); }

            if( ex.FusionLog != null ) _writer.WriteProperty( "FusionLog", ex.FusionLog );
            if( ex.FileName != null ) _writer.WriteProperty( "FileName", ex.FileName );

            if( ex.LoaderExceptions != null )
            {
                _writer.WritePropertyStart( "LoaderExceptions" );
                _writer.OpenArray();
                foreach( var lex in ex.LoaderExceptions ) WriteException( lex );
                _writer.CloseArray();
            }

            if( ex.AggregatedExceptions != null )
            {
                _writer.WritePropertyStart( "AggregatedExceptions" );
                _writer.OpenArray();
                foreach( var aex in ex.AggregatedExceptions ) WriteException( aex );
                _writer.CloseArray();
            }

            _writer.CloseObject();
        }
    }

}

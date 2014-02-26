using System;
using System.Collections.Generic;
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
        #region ILogPageSerializer Members

        public static bool SerializeLogPage( ILogPage page, string outputFile )
        {
            using( JSResWriter writer = new JSResWriter() )
            {
                writer.ExportNamedVar( "PageNumber", page.PageNumber.ToString() );
                writer.ExportNamedArray( "OpenGroupsAtStart", page.OpenGroupsAtStart, WriteLogEntry );
                writer.ExportNamedArray( "OpenGroupsAtEnd", page.OpenGroupsAtEnd, WriteLogEntry );

                writer.ExportNamedArray( "Entries", page.Entries, WriteLogEntry );

                File.WriteAllText( outputFile, writer.GetResult() );
            }

            return true;
        }

        public static bool SerializeMonitorIndex( MonitorIndexInfo index, string outputFile, Func<int, string> getLogPageJsonPath )
        {
            using( JSResWriter writer = new JSResWriter() )
            {
                writer.ExportNamedVar( "MonitorGuid", index.MonitorGuid.ToString() );
                writer.ExportNamedVar( "MonitorTitle", index.MonitorTitle );
                writer.ExportNamedVar( "PageCount", index.PageCount.ToString() );
                writer.ExportNamedVar( "PageLength", index.PageLength.ToString() );
                writer.ExportNamedVar( "TotalEntryCount", index.TotalEntryCount.ToString() );
                writer.ExportNamedVar( "TotalTraceCount", index.TotalTraceCount.ToString() );
                writer.ExportNamedVar( "TotalInfoCount", index.TotalInfoCount.ToString() );
                writer.ExportNamedVar( "TotalWarnCount", index.TotalWarnCount.ToString() );
                writer.ExportNamedVar( "TotalErrorCount", index.TotalErrorCount.ToString() );
                writer.ExportNamedVar( "TotalFatalCount", index.TotalFatalCount.ToString() );


                writer.ExportNamedArray( "Groups", index.Groups, WriteMonitorGroupReference );
                writer.ExportNamedArray( "Pages", index.Pages, ( p, jw ) => { WriteMonitorPageReference( p, jw, getLogPageJsonPath ); } );


                File.WriteAllText( outputFile, writer.GetResult() );
            }

            return true;
        }

        public static bool SerializeMonitorList(IEnumerable<MonitorIndexInfo> monitors, string outputFile, Func<Guid, string> getMonitorJsonPath)
        {
            using( JSResWriter writer = new JSResWriter() )
            {
                writer.ExportNamedArray( "Monitors", monitors, ( info, w ) => {
                    using( JSResWriter sw = w.OpenRawObject() )
                    {
                        writer.ExportNamedVar( "MonitorGuid", info.MonitorGuid.ToString() );
                        writer.ExportNamedVar( "MonitorTitle", info.MonitorTitle );
                        writer.ExportNamedVar( "Path", getMonitorJsonPath(info.MonitorGuid) );

                        writer.ExportNamedVar( "PageCount", info.PageCount.ToString() );

                        writer.ExportNamedVar( "TotalEntryCount", info.TotalEntryCount.ToString() );

                        writer.ExportNamedVar( "TotalWarnCount", info.TotalWarnCount.ToString() );
                        writer.ExportNamedVar( "TotalErrorCount", info.TotalErrorCount.ToString() );
                        writer.ExportNamedVar( "TotalFatalCount", info.TotalFatalCount.ToString() );
                    }
                } );


                File.WriteAllText( outputFile, writer.GetResult() );
            }
            return true;
        }

        private static void WriteMonitorPageReference( MonitorPageReference p, JSResWriter w, Func<int, string> getLogPageJsonPath )
        {
            using( JSResWriter sw = w.OpenRawObject() )
            {
                sw.ExportNamedVar( "EntryCount", p.EntryCount.ToString() );
                sw.ExportNamedVar( "PageLength", p.PageLength.ToString() );
                sw.ExportNamedVar( "FirstEntryTimestamp", p.FirstEntryTimestamp.ToString() );
                sw.ExportNamedVar( "LastEntryTimestamp", p.LastEntryTimestamp.ToString() );
                sw.ExportNamedVar( "Path", getLogPageJsonPath( p.PageNumber ) );
            }
        }

        private static void WriteMonitorGroupReference( MonitorGroupReference g, JSResWriter w )
        {
            using( JSResWriter sw = w.OpenRawObject() )
            {
                sw.ExportNamedVar( "OpenGroupTimestamp", g.OpenGroupTimestamp.ToString() );
                sw.ExportNamedVar( "CloseGroupTimestamp", g.CloseGroupTimestamp.ToString() );
                sw.ExportNamedVar( "HighestLogLevel", (g.HighestLogLevel & LogLevel.Mask).ToString() );

                sw.ExportUnescapedNamedVar( "OpenGroupEntry", g.OpenGroupEntry, WriteLogEntry );
            }
        }

        private static void WriteLogEntry( ILogEntry e, JSResWriter w )
        {
            using( JSResWriter sw = w.OpenRawObject() )
            {
                sw.ExportNamedVar( "LogType", e.LogType.ToString() );
                sw.ExportNamedVar( "LogTime", e.LogTime.ToString() );

                if( e.LogLevel != LogLevel.None ) sw.ExportNamedVar( "LogLevel", (e.LogLevel & LogLevel.Mask).ToString() );
                if( e.LogLevel != LogLevel.None ) sw.ExportNamedVar( "IsFiltered", ((e.LogLevel & LogLevel.IsFiltered) == LogLevel.IsFiltered).ToString() );

                if( e.Text != null ) sw.ExportNamedVar( "Text", e.Text );
                if( e.FileName != null ) sw.ExportNamedVar( "FileName", e.FileName );

                if( e.LineNumber > 0 ) sw.ExportNamedVar( "LineNumber", e.LineNumber.ToString() );

                if( !e.Tags.IsEmpty )
                {
                    sw.ExportNamedVar( "Tags", e.Tags.ToString() );
                }
                if( e.Conclusions != null )
                {
                    sw.ExportNamedArray( "Conclusions", e.Conclusions, ( c, ws ) => ws.ExportString( c.Text ) );
                }
                if( e.Exception != null )
                {
                    sw.ExportUnescapedNamedVar( "Exception", GetExceptionObject( e.Exception ) );
                }
            }
        }

        private static string GetExceptionObject( Core.CKExceptionData ex )
        {
            string result;

            using( JSResWriter w = new JSResWriter() )
            {
                w.ExportNamedVar( "ExceptionTypeAssemblyQualifiedName", ex.ExceptionTypeAssemblyQualifiedName );
                w.ExportNamedVar( "ExceptionTypeName", ex.ExceptionTypeName );
                w.ExportNamedVar( "Message", ex.Message );
                if( ex.StackTrace != null ) w.ExportNamedVar( "StackTrace", ex.StackTrace );
                if( ex.InnerException != null ) w.ExportUnescapedNamedVar( "InnerException", GetExceptionObject( ex.InnerException ) );
                if( ex.FusionLog != null ) w.ExportNamedVar( "FusionLog", ex.FusionLog );
                if( ex.FileName != null ) w.ExportNamedVar( "FileName", ex.FileName );
                if( ex.LoaderExceptions != null ) w.ExportNamedArray( "LoaderExceptions", ex.LoaderExceptions, ( e, wx ) => wx.ExportRawUnescaped( GetExceptionObject( e ) ) );
                if( ex.AggregatedExceptions != null ) w.ExportNamedArray( "AggregatedExceptions", ex.AggregatedExceptions, ( e, wx ) => wx.ExportRawUnescaped( GetExceptionObject( e ) ) );

                result = w.GetResult();
            }
            return result;
        }

        #endregion
    }



    public class JSResWriter : IDisposable
    {
        StringBuilder _builder;
        bool _hasPrev;
        bool _isRaw;

        public JSResWriter( string varName )
        {
            _builder = new StringBuilder();
            AppendJSString( varName );
            _builder.Append( "={" );
        }

        public JSResWriter()
        {
            _builder = new StringBuilder();
            _builder.Append( "{" );
            _isRaw = true;
        }

        public JSResWriter OpenNamedObject( string name )
        {
            if( _hasPrev ) _builder.Append( ',' );
            AppendJSString( name );
            _builder.Append( ":{" );
            _hasPrev = false;
            return this;
        }

        public JSResWriter OpenRawObject()
        {
            if( _hasPrev ) _builder.Append( ',' );
            _builder.Append( "{" );
            _hasPrev = false;
            return this;
        }

        public JSResWriter ExportNamedArray<T>( string arrayName, IEnumerable<T> items, Action<T, JSResWriter> writeObjectAction )
        {
            if( _hasPrev ) _builder.Append( ',' );

            AppendJSString( arrayName );
            _builder.Append( ":[" );
            _hasPrev = false;
            foreach( T item in items )
            {
                writeObjectAction( item, this );
            }
            _builder.Append( "]" );
            _hasPrev = true;
            return this;
        }

        public JSResWriter CloseObject()
        {
            _builder.Append( "}" );
            _hasPrev = true;
            return this;
        }

        public string GetResult()
        {
            if( _isRaw )
            {
                _builder.Append( "}" );
            }
            else
            {
                _builder.Append( "};" );
            }
            return _builder.ToString();
        }

        public JSResWriter ExportNamedVar( string name, string value )
        {
            if( _hasPrev ) _builder.Append( ',' );
            AppendJSString( name );
            _builder.Append( ':' );
            AppendJSString( value );
            _hasPrev = true;
            return this;
        }

        public JSResWriter ExportUnescapedNamedVar( string name, string value )
        {
            if( _hasPrev ) _builder.Append( ',' );
            AppendJSString( name );
            _builder.Append( ':' );
            _builder.Append( value );
            _hasPrev = true;
            return this;
        }
        public JSResWriter ExportUnescapedNamedVar<T>( string name, T value, Action<T, JSResWriter> writeObjectAction )
        {
            if( _hasPrev ) _builder.Append( ',' );
            AppendJSString( name );
            _builder.Append( ':' );
            _hasPrev = false;
            writeObjectAction( value, this );
            _hasPrev = true;
            return this;
        }

        public JSResWriter ExportString( string value )
        {
            if( _hasPrev ) _builder.Append( ',' );
            AppendJSString( value );
            _hasPrev = true;
            return this;
        }

        public JSResWriter ExportRawUnescaped( string content )
        {
            if( _hasPrev ) _builder.Append( ',' );
            _builder.Append( content );
            _hasPrev = true;
            return this;
        }

        void AppendJSString( string value )
        {
            _builder.Append( "\"" );
            foreach( char c in value )
            {
                switch( c )
                {
                    case '\"':
                        _builder.Append( "\\\"" );
                        break;
                    case '\\':
                        _builder.Append( "\\\\" );
                        break;
                    case '\b':
                        _builder.Append( "\\b" );
                        break;
                    case '\f':
                        _builder.Append( "\\f" );
                        break;
                    case '\n':
                        _builder.Append( "\\n" );
                        break;
                    case '\r':
                        _builder.Append( "\\r" );
                        break;
                    case '\t':
                        _builder.Append( "\\t" );
                        break;
                    default:
                        int i = (int)c;
                        if( i < 32 || i > 127 )
                        {
                            _builder.AppendFormat( "\\u{0:X04}", i );
                        }
                        else
                        {
                            _builder.Append( c );
                        }
                        break;
                }
            }
            _builder.Append( "\"" );
        }

        void IDisposable.Dispose()
        {
            CloseObject();
        }
    }
}

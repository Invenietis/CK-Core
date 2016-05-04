using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CK.Core;
using System.Text;
using CK.Text;
using System.Diagnostics;
using System.Linq;

namespace CK.Monitoring
{
    /// <summary>
    /// Helper class that encapsulates temporary stream and final renaming for log entries streams.
    /// This currently handles only the maximum count of entries per file but this may be extended with options like "SubFolderMode" that can be based 
    /// on current time (to group logs inside timed intermediate folders like one per day: 2014/01/12 or 2014-01/12, etc.). 
    /// </summary>
    public class MonitorTextFileOutput : MonitorFileOutputBase
    {
        readonly StringBuilder _builder;
        StreamWriter _writer;
        Guid _currentMonitorId;
        bool _monitorColumn;

        /// <summary>
        /// Initializes a new file for <see cref="IMulticastLogEntry"/>: the final file name is based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> with a ".ckmon" extension.
        /// You must call <see cref="Initialize"/> before actually using this object.
        /// </summary>
        /// <param name="configuredPath">The path: it can be absolute and when relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> (that must be set).</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        /// <param name="useGzipCompression">True to gzip the file.</param>
        public MonitorTextFileOutput( string configuredPath, int maxCountPerFile, bool useGzipCompression )
            : base( configuredPath, ".txt" + (useGzipCompression ? ".gzip" : string.Empty), maxCountPerFile, useGzipCompression )
        {
            _builder = new StringBuilder();
        }

        /// <summary>
        /// Gets or sets whether the monitor identifier should appear as the first column.
        /// </summary>
        public bool MonitorColumn
        {
            get { return _monitorColumn; }
            set { _monitorColumn = value; }
        }
        /// <summary>
        /// Writes a log entry (that can actually be a <see cref="IMulticastLogEntry"/>).
        /// </summary>
        /// <param name="e">The log entry.</param>
        public void Write( IMulticastLogEntry e )
        {
            Debug.Assert( DateTimeStamp.MaxValue.ToString().Length == 32,
                "DateTimeStamp FileNameUniqueTimeUtcFormat and the uniquifier: max => 32 characters long." );
            Debug.Assert( Guid.NewGuid().ToString().Length == 36,
                "Guid => 18 characters long." );

            BeforeWrite();
            int stdPrefixLen = 35;
            if( _monitorColumn )
            {
                stdPrefixLen += 37;
            }
            _builder.Append( ' ', _monitorColumn ? 37 + 35 : 35 );
            _builder.Append( "| ", e.Text != null ? e.GroupDepth : e.GroupDepth - 1 );
            string prefix = _builder.ToString();
            _builder.Clear();
            if( _monitorColumn )
            {
                _builder.Append( e.MonitorId.ToString() ).Append( ' ' );
            }
            else
            {
                // MonitorId (if needed) on one line.
                if( _currentMonitorId != e.MonitorId )
                {
                    _currentMonitorId = e.MonitorId;
                    _builder.Append( '-', 33 ).AppendLine( _currentMonitorId.ToString() );
                }
            }
            // Log time prefixes the first line only.
            string logTime = e.LogTime.ToString();
            _builder.Append( logTime );
            _builder.Append( ' ', 33 - logTime.Length );

            // Level is one char.
            char level;
            switch( e.LogLevel & LogLevel.Mask )
            {
                case LogLevel.Trace: level = ' '; break;
                case LogLevel.Info: level = 'i'; break;
                case LogLevel.Warn: level = 'W'; break;
                case LogLevel.Error: level = 'E'; break;
                default: level = 'F'; break;
            }
            _builder.Append( level );
            _builder.Append( ' ' );
            _builder.Append( "| ", e.Text != null ? e.GroupDepth : e.GroupDepth - 1 );

            if( e.Text != null )
            {
                if( e.LogType == LogEntryType.OpenGroup ) _builder.Append( ">>" );

                _builder.AppendLine( e.Text.Replace( Environment.NewLine, Environment.NewLine + prefix ) );
                if( e.Exception != null )
                {
                    e.Exception.ToStringBuilder( _builder, prefix );
                }
            }
            else 
            {
                Debug.Assert( e.Conclusions != null );
                _builder.Append( "<<" );
                if( e.Conclusions.Count > 0 )
                {
                    _builder.Append( " | " ).Append( e.Conclusions.Count ).Append( " conclusion" );
                    if( e.Conclusions.Count > 1 ) _builder.Append( 's' );
                    _builder.Append( ':' ).AppendLine();
                    string prefixConclusions = prefix + "   | ";
                    foreach( var c in e.Conclusions )
                    {
                        _builder.Append( prefixConclusions ).AppendLine( c.Text.Replace( Environment.NewLine, Environment.NewLine + prefixConclusions ) );
                    }
                }
                else 
                {
                    _builder.AppendLine();
                }
            }
            _writer.Write( _builder.ToString() );
            AfterWrite();
            _builder.Clear();
        }

        protected override Stream OpenNewFile()
        {
            Stream s = base.OpenNewFile();
            _writer = new StreamWriter( s );
            _currentMonitorId = Guid.Empty;
            return s;
        }

        protected override void CloseCurrentFile()
        {
            _writer.Flush();
            base.CloseCurrentFile();
            _writer.Dispose();
            _writer = null;
        }
    }
}

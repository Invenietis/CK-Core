using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CK.Core;
using CK.Text;

namespace CK.Monitoring
{
    /// <summary>
    /// Helper class that encapsulates temporary stream and final renaming for log entries streams.
    /// This currently handles only the maximum count of entries per file but this may be extended with options like "SubFolderMode" that can be based 
    /// on current time (to group logs inside timed intermediate folders like one per day: 2014/01/12 or 2014-01/12, etc.). 
    /// </summary>
    public class MonitorBinaryFileOutput : MonitorFileOutputBase
    {
        CKBinaryWriter _writer;

        /// <summary>
        /// Initializes a new file for <see cref="IMulticastLogEntry"/>: the final file name is based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> with a ".ckmon" extension.
        /// You must call <see cref="Initialize"/> before actually using this object.
        /// </summary>
        /// <param name="configuredPath">The path: it can be absolute and when relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> (that must be set).</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        /// <param name="useGzipCompression">True to gzip the file.</param>
        public MonitorBinaryFileOutput( string configuredPath, int maxCountPerFile, bool useGzipCompression )
            : base( configuredPath, ".ckmon", maxCountPerFile, useGzipCompression )
        {
        }

        /// <summary>
        /// Initializes a new file for <see cref="ILogEntry"/> issued from a specific monitor: the final file name is 
        /// based on <see cref="FileUtil.FileNameUniqueTimeUtcFormat"/> with a "-{XXX...XXX}.ckmon" suffix where {XXX...XXX} is the unique identifier (Guid with the B format - 32 digits separated by 
        /// hyphens, enclosed in braces) of the monitor.
        /// You must call <see cref="Initialize"/> before actually using this object.
        /// </summary>
        /// <param name="configuredPath">The path. Can be absolute. When relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> that must be set.</param>
        /// <param name="monitorId">Monitor identifier.</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        /// <param name="useGzipCompression">True to gzip the file.</param>
        public MonitorBinaryFileOutput( string configuredPath, Guid monitorId, int maxCountPerFile, bool useGzipCompression )
            : base( configuredPath, '-' + monitorId.ToString( "B" ) + ".ckmon", maxCountPerFile, useGzipCompression )
        {
        }

        #region Write methods

        /// <summary>
        /// Writes a log entry (that can actually be a <see cref="IMulticastLogEntry"/>).
        /// </summary>
        /// <param name="e">The log entry.</param>
        public void Write( ILogEntry e )
        {
            BeforeWrite();
            e.WriteLogEntry( _writer );
            AfterWrite();
        }

        /// <summary>
        /// Writes a line entry as a uni-cast compact entry or as a multi-cast one if needed.
        /// </summary>
        /// <param name="data">The log line.</param>
        /// <param name="adapter">Multi-cast information to be able to write multi-cast entry when needed.</param>
        public void UnicastWrite( ActivityMonitorLogData data, IMulticastLogInfo adapter )
        {
            BeforeWrite();
            LogEntry.WriteLog( _writer, adapter.MonitorId, adapter.PreviousEntryType, adapter.PreviousLogTime, adapter.GroupDepth, false, data.Level, data.LogTime, data.Text, data.Tags, data.ExceptionData, data.FileName, data.LineNumber );
            AfterWrite();
        }

        /// <summary>
        /// Writes a group opening entry as a uni-cast compact entry or as a multi-cast one if needed.
        /// </summary>
        /// <param name="g">The group line.</param>
        /// <param name="adapter">Multi-cast information to be able to write multi-cast entry when needed.</param>
        public void UnicastWriteOpenGroup( IActivityLogGroup g, IMulticastLogInfo adapter )
        {
            BeforeWrite();
            LogEntry.WriteLog( _writer, adapter.MonitorId, adapter.PreviousEntryType, adapter.PreviousLogTime, adapter.GroupDepth, true, g.GroupLevel, g.LogTime, g.GroupText, g.GroupTags, g.ExceptionData, g.FileName, g.LineNumber );
            AfterWrite();
        }

        /// <summary>
        /// Writes a group closing entry as a uni-cast compact entry or as a multi-cast one if needed.
        /// </summary>
        /// <param name="g">The group.</param>
        /// <param name="conclusions">Group's conclusions.</param>
        /// <param name="adapter">Multi-cast information to be able to write multi-cast entry when needed.</param>
        public void UnicastWriteCloseGroup( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions, IMulticastLogInfo adapter )
        {
            BeforeWrite();
            LogEntry.WriteCloseGroup( _writer, adapter.MonitorId, adapter.PreviousEntryType, adapter.PreviousLogTime, adapter.GroupDepth, g.GroupLevel, g.CloseLogTime, conclusions );
            AfterWrite();
        }

        #endregion

        protected override Stream OpenNewFile()
        {
            Stream s = base.OpenNewFile();
            _writer = new CKBinaryWriter( s );
            _writer.Write( LogReader.FileHeader );
            _writer.Write( LogReader.CurrentStreamVersion );
            return s;
        }

        protected override void CloseCurrentFile()
        {
            _writer.Write( (byte)0 );
            base.CloseCurrentFile();
            _writer.Dispose();
            _writer = null;
        }
    }
}

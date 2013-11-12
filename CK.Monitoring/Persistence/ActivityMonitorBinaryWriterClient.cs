using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    public sealed class ActivityMonitorBinaryWriterClient : IActivityMonitorClient, IDisposable
    {
        Stream _stream;
        BinaryWriter _binaryWriter;
        bool _writeVersion;

#if net40
        public ActivityMonitorBinaryWriterClient( Stream stream, bool writeVersion )
        {
            _stream = stream;
            _binaryWriter = new BinaryWriter( stream, Encoding.UTF8 );
            _writeVersion = writeVersion;
            if( writeVersion ) _binaryWriter.Write( LogReader.CurrentStreamVersion );
        }
#else
        public ActivityMonitorBinaryWriterClient( Stream stream, bool writeVersion, bool mustClose = true )
        {
            _stream = stream;
            _binaryWriter = new BinaryWriter( stream, Encoding.UTF8, !mustClose );
            _writeVersion = writeVersion;
            if( writeVersion ) _binaryWriter.Write( LogReader.CurrentStreamVersion );
        }
#endif
        public static ActivityMonitorBinaryWriterClient Create( string fileDirectory, string autoNameFile = "CK.Monitor-{0:u}.ckmon", bool writeVersion = true )
        {
            if( autoNameFile == null ) autoNameFile = "CK.Monitor-{0:u}.ckmon";
            string path = Path.Combine( fileDirectory, String.Format( autoNameFile, DateTime.UtcNow ) );
            return new ActivityMonitorBinaryWriterClient( new FileStream( path, FileMode.CreateNew, FileAccess.Write ), writeVersion );
        }

        public LogFilter MinimalFilter { get { return LogFilter.Undefined; } }

        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            LogEntry.WriteLog( _binaryWriter, false, data.Level, data.LogTimeUtc, data.Text, data.Tags, data.EnsureExceptionData(), data.FileName, data.LineNumber );
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            LogEntry.WriteLog( _binaryWriter, true, group.GroupLevel, group.LogTimeUtc, group.GroupText, group.GroupTags, group.EnsureExceptionData(), group.FileName, group.LineNumber );
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            LogEntry.WriteCloseGroup( _binaryWriter, group.GroupLevel, group.CloseLogTimeUtc, conclusions );
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        /// <summary>
        /// Closes the stream (writing the end of log marker).
        /// Use an explicit call to <see cref="Close"/> to avoid writing the marker.
        /// </summary>
        public void Dispose()
        {
            Close( true );
        }

        /// <summary>
        /// Close the log by optionally writing a zero terminal byte into the inner stream.
        /// The stream itself will be closed only if this writer has been asked to do so (thanks to constructors' parameter mustClose sets to true).
        /// </summary>
        /// <param name="writeEndMarker">True to write an end byte marker in the inner stream.</param>
        public void Close( bool writeEndMarker = true )
        {
            if( _stream != null )
            {
                if( writeEndMarker ) _binaryWriter.Write( (byte)0 );
                _binaryWriter.Flush();
                _binaryWriter.Dispose();
                _stream = null;
                _binaryWriter = null;
            }
        }

        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            // Does nothing.
        }

        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
            // Does nothing.
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CK.Core;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    public class ActivityMonitorWriterClient : IActivityMonitorClient, IDisposable
    {
        Stream _stream;
        BinaryWriter _binaryWriter;
        BinaryFormatter _binaryFormatter;
        bool _writeVersion;

        public ActivityMonitorWriterClient( Stream stream, bool writeVersion, bool mustClose = true )
        {
            _stream = stream;
            _binaryWriter = new BinaryWriter( stream, Encoding.UTF8, !mustClose );
            _binaryFormatter = new BinaryFormatter();
            _writeVersion = writeVersion;
            if( writeVersion ) _binaryWriter.Write( LogReader.CurrentStreamVersion );
        }

        public static ActivityMonitorWriterClient Create( string fileDirectory, string autoNameFile = "CK.Monitor-{0:u}.ckmon", bool writeVersion = true )
        {
            if( autoNameFile == null ) autoNameFile = "CK.Monitor-{0:u}.ckmon";
            string path = Path.Combine( fileDirectory, String.Format( autoNameFile, DateTime.UtcNow ) );
            return new ActivityMonitorWriterClient( new FileStream( path, FileMode.Create, FileAccess.Write ), writeVersion );
        }

        void IActivityMonitorClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            _binaryWriter.Write( (byte)((int)level << 2 | (int)StreamLogType.TypeLog) );
            _binaryWriter.Write( tags.ToString() );
            _binaryWriter.Write( text );
            _binaryWriter.Write( logTimeUtc.ToBinary() );
        }

        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            StreamLogType o = group.Exception == null ? StreamLogType.TypeOpenGroup : StreamLogType.TypeOpenGroupWithException;
            _binaryWriter.Write( (byte)((int)group.GroupLevel << 2 | (int)o) );
            _binaryWriter.Write( group.GroupTags.ToString() );
            _binaryWriter.Write( group.GroupText );
            _binaryWriter.Write( group.LogTimeUtc.ToBinary() );
            if( group.Exception != null )
            {
                _binaryFormatter.Serialize( _stream, group.Exception );
            }
        }

        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            _binaryWriter.Write( (byte)((int)group.GroupLevel << 2 | (int)StreamLogType.TypeGroupClosed) );
            _binaryWriter.Write( group.CloseLogTimeUtc.ToBinary() );
            _binaryWriter.Write( conclusions.Count );
            foreach( ActivityLogGroupConclusion c in conclusions )
            {
                _binaryWriter.Write( c.Tag.ToString() );
                _binaryWriter.Write( c.Text );
            }
        }

        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        void IActivityMonitorClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
        }

        /// <summary>
        /// Closes the stream (without writing the end of log marker).
        /// Use an explicit call to <see cref="Close"/> to write the marker.
        /// </summary>
        public void Dispose()
        {
            Close( true );
        }

        /// <summary>
        /// Close the log by optionnaly writing a zero terminal byte into the inner stream.
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
                _binaryFormatter = null;
            }
        }
    }
}

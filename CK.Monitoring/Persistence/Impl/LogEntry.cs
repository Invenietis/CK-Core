using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.Impl
{
    static class LogEntry
    {
        public static ILogEntry CreateLog( string text, DateTime t, LogLevel level, string fileName, int lineNumber, CKTrait tags = null )
        {
            if( tags == null ) return new LELog( text, t, fileName, lineNumber, level );
            return new LELogWithTrait( text, t, fileName, lineNumber, level, tags );
        }

        public static ILogEntry CreateOpenGroup( string text, DateTime t, LogLevel level, string fileName, int lineNumber, CKTrait tags = null, CKExceptionData ex = null )
        {
            if( ex == null )
            {
                if( tags == null ) return new LEOpenGroup( text, t, fileName, lineNumber, level );
                else return new LEOpenGroupWithTrait( text, t, fileName, lineNumber, level, tags );
            }
            return new LEOpenGroupWithException( text, t, fileName, lineNumber, level, tags, ex );
        }

        public static ILogEntry CreateCloseGroup( DateTime t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
        {
            return new LECloseGroup( t, level, c );
        }

        static public void WriteLog( BinaryWriter w, LogLevel level, DateTime logTimeUtc, string text, CKTrait tags, string fileName, int lineNumber )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            w.Write( (byte)((int)level << 2 | (int)StreamLogType.TypeLog) );
            w.Write( tags != null ? tags.ToString() : String.Empty );
            w.Write( text );
            w.Write( fileName );
            w.Write( lineNumber );
            w.Write( logTimeUtc.ToBinary() );
        }

        static public void WriteOpenGroup( BinaryWriter w, LogLevel level, DateTime logTimeUtc, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            StreamLogType o = ex == null ? StreamLogType.TypeOpenGroup : StreamLogType.TypeOpenGroupWithException;
            w.Write( (byte)((int)level << 2 | (int)o) );
            w.Write( tags != null ? tags.ToString() : String.Empty );
            w.Write( text );
            w.Write( fileName );
            w.Write( lineNumber );
            w.Write( logTimeUtc.ToBinary() );
            if( ex != null ) ex.Write( w );
        }

        static public void WriteCloseGroup( BinaryWriter w, LogLevel level, DateTime closeTimeUtc, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            w.Write( (byte)((int)level << 2 | (int)StreamLogType.TypeGroupClosed) );
            w.Write( closeTimeUtc.ToBinary() );
            w.Write( conclusions.Count );
            foreach( ActivityLogGroupConclusion c in conclusions )
            {
                w.Write( c.Tag.ToString() );
                w.Write( c.Text );
            }
        }

        /// <summary>
        /// Reads a <see cref="ILogeEntry"/> from the reader.
        /// If the first read byte is 0 (or has its two less significant bits set to 0), read stops and null is returned.
        /// The 0 byte is the "end marker" that <see cref="ActivityMonitorBinaryWriterClient.Close(bool)"/> can write.
        /// </summary>
        /// <param name="r">The binary reader.</param>
        /// <returns>The log entry or null if a zero byte (end marker) has been found.</returns>
        static public ILogEntry Read( BinaryReader r )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            int bH = r.ReadByte();
            StreamLogType h = (StreamLogType)(bH & 3);
            var logLevel = (LogLevel)(bH >> 2);
            switch( h )
            {
                case StreamLogType.TypeLog:
                    {
                        var tags = ActivityMonitor.RegisteredTags.FindOrCreate( r.ReadString() );
                        var text = r.ReadString();
                        var fileName = r.ReadString();
                        var lineNumber = r.ReadInt32();
                        var logTimeUtc = DateTime.FromBinary( r.ReadInt64() );
                        if( tags != ActivityMonitor.EmptyTag ) return new LELogWithTrait( text, logTimeUtc, fileName, lineNumber, logLevel, tags );
                        return new LELog( text, logTimeUtc, fileName, lineNumber, logLevel );
                    }
                case StreamLogType.TypeOpenGroup:
                    {
                        var tags = ActivityMonitor.RegisteredTags.FindOrCreate( r.ReadString() );
                        var text = r.ReadString();
                        var fileName = r.ReadString();
                        var lineNumber = r.ReadInt32();
                        var logTimeUtc = DateTime.FromBinary( r.ReadInt64() );
                        if( tags != ActivityMonitor.EmptyTag ) return new LEOpenGroupWithTrait( text, logTimeUtc, fileName, lineNumber, logLevel, tags );
                        return new LEOpenGroup( text, logTimeUtc, fileName, lineNumber, logLevel );
                    }
                case StreamLogType.TypeOpenGroupWithException:
                    {
                        var tags = ActivityMonitor.RegisteredTags.FindOrCreate( r.ReadString() );
                        var text = r.ReadString();
                        var fileName = r.ReadString();
                        var lineNumber = r.ReadInt32();
                        var logTimeUtc = DateTime.FromBinary( r.ReadInt64() );
                        var exception = new CKExceptionData( r );
                        return new LEOpenGroupWithException( text, logTimeUtc, fileName, lineNumber, logLevel, tags, exception );
                    }
                case StreamLogType.TypeGroupClosed:
                    {
                        DateTime time = DateTime.FromBinary( r.ReadInt64() );
                        int conclusionsCount = r.ReadInt32();
                        ActivityLogGroupConclusion[] conclusions;
                        if( conclusionsCount == 0 ) conclusions = Util.EmptyArray<ActivityLogGroupConclusion>.Empty;
                        else
                        {
                            conclusions = new ActivityLogGroupConclusion[conclusionsCount];
                            for( int i = 0; i < conclusionsCount; i++ )
                            {
                                CKTrait tags = ActivityMonitor.RegisteredTags.FindOrCreate( r.ReadString() );
                                string text = r.ReadString();
                                conclusions[i] = new ActivityLogGroupConclusion( text, tags );
                            }
                        }
                        return new LECloseGroup( time, logLevel, conclusions.AsReadOnlyList() );
                    }
                default: return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    public static class LogEntry
    {
        #region Unicast

        public static ILogEntry CreateLog( string text, DateTime t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LELog( text, t, fileName, lineNumber, level, tags, ex );
        }

        public static ILogEntry CreateOpenGroup( string text, DateTime t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LEOpenGroup( text, t, fileName, lineNumber, level, tags, ex );
        }

        public static ILogEntry CreateCloseGroup( DateTime t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
        {
            return new LECloseGroup( t, level, c );
        }

        #endregion

        #region Multicast

        public static IMulticastLogEntry CreateMulticastLog( Guid monitorId, int depth, string text, DateTime t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LEMCLog( monitorId, depth, text, t, fileName, lineNumber, level, tags, ex );
        }

        public static IMulticastLogEntry CreateMulticastOpenGroup( Guid monitorId, int depth, string text, DateTime t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LEMCOpenGroup( monitorId, depth, text, t, fileName, lineNumber, level, tags, ex );
        }

        public static IMulticastLogEntry CreateMulticastCloseGroup( Guid monitorId, int depth, DateTime t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
        {
            return new LEMCCloseGroup( monitorId, depth, t, level, c );
        }

        #endregion

        static public void WriteLog( BinaryWriter w, Guid monitorId, int depth, bool isOpenGroup, LogLevel level, DateTime logTimeUtc, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            DoWriteLog( w, StreamLogType.IsMultiCast | (isOpenGroup ? StreamLogType.TypeOpenGroup : StreamLogType.TypeLine), level, logTimeUtc, text, tags, ex, fileName, lineNumber );
            w.Write( monitorId.ToByteArray() );
            w.Write( depth );
        }

        static public void WriteLog( BinaryWriter w, bool isOpenGroup, LogLevel level, DateTime logTimeUtc, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            DoWriteLog( w, isOpenGroup ? StreamLogType.TypeOpenGroup : StreamLogType.TypeLine, level, logTimeUtc, text, tags, ex, fileName, lineNumber );
        }

        private static void DoWriteLog( BinaryWriter w, StreamLogType t, LogLevel level, DateTime logTimeUtc, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( tags != null && !tags.IsEmpty ) t |= StreamLogType.HasTags;
            if( ex != null )
            {
                t |= StreamLogType.HasException;
                if( text == ex.Message ) t |= StreamLogType.IsTextTheExceptionMessage;
            }
            if( fileName != null ) t |= StreamLogType.HasFileName;

            w.Write( (byte)t );
            w.Write( (byte)level );
            w.Write( logTimeUtc.ToBinary() );
            if( (t & StreamLogType.HasTags) != 0 ) w.Write( tags.ToString() );
            if( (t & StreamLogType.HasFileName) != 0 )
            {
                w.Write( fileName );
                w.Write( lineNumber );
            }
            if( (t & StreamLogType.HasException) != 0 ) ex.Write( w );
            if( (t & StreamLogType.IsTextTheExceptionMessage) == 0 ) w.Write( text );
        }

        static public void WriteCloseGroup( BinaryWriter w, LogLevel level, DateTime closeTimeUtc, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            DoWriteCloseGroup( w, StreamLogType.TypeGroupClosed, level, closeTimeUtc, conclusions );
        }

        static public void WriteCloseGroup( BinaryWriter w, Guid monitorId, int depth, LogLevel level, DateTime closeTimeUtc, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            DoWriteCloseGroup( w, StreamLogType.TypeGroupClosed|StreamLogType.IsMultiCast, level, closeTimeUtc, conclusions );
            w.Write( monitorId.ToByteArray() );
            w.Write( depth );
        }

        private static void DoWriteCloseGroup( BinaryWriter w, StreamLogType t, LogLevel level, DateTime closeTimeUtc, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( conclusions != null && conclusions.Count > 0 ) t |= StreamLogType.HasConclusions;
            w.Write( (byte)t );
            w.Write( (byte)level );
            w.Write( closeTimeUtc.ToBinary() );
            if( (t & StreamLogType.HasConclusions) != 0 )
            {
                w.Write( conclusions.Count );
                foreach( ActivityLogGroupConclusion c in conclusions )
                {
                    w.Write( c.Tag.ToString() );
                    w.Write( c.Text );
                }
            }
        }

        /// <summary>
        /// Reads a <see cref="ILogEntry"/> from the binary reader that can be a <see cref="IMulticastLogEntry"/>.
        /// If the first read byte is 0, read stops and null is returned.
        /// The 0 byte is the "end marker" that <see cref="ActivityMonitorBinaryWriterClient.Close(bool)"/> can write.
        /// </summary>
        /// <param name="r">The binary reader.</param>
        /// <returns>The log entry or null if a zero byte (end marker) has been found.</returns>
        static public ILogEntry Read( BinaryReader r )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            StreamLogType t = (StreamLogType)r.ReadByte();
            if( t == StreamLogType.EndOfStream ) return null;
            var logLevel = (LogLevel)r.ReadByte();
            
            if( (t & StreamLogType.TypeMask) == StreamLogType.TypeGroupClosed )
            {
                return ReadGroupClosed( r, t, logLevel );
            }
            var logTimeUtc = DateTime.FromBinary( r.ReadInt64() );
            CKTrait tags = ActivityMonitor.Tags.Empty;
            string fileName = null;
            int lineNumber = 0;
            CKExceptionData ex = null;
            string text = null;

            if( (t & StreamLogType.HasTags) != 0 ) tags = ActivityMonitor.Tags.Register( r.ReadString() );
            if( (t & StreamLogType.HasFileName) != 0 )
            {
                fileName = r.ReadString();
                lineNumber = r.ReadInt32();
            }
            if( (t & StreamLogType.HasException) != 0 )
            {
                ex = new CKExceptionData( r );
                if( (t & StreamLogType.IsTextTheExceptionMessage) == 0 ) text = ex.Message;
            }
            if( text == null ) text = r.ReadString();

            if( (t & StreamLogType.TypeMask) == StreamLogType.TypeLine )
            {
                if( (t & StreamLogType.IsMultiCast) == 0 )
                {
                    return new LELog( text, logTimeUtc, fileName, lineNumber, logLevel, tags, ex );
                }
                Guid mId1 = new Guid( r.ReadBytes( 16 ) );
                int depth1 = r.ReadInt32();
                return new LEMCLog( mId1, depth1, text, logTimeUtc, fileName, lineNumber, logLevel, tags, ex );
            }
            if( (t & StreamLogType.TypeMask) != StreamLogType.TypeOpenGroup ) throw new InvalidDataException();
            if( (t & StreamLogType.IsMultiCast) == 0 )
            {
                return new LEOpenGroup( text, logTimeUtc, fileName, lineNumber, logLevel, tags, ex );
            }
            Guid mId = new Guid( r.ReadBytes( 16 ) );
            int depth = r.ReadInt32();
            return new LEMCOpenGroup( mId, depth, text, logTimeUtc, fileName, lineNumber, logLevel, tags, ex );
        }

        private static ILogEntry ReadGroupClosed( BinaryReader r, StreamLogType t, LogLevel logLevel )
        {
            DateTime time = DateTime.FromBinary( r.ReadInt64() );
            ActivityLogGroupConclusion[] conclusions = Util.EmptyArray<ActivityLogGroupConclusion>.Empty;
            if( (t & StreamLogType.HasConclusions) != 0 )
            {
                int conclusionsCount = r.ReadInt32();
                conclusions = new ActivityLogGroupConclusion[conclusionsCount];
                for( int i = 0; i < conclusionsCount; i++ )
                {
                    CKTrait cTags = ActivityMonitor.Tags.Register( r.ReadString() );
                    string cText = r.ReadString();
                    conclusions[i] = new ActivityLogGroupConclusion( cText, cTags );
                }
            }
            if( (t & StreamLogType.IsMultiCast) == 0 )
            {
                return new LECloseGroup( time, logLevel, conclusions.AsReadOnlyList() );
            }
            Debug.Assert( Guid.Empty.ToByteArray().Length == 16 );
            Guid mId = new Guid( r.ReadBytes( 16 ) );
            int depth = r.ReadInt32();
            return new LEMCCloseGroup( mId, depth, time, logLevel, conclusions.AsReadOnlyList() );
        }
    }
}

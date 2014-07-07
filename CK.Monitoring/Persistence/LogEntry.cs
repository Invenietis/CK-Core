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
    /// <summary>
    /// Encapsulates <see cref="ILogEntry"/> concrete objects manipulation.
    /// </summary>
    public static class LogEntry
    {
        #region Unicast

        /// <summary>
        /// Creates a <see cref="ILogEntry"/> for a line.
        /// </summary>
        /// <param name="text">Text of the log entry.</param>
        /// <param name="t">Time stamp of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="fileName">Source file name of the log entry</param>
        /// <param name="lineNumber">Source line number of the log entry</param>
        /// <param name="tags">Tags of the log entry</param>
        /// <param name="ex">Exception of the log entry.</param>
        /// <returns>A log entry object.</returns>
        public static ILogEntry CreateLog( string text, DateTimeStamp t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LELog( text, t, fileName, lineNumber, level, tags, ex );
        }

        /// <summary>
        /// Creates a <see cref="ILogEntry"/> for an opened group.
        /// </summary>
        /// <param name="text">Text of the log entry.</param>
        /// <param name="t">Time stamp of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="fileName">Source file name of the log entry</param>
        /// <param name="lineNumber">Source line number of the log entry</param>
        /// <param name="tags">Tags of the log entry</param>
        /// <param name="ex">Exception of the log entry.</param>
        /// <returns>A log entry object.</returns>
        public static ILogEntry CreateOpenGroup( string text, DateTimeStamp t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LEOpenGroup( text, t, fileName, lineNumber, level, tags, ex );
        }

        /// <summary>
        /// Creates a <see cref="ILogEntry"/> for the closing of a group.
        /// </summary>
        /// <param name="t">Time stamp of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="c">Group conclusions.</param>
        /// <returns>A log entry object.</returns>
        public static ILogEntry CreateCloseGroup( DateTimeStamp t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
        {
            return new LECloseGroup( t, level, c );
        }

        #endregion

        #region Multi-cast

        /// <summary>
        /// Creates a <see cref="ILogEntry"/> for a line.
        /// </summary>
        /// <param name="monitorId">Identifier of the monitor.</param>
        /// <param name="previousEntryType">Log type of the previous entry in the monitor..</param>
        /// <param name="previousLogTime">Time stamp of the previous entry in the monitor.</param>
        /// <param name="depth">Depth of the line (number of opened groups above).</param>
        /// <param name="text">Text of the log entry.</param>
        /// <param name="t">Time stamp of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="fileName">Source file name of the log entry</param>
        /// <param name="lineNumber">Source line number of the log entry</param>
        /// <param name="tags">Tags of the log entry</param>
        /// <param name="ex">Exception of the log entry.</param>
        /// <returns>A log entry object.</returns>
        public static IMulticastLogEntry CreateMulticastLog( Guid monitorId, LogEntryType previousEntryType, DateTimeStamp previousLogTime, int depth, string text, DateTimeStamp t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LEMCLog( monitorId, depth, previousLogTime, previousEntryType, text, t, fileName, lineNumber, level, tags, ex );
        }

        /// <summary>
        /// Creates a <see cref="ILogEntry"/> for an opend group.
        /// </summary>
        /// <param name="monitorId">Identifier of the monitor.</param>
        /// <param name="previousEntryType">Log type of the previous entry in the monitor..</param>
        /// <param name="previousLogTime">Time stamp of the previous entry in the monitor.</param>
        /// <param name="depth">Depth of the line (number of opened groups above).</param>
        /// <param name="text">Text of the log entry.</param>
        /// <param name="t">Time stamp of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="fileName">Source file name of the log entry</param>
        /// <param name="lineNumber">Source line number of the log entry</param>
        /// <param name="tags">Tags of the log entry</param>
        /// <param name="ex">Exception of the log entry.</param>
        /// <returns>A log entry object.</returns>
        public static IMulticastLogEntry CreateMulticastOpenGroup( Guid monitorId, LogEntryType previousEntryType, DateTimeStamp previousLogTime, int depth, string text, DateTimeStamp t, LogLevel level, string fileName, int lineNumber, CKTrait tags, CKExceptionData ex )
        {
            return new LEMCOpenGroup( monitorId, depth, previousLogTime, previousEntryType, text, t, fileName, lineNumber, level, tags, ex );
        }

        /// <summary>
        /// Creates a <see cref="ILogEntry"/> for the closing of a group.
        /// </summary>
        /// <param name="monitorId">Identifier of the monitor.</param>
        /// <param name="previousEntryType">Log type of the previous entry in the monitor..</param>
        /// <param name="previousLogTime">Time stamp of the previous entry in the monitor.</param>
        /// <param name="depth">Depth of the line (number of opened groups above).</param>
        /// <param name="t">Time stamp of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="c">Group conclusions.</param>
        /// <returns>A log entry object.</returns>
        public static IMulticastLogEntry CreateMulticastCloseGroup( Guid monitorId, LogEntryType previousEntryType, DateTimeStamp previousLogTime, int depth, DateTimeStamp t, LogLevel level, IReadOnlyList<ActivityLogGroupConclusion> c )
        {
            return new LEMCCloseGroup( monitorId, depth, previousLogTime, previousEntryType, t, level, c );
        }

        #endregion

        /// <summary>
        /// Binary writes a multicast log entry.
        /// </summary>
        /// <param name="w">Binary writer to use.</param>
        /// <param name="monitorId">Identifier of the monitor.</param>
        /// <param name="previousEntryType">Log type of the previous entry in the monitor..</param>
        /// <param name="previousLogTime">Time stamp of the previous entry in the monitor.</param>
        /// <param name="depth">Depth of the line (number of opened groups above).</param>
        /// <param name="isOpenGroup">True if this the opening of a group. False for a line.</param>
        /// <param name="text">Text of the log entry.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="logTime">Time stamp of the log entry.</param>
        /// <param name="tags">Tags of the log entry</param>
        /// <param name="ex">Exception of the log entry.</param>
        /// <param name="fileName">Source file name of the log entry</param>
        /// <param name="lineNumber">Source line number of the log entry</param>
        static public void WriteLog( BinaryWriter w, Guid monitorId, LogEntryType previousEntryType, DateTimeStamp previousLogTime, int depth, bool isOpenGroup, LogLevel level, DateTimeStamp logTime, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            StreamLogType type = StreamLogType.IsMultiCast | (isOpenGroup ? StreamLogType.TypeOpenGroup : StreamLogType.TypeLine);
            type = UpdateTypeWithPrevious( type, previousEntryType, ref previousLogTime );
            DoWriteLog( w, type, level, logTime, text, tags, ex, fileName, lineNumber );
            WriteMulticastFooter( w, monitorId, previousEntryType, previousLogTime, depth );
        }

        /// <summary>
        /// Binary writes a log entry.
        /// </summary>
        /// <param name="w">Binary writer to use.</param>
        /// <param name="isOpenGroup">True if this the opening of a group. False for a line.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="text">Text of the log entry.</param>
        /// <param name="logTime">Time stamp of the log entry.</param>
        /// <param name="tags">Tags of the log entry</param>
        /// <param name="ex">Exception of the log entry.</param>
        /// <param name="fileName">Source file name of the log entry</param>
        /// <param name="lineNumber">Source line number of the log entry</param>
        static public void WriteLog( BinaryWriter w, bool isOpenGroup, LogLevel level, DateTimeStamp logTime, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            DoWriteLog( w, isOpenGroup ? StreamLogType.TypeOpenGroup : StreamLogType.TypeLine, level, logTime, text, tags, ex, fileName, lineNumber );
        }

        static void DoWriteLog( BinaryWriter w, StreamLogType t, LogLevel level, DateTimeStamp logTime, string text, CKTrait tags, CKExceptionData ex, string fileName, int lineNumber )
        {
            if( tags != null && !tags.IsEmpty ) t |= StreamLogType.HasTags;
            if( ex != null )
            {
                t |= StreamLogType.HasException;
                if( text == ex.Message ) t |= StreamLogType.IsTextTheExceptionMessage;
            }
            if( fileName != null ) t |= StreamLogType.HasFileName;
            if( logTime.Uniquifier != 0 ) t |= StreamLogType.HasUniquifier;

            WriteLogTypeAndLevel( w, t, level );
            w.Write( logTime.TimeUtc.ToBinary() );
            if( logTime.Uniquifier != 0 ) w.Write( logTime.Uniquifier );
            if( (t & StreamLogType.HasTags) != 0 ) w.Write( tags.ToString() );
            if( (t & StreamLogType.HasFileName) != 0 )
            {
                w.Write( fileName );
                w.Write( lineNumber );
            }
            if( (t & StreamLogType.HasException) != 0 ) ex.Write( w );
            if( (t & StreamLogType.IsTextTheExceptionMessage) == 0 ) w.Write( text );
        }

        /// <summary>
        /// Binary writes a closing entry.
        /// </summary>
        /// <param name="w">Binary writer to use.</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="closeTime">Time stamp of the group closing.</param>
        /// <param name="conclusions">Group conclusions.</param>
        static public void WriteCloseGroup( BinaryWriter w, LogLevel level, DateTimeStamp closeTime, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            DoWriteCloseGroup( w, StreamLogType.TypeGroupClosed, level, closeTime, conclusions );
        }

        /// <summary>
        /// Binary writes a multicast closing entry.
        /// </summary>
        /// <param name="w">Binary writer to use.</param>
        /// <param name="monitorId">Identifier of the monitor.</param>
        /// <param name="previousEntryType">Log type of the previous entry in the monitor..</param>
        /// <param name="previousLogTime">Time stamp of the previous entry in the monitor.</param>
        /// <param name="depth">Depth of the group (number of opened groups above).</param>
        /// <param name="level">Log level of the log entry.</param>
        /// <param name="closeTime">Time stamp of the group closing.</param>
        /// <param name="conclusions">Group conclusions.</param>
        static public void WriteCloseGroup( BinaryWriter w, Guid monitorId, LogEntryType previousEntryType, DateTimeStamp previousLogTime, int depth, LogLevel level, DateTimeStamp closeTime, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            StreamLogType type = StreamLogType.TypeGroupClosed | StreamLogType.IsMultiCast;
            type = UpdateTypeWithPrevious( type, previousEntryType, ref previousLogTime );
            DoWriteCloseGroup( w, type, level, closeTime, conclusions );
            WriteMulticastFooter( w, monitorId, previousEntryType, previousLogTime, depth );
        }

        static StreamLogType UpdateTypeWithPrevious( StreamLogType type, LogEntryType previousEntryType, ref DateTimeStamp previousStamp )
        {
            if( previousStamp.IsKnown )
            {
                type |= StreamLogType.IsPreviousKnown;
                if( previousEntryType == LogEntryType.None ) throw new ArgumentException( "Must not be None since previousStamp is known.", "previousEntryType" );
                if( previousStamp.Uniquifier != 0 ) type |= StreamLogType.IsPreviousKnownHasUniquifier;
            }
            return type;
        }

        static void WriteMulticastFooter( BinaryWriter w, Guid monitorId, LogEntryType previousEntryType, DateTimeStamp previousStamp, int depth )
        {
            w.Write( monitorId.ToByteArray() );
            w.Write( depth );
            if( previousStamp.IsKnown )
            {
                w.Write( previousStamp.TimeUtc.ToBinary() );
                if( previousStamp.Uniquifier != 0 ) w.Write( previousStamp.Uniquifier );
                w.Write( (byte)previousEntryType );
            }
        }

        static void DoWriteCloseGroup( BinaryWriter w, StreamLogType t, LogLevel level, DateTimeStamp closeTime, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( conclusions != null && conclusions.Count > 0 ) t |= StreamLogType.HasConclusions;
            if( closeTime.Uniquifier != 0 ) t |= StreamLogType.HasUniquifier;
            WriteLogTypeAndLevel( w, t, level );
            w.Write( closeTime.TimeUtc.ToBinary() );
            if( closeTime.Uniquifier != 0 ) w.Write( closeTime.Uniquifier );
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
        /// The 0 byte is the "end marker" that <see cref="CKMonWriterClient.Close()"/> write, but this
        /// method can read non zero-terminated streams (it catches an EndOfStreamException when reading the first byte and handles it silently).
        /// This method can throw any type of exception (<see cref="System.IO.EndOfStreamException"/> or <see cref="InvalidDataException"/> for instance) that
        /// must be handled by the caller.
        /// </summary>
        /// <param name="r">The binary reader.</param>
        /// <param name="streamVersion">The version of the stream.</param>
        /// <param name="badEndOfFile">True whenever the end of file is the result of an <see cref="EndOfStreamException"/>.</param>
        /// <returns>The log entry or null if a zero byte (end marker) has been found.</returns>
        static public ILogEntry Read( BinaryReader r, int streamVersion, out bool badEndOfFile )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            badEndOfFile = false;
            StreamLogType t = StreamLogType.EndOfStream;
            LogLevel logLevel = LogLevel.None;
            try
            {
                ReadLogTypeAndLevel( r, out t, out logLevel );
            }
            catch( System.IO.EndOfStreamException )
            {
                badEndOfFile = true;
                // Silently ignores here reading beyond the stream: this
                // kindly handles the lack of terminating 0 byte.
            }
            if( t == StreamLogType.EndOfStream ) return null;

            if( (t & StreamLogType.TypeMask) == StreamLogType.TypeGroupClosed )
            {
                return ReadGroupClosed( r, t, logLevel );
            }
            DateTimeStamp time = new DateTimeStamp( DateTime.FromBinary( r.ReadInt64() ), (t & StreamLogType.HasUniquifier) != 0 ? r.ReadByte() : (Byte)0 );
            if( time.TimeUtc.Year < 2014 || time.TimeUtc.Year > 3000 ) throw new InvalidDataException( "Date year before 2014 or after 3000 are considered invalid." );
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
                if( lineNumber > 100*1000 ) throw new InvalidDataException( "LineNumber greater than 100K is considered invalid." );
            }
            if( (t & StreamLogType.HasException) != 0 )
            {
                ex = new CKExceptionData( r );
                if( (t & StreamLogType.IsTextTheExceptionMessage) != 0 ) text = ex.Message;
            }
            if( text == null ) text = r.ReadString();

            Guid mId;
            int depth;
            LogEntryType prevType;
            DateTimeStamp prevTime;

            if( (t & StreamLogType.TypeMask) == StreamLogType.TypeLine )
            {
                if( (t & StreamLogType.IsMultiCast) == 0 )
                {
                    return new LELog( text, time, fileName, lineNumber, logLevel, tags, ex );
                }
                ReadMulticastFooter( r, t, out mId, out depth, out prevType, out prevTime );
                return new LEMCLog( mId, depth, prevTime, prevType, text, time, fileName, lineNumber, logLevel, tags, ex );
            }
            if( (t & StreamLogType.TypeMask) != StreamLogType.TypeOpenGroup ) throw new InvalidDataException();
            if( (t & StreamLogType.IsMultiCast) == 0 )
            {
                return new LEOpenGroup( text, time, fileName, lineNumber, logLevel, tags, ex );
            }
            ReadMulticastFooter( r, t, out mId, out depth, out prevType, out prevTime );
            return new LEMCOpenGroup( mId, depth, prevTime, prevType, text, time, fileName, lineNumber, logLevel, tags, ex );
        }

        static void ReadMulticastFooter( BinaryReader r, StreamLogType t, out Guid mId, out int depth, out LogEntryType prevType, out DateTimeStamp prevTime )
        {
            Debug.Assert( Guid.Empty.ToByteArray().Length == 16 );
            mId = new Guid( r.ReadBytes( 16 ) );
            depth = r.ReadInt32();
            if( depth < 0 ) throw new InvalidDataException();
            prevType = LogEntryType.None;
            prevTime = DateTimeStamp.Unknown;
            if( (t & StreamLogType.IsPreviousKnown) != 0 )
            {
                prevTime = new DateTimeStamp( DateTime.FromBinary( r.ReadInt64() ), (t & StreamLogType.IsPreviousKnownHasUniquifier) != 0 ? r.ReadByte() : (Byte)0 );
                prevType = (LogEntryType)r.ReadByte();
            }
        }

        static ILogEntry ReadGroupClosed( BinaryReader r, StreamLogType t, LogLevel logLevel )
        {
            DateTimeStamp time = new DateTimeStamp( DateTime.FromBinary( r.ReadInt64() ), (t & StreamLogType.HasUniquifier) != 0 ? r.ReadByte() : (Byte)0 );
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
            Guid mId;
            int depth;
            LogEntryType prevType;
            DateTimeStamp prevTime;
            ReadMulticastFooter( r, t, out mId, out depth, out prevType, out prevTime );

            return new LEMCCloseGroup( mId, depth, prevTime, prevType, time, logLevel, conclusions.AsReadOnlyList() );
        }

        static void WriteLogTypeAndLevel( BinaryWriter w, StreamLogType t, LogLevel level )
        {
            Debug.Assert( (int)StreamLogType.MaxFlag < (1 << 16) );
            Debug.Assert( (int)LogLevel.NumberOfBits < 8 );
            w.Write( (Byte)level );
            w.Write( (UInt16)t );
        }

        static void ReadLogTypeAndLevel( BinaryReader r, out StreamLogType t, out LogLevel l )
        {
            Debug.Assert( (int)StreamLogType.MaxFlag < (1 << 16) );
            Debug.Assert( (int)LogLevel.NumberOfBits < 8 );

            t = StreamLogType.EndOfStream;
            l = LogLevel.Trace;

            Byte level = r.ReadByte();
            // Found the 0 end marker?
            if( level != 0 )
            {
                if( level >= (1 << (int)LogLevel.NumberOfBits) ) throw new InvalidDataException();
                l = (LogLevel)level;

                UInt16 type = r.ReadUInt16();
                if( type >= ((int)StreamLogType.MaxFlag * 2 - 1) ) throw new InvalidDataException();
                t = (StreamLogType)type;
            }
        }

        static readonly string _missingLineText = "<Missing log data>";
        static readonly string _missingGroupText = "<Missing group>";
        static readonly IReadOnlyList<ActivityLogGroupConclusion> _missingConclusions = new CKReadOnlyListOnIList<ActivityLogGroupConclusion>( Util.EmptyArray<ActivityLogGroupConclusion>.Empty );

        static internal ILogEntry CreateMissingLine( DateTimeStamp knownTime )
        {
            Debug.Assert( !knownTime.IsInvalid );
            return new LELog( _missingLineText, knownTime, null, 0, LogLevel.None, ActivityMonitor.Tags.Empty, null );
        }

        static internal ILogEntry CreateMissingOpenGroup( DateTimeStamp knownTime )
        {
            Debug.Assert( !knownTime.IsInvalid );
            return new LEOpenGroup( _missingGroupText, knownTime, null, 0, LogLevel.None, ActivityMonitor.Tags.Empty, null );
        }

        static internal ILogEntry CreateMissingCloseGroup( DateTimeStamp knownTime )
        {
            Debug.Assert( !knownTime.IsInvalid );
            return new LECloseGroup( knownTime, LogLevel.None, _missingConclusions );
        }

        internal static bool IsMissingLogEntry( ILogEntry entry )
        {
            Debug.Assert( entry != null );
            return ReferenceEquals( entry.Text, _missingGroupText ) || ReferenceEquals( entry.Text, _missingLineText ) || entry.Conclusions == _missingConclusions;
        }
    }
}

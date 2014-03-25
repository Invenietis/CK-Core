using System;
using System.Collections.Generic;
using NLog;

namespace CK.Core.ActivityMonitorAdapters.NLogImpl
{
    /// <summary>
    /// Static utilities and extensions to create NLog entries (LogEventInfo) from ActivityMonitor or other entries.
    /// </summary>
    /// <remarks>
    /// Tree-like structure of ActivityMonitor is lost during the operation.
    /// </remarks>
    public static class NLogHelper
    {

        /// <summary>
        /// Creates an event info using NLog parameters.
        /// </summary>
        /// <param name="loggerName">Name of the NLog <see cref="Logger"/> name. <seealso cref="Logger.Name"/></param>
        /// <param name="level">NLog <see cref="NLog.LogLevel"/>.</param>
        /// <param name="message">Log entry message.</param>
        /// <param name="exception">Log entry exception.</param>
        /// <param name="logTime">Log entry TimeStamp.</param>
        /// <returns>Created LogEventInfo.</returns>
        public static LogEventInfo CreateEventInfo( string loggerName, NLog.LogLevel level, string message, Exception exception, DateTime logTime )
        {
            var li = new LogEventInfo()
            {
                Level = level,
                LoggerName = loggerName,
                Message = message,
                Exception = exception,
                TimeStamp = logTime,
            };
            return li;
        }

        /// <summary>
        /// Creates an event info using ActivityMonitor parameters.
        /// </summary>
        /// <param name="loggerName">Name of the NLog <see cref="Logger"/> name. <seealso cref="Logger.Name"/></param>
        /// <param name="level">ActivityMonitor <see cref="CK.Core.LogLevel"/>.</param>
        /// <param name="message">Log entry message.</param>
        /// <param name="exception">Log entry exception.</param>
        /// <param name="logTime">ActivityMonitor log entry <see cref="DateTimeStamp"/>.</param>
        /// <returns>Created LogEventInfo.</returns>
        public static LogEventInfo CreateEventInfo( string loggerName, CK.Core.LogLevel level, string message, Exception exception, DateTimeStamp logTime )
        {
            var li = new LogEventInfo()
            {
                Level = CKLogLevelToNLogLevel( level ),
                LoggerName = loggerName,
                Message = message,
                Exception = exception,
                TimeStamp = logTime.TimeUtc
            };
            return li;
        }

        /// <summary>
        /// Creates a LogEventInfo from an ActivityMonitor log entry.
        /// </summary>
        /// <param name="loggerName">Name of the NLog <see cref="Logger"/> name. <seealso cref="Logger.Name"/></param>
        /// <param name="logData">ActivityMonitor entry.</param>
        /// <returns>Created LogEventInfo.</returns>
        public static LogEventInfo CreateEventInfo( string loggerName, ActivityMonitorLogData logData )
        {
            string message = logData.Tags == ActivityMonitor.Tags.Empty ? logData.Text : String.Format( "[{0}] {1}", logData.Tags.ToString(), logData.Text );
            return CreateEventInfo( loggerName, logData.MaskedLevel, message, logData.Exception, logData.LogTime );
        }

        /// <summary>
        /// Creates a LogEventInfo from an ActivityMonitor group entry.
        /// </summary>
        /// <param name="loggerName">Name of the NLog <see cref="Logger"/> name. <seealso cref="Logger.Name"/></param>
        /// <param name="logGroup">ActivityMonitor group entry.</param>
        /// <param name="tag">Tag to add next to the ActivityMonitor line header.</param>
        /// <returns>Created LogEventInfo.</returns>
        public static LogEventInfo CreateEventInfo( string loggerName, IActivityLogGroup logGroup, string tag )
        {
            string entryText = String.Format( "[ActivityMonitor {1}] {0}", logGroup.GroupText, tag );
            return CreateEventInfo( loggerName, logGroup.MaskedGroupLevel, entryText, logGroup.Exception,
                logGroup.CloseLogTime > DateTimeStamp.MinValue ? logGroup.CloseLogTime : logGroup.LogTime );
        }

        /// <summary>
        /// Converts an ActivityMonitor's <see cref="CK.Core.LogLevel"/> into a <see cref="NLog.LogLevel"/>.
        /// </summary>
        /// <param name="level">ActivityMonitor LogLevel</param>
        /// <returns>NLog LogLevel</returns>
        public static NLog.LogLevel CKLogLevelToNLogLevel( CK.Core.LogLevel level )
        {
            level = level & CK.Core.LogLevel.Mask;

            switch( level )
            {
                case CK.Core.LogLevel.Trace: return NLog.LogLevel.Trace;
                case CK.Core.LogLevel.Info: return NLog.LogLevel.Info;
                case CK.Core.LogLevel.Warn: return NLog.LogLevel.Warn;
                case CK.Core.LogLevel.Error: return NLog.LogLevel.Error;
                case CK.Core.LogLevel.Fatal: return NLog.LogLevel.Fatal;
                default: return NLog.LogLevel.Debug;
            }
        }

        /// <summary>
        /// Logs an ActivityMonitorLogEntry to this NLog logger.
        /// </summary>
        /// <param name="this">NLog logger</param>
        /// <param name="data">Entry to log</param>
        public static void LogActivityMonitorEntry( this Logger @this, ActivityMonitorLogData data )
        {
            @this.Log( CreateEventInfo( @this.Name, data ) );
        }

        /// <summary>
        /// Logs an ActivityMonitor topic change to this NLog logger.
        /// </summary>
        /// <param name="this">NLog logger</param>
        /// <param name="newTopic">New topic to log.</param>
        /// <param name="fileName">Name of the file calling the topic change.</param>
        /// <param name="lineNumber">Line of the file calling the topic change.</param>
        public static void LogActivityMonitorTopicChanged( this Logger @this, string newTopic, string fileName, int lineNumber )
        {
            @this.Info( "[ActivityMonitor] Log topic changed: {0}", newTopic );
        }

        /// <summary>
        /// Logs an ActivityMonitor OpenGroup entry to this NLog logger.
        /// Note: Group and tree structure is lost.
        /// </summary>
        /// <param name="this">NLog logger.</param>
        /// <param name="group">Opened group entry</param>
        public static void LogActivityMonitorOpenGroup( this Logger @this, IActivityLogGroup group )
        {
            @this.Log( CreateEventInfo( @this.Name, group, "OpenGroup" ) );
        }

        /// <summary>
        /// Logs an ActivityMonitor ClosedGroup entry to this NLog logger, additionally logging ActivityMonitor conclusions if any were found.
        /// </summary>
        /// <param name="this">NLog logger</param>
        /// <param name="group">Closing group</param>
        /// <param name="conclusions">Conclusions</param>
        public static void LogActivityMonitorGroupClosed( this Logger @this, IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            @this.Log( CreateEventInfo( @this.Name, group, "GroupClosed" ) );
            if( conclusions.Count > 0 ) @this.Log( CreateEventInfo( @this.Name, group.MaskedGroupLevel, conclusions.ToStringGroupConclusion(), null, group.CloseLogTime ) );
        }
        public static void LogActivityMonitorAutoTagsChanged( this Logger @this, CKTrait newTrait )
        {
            @this.Trace( "[ActivityMonitor] AutoTags Changed to: {0}", newTrait.ToString() );
        }
    }
}

using System;
using System.Collections.Generic;
using Common.Logging;

namespace CK.Core.ActivityMonitorAdapters.CommonLoggingImpl
{
    /// <summary>
    /// Static utilities and extensions to create Common.Logging entries from ActivityMonitor or other entries.
    /// </summary>
    /// <remarks>
    /// Tree-like structure of ActivityMonitor is lost during the operation.
    /// </remarks>
    public static class NLogHelper
    {
        static string CreateMessageFromEntry( ActivityMonitorLogData logData )
        {
            return logData.Tags == ActivityMonitor.Tags.Empty ? logData.Text : String.Format( "[{0}] {1}", logData.Tags.ToString(), logData.Text );
        }

        static string CreateMessageFromGroup( IActivityLogGroup logGroup, string tag )
        {
            return String.Format( "[ActivityMonitor {1}] {0}",
                logGroup.GroupTags == ActivityMonitor.Tags.Empty ? logGroup.GroupText : String.Format( "[{0}] {1}", logGroup.GroupTags.ToString(), logGroup.GroupText ),
                tag );
        }

        /// <summary>
        /// Converts an ActivityMonitor's <see cref="CK.Core.LogLevel"/> into a <see cref="Common.Logging.LogLevel"/>.
        /// </summary>
        /// <param name="level">ActivityMonitor LogLevel</param>
        /// <returns>Common.Logging LogLevel</returns>
        public static Common.Logging.LogLevel CKLogLevelToCommonLogLevel( CK.Core.LogLevel level )
        {
            level = level & CK.Core.LogLevel.Mask;

            switch( level )
            {
                case CK.Core.LogLevel.Trace: return Common.Logging.LogLevel.Trace;
                case CK.Core.LogLevel.Info: return Common.Logging.LogLevel.Info;
                case CK.Core.LogLevel.Warn: return Common.Logging.LogLevel.Warn;
                case CK.Core.LogLevel.Error: return Common.Logging.LogLevel.Error;
                case CK.Core.LogLevel.Fatal: return Common.Logging.LogLevel.Fatal;
                default: return Common.Logging.LogLevel.Debug;
            }
        }

        /// <summary>
        /// Logs an ActivityMonitorLogEntry to this Common.Logging logger.
        /// </summary>
        /// <param name="this">Common.Logging logger</param>
        /// <param name="data">Entry to log</param>
        public static void LogActivityMonitorEntry( this ILog @this, ActivityMonitorLogData data )
        {
            @this.DoLog( data.MaskedLevel, CreateMessageFromEntry(data), data.Exception );
        }

        /// <summary>
        /// Logs an ActivityMonitor topic change to this Common.Logging logger.
        /// </summary>
        /// <param name="this">Common.Logging logger</param>
        /// <param name="newTopic">New topic to log.</param>
        /// <param name="fileName">Name of the file calling the topic change.</param>
        /// <param name="lineNumber">Line of the file calling the topic change.</param>
        public static void LogActivityMonitorTopicChanged( this ILog @this, string newTopic, string fileName, int lineNumber )
        {
            @this.InfoFormat( "[ActivityMonitor] Log topic changed: {0}", newTopic );
        }

        /// <summary>
        /// Logs an ActivityMonitor OpenGroup entry to this Common.Logging logger.
        /// Note: Group and tree structure is lost.
        /// </summary>
        /// <param name="this">Common.Logging logger.</param>
        /// <param name="group">Opened group entry</param>
        public static void LogActivityMonitorOpenGroup( this ILog @this, IActivityLogGroup group )
        {
            @this.DoLog( group.MaskedGroupLevel, CreateMessageFromGroup( group, "OpenGroup" ), group.Exception );
        }

        /// <summary>
        /// Logs an ActivityMonitor ClosedGroup entry to this Common.Logging logger, additionally logging ActivityMonitor conclusions if any were found.
        /// </summary>
        /// <param name="this">This Common.Logging logger</param>
        /// <param name="group">Closing group</param>
        /// <param name="conclusions">Conclusions</param>
        public static void LogActivityMonitorGroupClosed( this ILog @this, IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            @this.DoLog( group.MaskedGroupLevel, CreateMessageFromGroup( group, "OpenGroup" ), null );

            if( conclusions.Count > 0 ) @this.DoLog( group.MaskedGroupLevel, String.Format( "[Group conclusions] {0}", conclusions.ToStringGroupConclusion() ), null );
        }

        /// <summary>
        /// Logs a change in <see cref="IActivityMonitor.AutoTags"/>.
        /// </summary>
        /// <param name="this">This Common.Logging logger</param>
        /// <param name="newTrait">The changed tags.</param>
        public static void LogActivityMonitorAutoTagsChanged( this ILog @this, CKTrait newTrait )
        {
            @this.TraceFormat( "[ActivityMonitor] AutoTags Changed to: {0}", newTrait.ToString() );
        }

        static void DoLog( this ILog @this, CK.Core.LogLevel level, string message, Exception e )
        {
            @this.DoLog( CKLogLevelToCommonLogLevel( level ), message, e );
        }

        static void DoLog( this ILog @this, Common.Logging.LogLevel level, string message, Exception e )
        {
            switch( level )
            {
                case Common.Logging.LogLevel.Trace:
                    @this.Trace( message, e ); break;
                case Common.Logging.LogLevel.Info:
                    @this.Info( message, e ); break;
                case Common.Logging.LogLevel.Warn:
                    @this.Warn( message, e ); break;
                case Common.Logging.LogLevel.Error:
                    @this.Error( message, e ); break;
                case Common.Logging.LogLevel.Fatal:
                    @this.Fatal( message, e ); break;
                case Common.Logging.LogLevel.Debug:
                    @this.Debug( message, e ); break;
                default:
                    @this.Info( message, e ); break;
            }
        }
    }
}

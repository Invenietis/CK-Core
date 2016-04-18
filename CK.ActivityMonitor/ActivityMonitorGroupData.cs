using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Data required by <see cref="IActivityMonitor.UnfilteredOpenGroup"/>.
    /// </summary>
    public class ActivityMonitorGroupData : ActivityMonitorLogData
    {
        Func<string> _getConclusion;

        internal Func<string> GetConclusionText
        {
            get { return _getConclusion; }
            set { _getConclusion = value; }
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorGroupData"/>.
        /// </summary>
        /// <param name="level">Log level. Can not be <see cref="LogLevel.None"/>.</param>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.Tags"/>) to associate to the log. It will be union-ed with the current <see cref="IActivityMonitor.AutoTags"/>.</param>
        /// <param name="text">Text of the log. Can be null or empty only if <paramref name="exception"/> is not null: the <see cref="Exception.Message"/> is the text.</param>
        /// <param name="logTime">
        /// Time of the log.
        /// You may use <see cref="DateTimeStamp.UtcNow"/> or <see cref="ActivityMonitorExtension.NextLogTime">IActivityMonitor.NextLogTime()</see> extension method.
        /// </param>
        /// <param name="exception">Exception of the log. Can be null.</param>
        /// <param name="getConclusionText">Optional function that provides delayed obtention of the group conclusion: will be called on group closing.</param>
        /// <param name="fileName">Name of the source file that emitted the log. Can be null.</param>
        /// <param name="lineNumber">Line number in the source file that emitted the log. Can be null.</param>
        public ActivityMonitorGroupData( LogLevel level, CKTrait tags, string text, DateTimeStamp logTime, Exception exception, Func<string> getConclusionText, string fileName, int lineNumber )
            : base( level, exception, tags, text, logTime, fileName, lineNumber )
        {
            _getConclusion = getConclusionText;
        }

        /// <summary>
        /// Preinitializes a new <see cref="ActivityMonitorLogData"/>: <see cref="Initialize"/> has yet to be called.
        /// </summary>
        /// <param name="level">Log level. Can not be <see cref="LogLevel.None"/>.</param>
        /// <param name="fileName">Name of the source file that emitted the log. Can be null.</param>
        /// <param name="lineNumber">Line number in the source file that emitted the log. Can be null.</param>
        public ActivityMonitorGroupData( LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
        }

        /// <summary>
        /// Initializes a mere new <see cref="ActivityMonitorGroupData"/> without any actual data.
        /// Should be unsed only for rejected opened group.
        /// </summary>
        public ActivityMonitorGroupData()
        {
        }

        /// <summary>
        /// Initializes this group data.
        /// </summary>
        /// <param name="text">Text of the log. Can be null or empty only if <paramref name="exception"/> is not null: the <see cref="Exception.Message"/> is the text.</param>
        /// <param name="exception">Exception of the log. Can be null.</param>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.Tags"/>) to associate to the log. It will be union-ed with the current <see cref="IActivityMonitor.AutoTags"/>.</param>
        /// <param name="logTime">
        /// Time of the log.
        /// You may use <see cref="DateTimeStamp.UtcNow"/> or <see cref="ActivityMonitorExtension.NextLogTime">IActivityMonitor.NextLogTime()</see> extension method.
        /// </param>
        /// <param name="getConclusionText">Optional function that provides delayed obtention of the group conclusion: will be called on group closing.</param>
        public void Initialize( string text, Exception exception, CKTrait tags, DateTimeStamp logTime, Func<string> getConclusionText )
        {
            base.Initialize( text, exception, tags, logTime );
            _getConclusion = getConclusionText;
        }

        /// <summary>
        /// Calls <see cref="GetConclusionText"/> and sets it to null.
        /// </summary>
        internal string ConsumeConclusionText()
        {
            string autoText = null;
            if( _getConclusion != null )
            {
                try
                {
                    autoText = _getConclusion();
                }
                catch( Exception ex )
                {
                    autoText = String.Format( Impl.ActivityMonitorResources.ActivityMonitorErrorWhileGetConclusionText, ex.Message );
                }
                _getConclusion = null;
            }
            return autoText;
        }

    }
}

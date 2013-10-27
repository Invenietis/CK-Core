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
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.RegisteredTags"/>) to associate to the log. It will be unioned with the current <see cref="IActivityMonitor.AutoTags"/>.</param>
        /// <param name="text">Text of the log. Can be null or empty only if <paramref name="exception"/> is not null: the <see cref="Exception.Message"/> is the text.</param>
        /// <param name="logTimeUtc">Date and time of the log. Must be in UTC.</param>
        /// <param name="exception">Exception of the log. Can be null.</param>
        /// <param name="getConclusionText">Optional function that provides delayed obtention of the group conclusion: will be called on group closing.</param>
        /// <param name="fileName">Name of the source file that emitted the log. Can be null.</param>
        /// <param name="lineNumber">Line number in the source filethat emitted the log. Can be null.</param>
        public ActivityMonitorGroupData( LogLevel level, CKTrait tags, string text, DateTime logTimeUtc, Exception exception, Func<string> getConclusionText, string fileName, int lineNumber )
            : base( level, exception, tags, text, logTimeUtc, fileName, lineNumber )
        {
            _getConclusion = getConclusionText;
        }

        internal ActivityMonitorGroupData( LogLevel level, string fileName, int lineNumber )
            : base( level, fileName, lineNumber )
        {
        }

        /// <summary>
        /// Used only to initialize a ActivityMonitorGroupSender for rejected opened group.
        /// </summary>
        internal ActivityMonitorGroupData()
        {
        }

        internal void Initialize( string text, Exception exception, CKTrait tags, DateTime logTimeUtc, Func<string> getConclusionText )
        {
            base.Initialize( text, exception, tags, logTimeUtc );
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
                    autoText = String.Format( R.ActivityMonitorErrorWhileGetConclusionText, ex.Message );
                }
                _getConclusion = null;
            }
            return autoText;
        }

    }
}

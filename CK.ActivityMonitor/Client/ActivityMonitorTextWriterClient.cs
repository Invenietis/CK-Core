using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using CK.Text;

namespace CK.Core
{
    /// <summary>
    /// Formats the activity and pushes piece of texts to an <see cref="Action{T}"/> where T is a string.
    /// </summary>
    public class ActivityMonitorTextWriterClient : ActivityMonitorTextHelperClient
    {
        readonly Action<string> _writer;
        readonly StringBuilder _buffer;
        string _prefix;
        string _prefixLevel;
        CKTrait _currentTags;

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to a 
        /// function that must write a string, with a filter.
        /// </summary>
        /// <param name="writer">Function that writes the content.</param>
        /// <param name="filter">Filter to apply</param>
        public ActivityMonitorTextWriterClient( Action<string> writer, LogFilter filter )
            : base( filter )
        {
            if( writer == null ) throw new ArgumentNullException( "writer" );
            _writer = writer;
            _buffer = new StringBuilder();
            _prefixLevel = _prefix = String.Empty;
            _currentTags = ActivityMonitor.Tags.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="ActivityMonitorTextWriterClient"/> bound to a 
        /// function that must write a string.
        /// </summary>
        /// <param name="writer">Function that writes the content.</param>
        public ActivityMonitorTextWriterClient( Action<string> writer )
            : this( writer, LogFilter.Undefined )
        {
        }

        /// <summary>
        /// Writes all the information.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected override void OnEnterLevel( ActivityMonitorLogData data )
        {
            var w = _buffer.Clear();
            _prefixLevel = _prefix + new string( ' ', data.MaskedLevel.ToString().Length + 4 );

            w.Append( _prefix )
                .Append( "- " )
                .Append( data.MaskedLevel.ToString() )
                .Append( ": " )
                .AppendMultiLine( _prefixLevel, data.Text, false );

            if( _currentTags != data.Tags )
            {
                w.Append( " -[" ).Append( data.Tags ).Append( ']' );
                _currentTags = data.Tags;
            }
            w.AppendLine();
            if( data.Exception != null )
            {
                DumpException( w, _prefix, !data.IsTextTheExceptionMessage, data.Exception );
            }
            _writer( w.ToString() );
        }

        /// <summary>
        /// Writes all information.
        /// </summary>
        /// <param name="data">Log data.</param>
        protected override void OnContinueOnSameLevel( ActivityMonitorLogData data )
        {
            var w = _buffer.Clear();
            w.AppendMultiLine( _prefixLevel, data.Text, true );
            if( _currentTags != data.Tags )
            {
                w.Append( " -[" ).Append( data.Tags ).Append( ']' );
                _currentTags = data.Tags;
            }
            w.AppendLine();
            if( data.Exception != null )
            {
                DumpException( w, _prefix, !data.IsTextTheExceptionMessage, data.Exception );
            }

            _writer( _buffer.ToString() );
        }

        /// <summary>
        /// Updates the internally maintained prefix for lines.
        /// </summary>
        /// <param name="level">Previous level.</param>
        protected override void OnLeaveLevel( LogLevel level )
        {
            Debug.Assert( (level & LogLevel.IsFiltered) == 0 );
            _prefixLevel = _prefix;
        }

        /// <summary>
        /// Writes a group opening.
        /// </summary>
        /// <param name="g">Group information.</param>
        protected override void OnGroupOpen( IActivityLogGroup g )
        {
            var w = _buffer.Clear();
            string levelLabel = g.MaskedGroupLevel.ToString();
            string start = string.Format( "{0}> {1}: ", _prefix, levelLabel );
            _prefix += "|  ";
            _prefixLevel = _prefix;
            string prefixLabel = _prefixLevel + new string( ' ', levelLabel.Length + 1 );

            w.Append( start ).AppendMultiLine( prefixLabel, g.GroupText, false );
            if( _currentTags != g.GroupTags )
            {
                w.Append( " -[" ).Append( g.GroupTags ).Append( ']' );
                _currentTags = g.GroupTags;
            }
            w.AppendLine();
            if( g.Exception != null )
            {
                DumpException( w, _prefix, !g.IsGroupTextTheExceptionMessage, g.Exception );
            }
            _writer( _buffer.ToString() );
        }

        /// <summary>
        /// Writes group conclusion and updates internally managed line prefix.
        /// </summary>
        /// <param name="g">Group that must be closed.</param>
        /// <param name="conclusions">Conclusions for the group.</param>
        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            _prefixLevel = _prefix = _prefix.Remove( _prefix.Length - 3 );
            if( conclusions.Count == 0 ) return;
            var w = _buffer.Clear();
            bool one = false;
            List<ActivityLogGroupConclusion> withMultiLines = null;
            foreach( var c in conclusions )
            {
                if( c.Text.Contains( '\n' ) )
                {
                    if( withMultiLines == null ) withMultiLines = new List<ActivityLogGroupConclusion>();
                    withMultiLines.Add( c );
                }
                else
                {
                    if( !one )
                    {
                        w.Append( _prefixLevel ).Append( "< " );
                        one = true;
                    }
                    else w.Append( " - " );
                    w.Append( c.Text );
                }
            }
            if( one ) w.AppendLine();
            if( withMultiLines != null )
            {
                foreach( var c in withMultiLines )
                {
                    w.Append( _prefixLevel ).Append( "< " );
                    w.AppendMultiLine( _prefixLevel + "  ", c.Text, false );
                    w.AppendLine();
                }
            }
            _writer( _buffer.ToString() );
        }

        /// <summary>
        /// Recursively dumps an <see cref="Exception"/> as readable text.
        /// </summary>
        /// <param name="w">The TextWriter to write to.</param>
        /// <param name="prefix">Prefix that will start all lines.</param>
        /// <param name="displayMessage">Whether the exception message must be displayed or skip.</param>
        /// <param name="ex">The exception to display.</param>
        static public void DumpException( StringBuilder w, string prefix, bool displayMessage, Exception ex )
        {
            CKException ckEx = ex as CKException;
            if( ckEx != null && ckEx.ExceptionData != null )
            {
                ckEx.ExceptionData.ToStringBuilder( w, prefix );
                return;
            }

            string header = String.Format( " ┌──────────────────────────■ Exception : {0} ■──────────────────────────", ex.GetType().Name );

            string p;
            w.AppendLine( prefix + header );
            string localPrefix = prefix + " | ";
            if( displayMessage && ex.Message != null )
            {
                w.Append( localPrefix + "Message: " );
                w.AppendMultiLine( localPrefix + "         ", ex.Message, false );
                w.AppendLine();
            }
            if( ex.StackTrace != null )
            {
                w.Append( localPrefix + "Stack: " );
                w.AppendMultiLine( localPrefix + "       ", ex.StackTrace, false );
                w.AppendLine();
            }
            var fileNFEx = ex as System.IO.FileNotFoundException;
            if( fileNFEx != null )
            {
                if( !String.IsNullOrEmpty( fileNFEx.FileName ) ) w.AppendLine( localPrefix + "FileName: " + fileNFEx.FileName );
                #if NET451 || NET46
                if( fileNFEx.FusionLog != null )
                {
                    w.Append( localPrefix + "FusionLog: " );
                    w.AppendMultiLine( localPrefix + "         ", fileNFEx.FusionLog, false );
                    w.AppendLine();
                }
                #endif
            }
            else
            {
                var loadFileEx = ex as System.IO.FileLoadException;
                if( loadFileEx != null )
                {
                    if( !String.IsNullOrEmpty( loadFileEx.FileName ) ) w.AppendLine( localPrefix + "FileName: " + loadFileEx.FileName );
                    #if NET451 || NET46
                    if( loadFileEx.FusionLog != null )
                    {
                        w.Append( localPrefix + "FusionLog: " );
                        w.AppendMultiLine( localPrefix + "         ", loadFileEx.FusionLog, false );
                        w.AppendLine();
                    }
                    #endif
                }
                else
                {
                    var typeLoadEx = ex as ReflectionTypeLoadException;
                    if( typeLoadEx != null )
                    {
                        w.AppendLine( localPrefix + " ┌──────────────────────────■ [Loader Exceptions] ■──────────────────────────" );
                        p = localPrefix + " | ";
                        foreach( var item in typeLoadEx.LoaderExceptions )
                        {
                            DumpException( w, p, true, item );
                        }
                        w.AppendLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
                    }
                    #if NET451 || NET46
                    else
                    {
                        var configEx = ex as System.Configuration.ConfigurationException;
                        if( configEx != null )
                        {
                            if( !String.IsNullOrEmpty( configEx.Filename ) ) w.AppendLine( localPrefix + "FileName: " + configEx.Filename );
                        }
                    }
                    #endif
                }
            }
            // The InnerException of an aggregated exception is the same as the first of it InnerExceptionS.
            // (The InnerExceptionS are the contained/aggregated exceptions of the AggregatedException object.)
            // This is why, if we are on an AggregatedException we do not follow its InnerException.
            var aggrex = ex as AggregateException;
            if( aggrex != null && aggrex.InnerExceptions.Count > 0 )
            {
                w.AppendLine( localPrefix + " ┌──────────────────────────■ [Aggregated Exceptions] ■──────────────────────────" );
                p = localPrefix + " | ";
                foreach( var item in aggrex.InnerExceptions )
                {
                    DumpException( w, p, true, item );
                }
                w.AppendLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            else if( ex.InnerException != null )
            {
                w.AppendLine( localPrefix + " ┌──────────────────────────■ [Inner Exception] ■──────────────────────────" );
                p = localPrefix + " | ";
                DumpException( w, p, true, ex.InnerException );
                w.AppendLine( localPrefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            w.AppendLine( prefix + " └" + new string( '─', header.Length - 2 ) );
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Sink the logs to the console.
    /// </summary>
    public class ActivityLoggerConsoleSync : IActivityLoggerSink
    {
        string _prefix;
        string _prefixLevel;

        /// <summary>
        /// Initializes a new <see cref="ActivityLoggerConsoleSync"/>.
        /// </summary>
        public ActivityLoggerConsoleSync()
        {
            _prefixLevel = _prefix = String.Empty;
        }

        void IActivityLoggerSink.OnEnterLevel( LogLevel level, string text )
        {
            Console.Write( _prefix + "- " + level.ToString() + ": " );
            _prefixLevel = _prefix + new String( ' ', level.ToString().Length + 4 );
            Console.WriteLine( text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel ) );
        }

        void IActivityLoggerSink.OnContinueOnSameLevel( LogLevel level, string text )
        {
            Console.WriteLine( _prefixLevel + text.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel ) );
        }

        void IActivityLoggerSink.OnLeaveLevel( LogLevel level )
        {
            _prefixLevel = _prefix;
        }

        void IActivityLoggerSink.OnGroupOpen( IActivityLogGroup g )
        {
            Console.Write( "{0}▪►-{1}: ", _prefix, g.GroupLevel.ToString() );
            _prefix += "▪  ";
            _prefixLevel = _prefix;
            Console.WriteLine( g.GroupText.Replace( Environment.NewLine, Environment.NewLine + _prefixLevel ) );
        }

        void IActivityLoggerSink.OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( g.Exception != null )
            {
                DumpException( !g.IsGroupTextTheExceptionMessage, g.Exception );
            }
            _prefixLevel = _prefix = _prefix.Remove( _prefix.Length - 3 );
            foreach( var c in conclusions )
            {
                string text = "◄▪-" + c.Conclusion;
                Console.WriteLine( _prefixLevel + text.Replace( _prefixLevel + Environment.NewLine, Environment.NewLine + _prefixLevel + "   " ) );
            }
        }

        void DumpException( bool displayMessage, Exception ex )
        {
            string p;

            Console.WriteLine( _prefix + " ┌──────────────────────────■ Exception ■──────────────────────────" );
            _prefix += " | ";
            if( displayMessage && ex.Message != null )
            {
                Console.Write( _prefix + "Message: " );
                p = _prefix + "         ";
                Console.WriteLine( ex.Message.Replace( Environment.NewLine, Environment.NewLine + p ) );
            }
            if( ex.StackTrace != null )
            {
                Console.Write( _prefix + "Stack: " );
                p = _prefix + "       ";
                Console.WriteLine( ex.StackTrace.Replace( Environment.NewLine, Environment.NewLine + p ) );
            }
            if( ex.InnerException != null )
            {
                Console.WriteLine( _prefix + " ┌──────────────────────────▪ [Inner Exception] ▪──────────────────────────" );
                _prefix += " | ";
                DumpException( true, ex.InnerException );
                _prefix = _prefix.Remove( _prefix.Length - 3 );
                Console.WriteLine( _prefix + " └─────────────────────────────────────────────────────────────────────────" );
            }
            _prefix = _prefix.Remove( _prefix.Length - 3 );
            Console.WriteLine( _prefix + " └─────────────────────────────────────────────────────────────────────────" );
        }

    }
}

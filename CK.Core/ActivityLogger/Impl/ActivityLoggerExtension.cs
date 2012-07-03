#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerExtension.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityLogger"/> and other types from the Activity logger framework.
    /// </summary>
    public static class ActivityLoggerExtension
    {
        /// <summary>
        /// Gets this Group conclusions as a readeable string.
        /// </summary>
        /// <param name="this">This group conclusion.</param>
        /// <param name="conclusionSeparator">Conclusion separator.</param>
        /// <returns>A lovely concatened string of conclusions.</returns>
        public static string ToStringGroupConclusion( this IEnumerable<ActivityLogGroupConclusion> @this, string conclusionSeparator = " - " )
        {
            if( @this == null ) return String.Empty;
            StringBuilder b = new StringBuilder();
            foreach( var e in @this )
            {
                if( b.Length > 0 ) b.Append( conclusionSeparator );
                b.Append( e.Conclusion );
            }
            return b.ToString();
        }

        /// <summary>
        /// Gets the path as a readable string.
        /// </summary>
        /// <param name="this">This path.</param>
        /// <param name="elementSeparator">Between elements.</param>
        /// <param name="withoutConclusionFormat">There must be 3 placeholders {0} for the level, {1} for the text and {2} for the conclusion.</param>
        /// <param name="withConclusionFormat">There must be 2 placeholders {0} for the level and {1} for the text.</param>
        /// <param name="conclusionSeparator">Conclusion separator.</param>
        /// <param name="fatal">For Fatal errors.</param>
        /// <param name="error">For Errors.</param>
        /// <param name="warn">For Warnings.</param>
        /// <param name="info">For Infos.</param>
        /// <param name="trace">For Traces.</param>
        /// <returns>A lovely path.</returns>
        public static string ToStringPath( this IEnumerable<ActivityLoggerPathCatcher.PathElement> @this,
            string elementSeparator = "> ",
            string withoutConclusionFormat = "{0}{1} ",
            string withConclusionFormat = "{0}{1} -{{ {2} }}",
            string conclusionSeparator = " - ",
            string fatal = "[Fatal]- ",
            string error = "[Error]- ",
            string warn = "[Warning]- ",
            string info = "[Info]- ",
            string trace = "" )
        {
            if( @this == null ) return String.Empty;
            StringBuilder b = new StringBuilder();
            foreach( var e in @this )
            {
                if( b.Length > 0 ) b.Append( elementSeparator );
                string prefix = trace;
                switch( e.Level )
                {
                    case LogLevel.Fatal: prefix = fatal; break;
                    case LogLevel.Error: prefix = error; break;
                    case LogLevel.Warn: prefix = warn; break;
                    case LogLevel.Info: prefix = info; break;
                }
                if( e.GroupConclusion != null ) b.AppendFormat( withConclusionFormat, prefix, e.Text, e.GroupConclusion.ToStringGroupConclusion( conclusionSeparator ) );
                else b.AppendFormat( withoutConclusionFormat, prefix, e.Text );
            }
            return b.ToString();
        }

        /// <summary>
        /// Concatenation of <see cref="IActivityLoggerClientRegistrar.RegisteredClients">RegisteredClients</see> 
        /// and <see cref="IMuxActivityLoggerClientRegistrar.RegisteredMuxClients">RegisteredMuxClients</see>
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerOutput"/>.</param>
        /// <returns>The enumeration of all output clients.</returns>
        public static IEnumerable<IActivityLoggerClientBase> AllClients( this IActivityLoggerOutput @this )
        {
            return @this.RegisteredClients.Cast<IActivityLoggerClientBase>().Concat( @this.RegisteredMuxClients );
        }

        #region Registrar

        /// <summary>
        /// Registers multiple <see cref="IActivityLoggerClientRegistrar"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerClientRegistrar"/> object.</param>
        /// <param name="clients">Multiple clients to register.</param>
        /// <returns>This registrar to enable fluent syntax.</returns>
        public static IActivityLoggerClientRegistrar Register( this IActivityLoggerClientRegistrar @this, IEnumerable<IActivityLoggerClient> clients )
        {
            foreach( var c in clients ) @this.RegisterClient( c );
            return @this;
        }

        /// <summary>
        /// Registers multiple <see cref="IActivityLoggerClientRegistrar"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLoggerClientRegistrar"/> object.</param>
        /// <param name="clients">Multiple clients to register.</param>
        /// <returns>This registrar to enable fluent syntax.</returns>
        public static IActivityLoggerClientRegistrar Register( this IActivityLoggerClientRegistrar @this, params IActivityLoggerClient[] clients )
        {
            return Register( @this, (IEnumerable<IActivityLoggerClient>)clients );
        }

        /// <summary>
        /// Registers multiple <see cref="IMuxActivityLoggerClientRegistrar"/>.
        /// </summary>
        /// <param name="this">This <see cref="IMuxActivityLoggerClientRegistrar"/> object.</param>
        /// <param name="clients">Multiple clients to register.</param>
        /// <returns>This registrar to enable fluent syntax.</returns>
        public static IMuxActivityLoggerClientRegistrar Register( this IMuxActivityLoggerClientRegistrar @this, IEnumerable<IMuxActivityLoggerClient> clients )
        {
            foreach( var c in clients ) @this.RegisterMuxClient( c );
            return @this;
        }

        /// <summary>
        /// Registers multiple <see cref="IMuxActivityLoggerClientRegistrar"/>.
        /// </summary>
        /// <param name="this">This <see cref="IMuxActivityLoggerClientRegistrar"/> object.</param>
        /// <param name="clients">Multiple clients to register.</param>
        /// <returns>This registrar to enable fluent syntax.</returns>
        public static IMuxActivityLoggerClientRegistrar Register( this IMuxActivityLoggerClientRegistrar @this, params IMuxActivityLoggerClient[] clients )
        {
            return Register( @this, (IEnumerable<IMuxActivityLoggerClient>)clients );
        }
        #endregion

        #region IActivityLogger.Filter( level )
        
        class LogFilterSentinel : IDisposable
        {
            IActivityLogger _logger;
            LogLevelFilter _prevLevel;

            public LogFilterSentinel( IActivityLogger l, LogLevelFilter filterLevel )
            {
                _prevLevel = l.Filter;
                _logger = l;
                l.Filter = filterLevel;
            }

            public void Dispose()
            {
                _logger.Filter = _prevLevel;
            }

        }

        /// <summary>
        /// Sets a filter level on this <see cref="IActivityLogger"/>. The current <see cref="IActivityLogger.Filter"/> will be automatically 
        /// restored when the returned <see cref="IDisposable"/> will be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="filterLevel">The new filter level.</param>
        /// <returns>A <see cref="IDisposable"/> object that will restore the current level.</returns>
        public static IDisposable Filter( this IActivityLogger @this, LogLevelFilter filterLevel )
        {
            return new LogFilterSentinel( @this, filterLevel );
        }

        #endregion IActivityLogger.Filter( level )

        #region IActivityLogger.OpenGroup( ... )

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">The log level of the group.</param>
        /// <param name="text">The text associated to the opening of the log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, string text )
        {
            return @this.OpenGroup( level, null, text );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing.</param>
        /// <param name="format">A composite format for the group title.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Func<string> getConclusionText, string format, params object[] arguments )
        {
            return @this.OpenGroup( level, getConclusionText, String.Format( format, arguments ) );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, string format, params object[] arguments )
        {
            return @this.OpenGroup( level, null, String.Format( format, arguments ) );
        }

        #endregion

        #region IActivityLogger Trace(...), Info(...), Warn(...), Error(...) and Fatal(...).

        #region Trace

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a trace.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, text );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace )
            {
                if( arg0 is Exception ) throw new ArgumentException( "Possible use of the wrong overload: Use the form that takes a first parameter of type Exception and then the string text instead of this ( string format, string arg0 ) overload to log the exception, or calls this overload explicitely with the Exception.Message string.", "arg0" );
                @this.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0 ) );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0, arg1 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0, arg1, arg2 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, args ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, text() );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace<T>( this IActivityLogger @this, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, text( param ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace<T1, T2>( this IActivityLogger @this, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, text( param1, param2 ) );
            return @this;
        }
        #endregion

        #region Info

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as an info.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, text );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info )
            {
                if( arg0 is Exception ) throw new ArgumentException( "Possible use of the wrong overload: Use the form that takes a first parameter of type Exception and then the string text instead of this ( string format, string arg0 ) overload to log the exception, or calls this overload explicitely with the Exception.Message string.", "arg0" );
                @this.UnfilteredLog( LogLevel.Info, String.Format( format, arg0 ) );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, arg0, arg1 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, arg0, arg1, arg2 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, args ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, text() );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info<T>( this IActivityLogger @this, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, text( param ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info<T1, T2>( this IActivityLogger @this, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, text( param1, param2 ) );
            return @this;
        }
        #endregion

        #region Warn

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a warning.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, text );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn )
            {
                if( arg0 is Exception ) throw new ArgumentException( "Possible use of the wrong overload: Use the form that takes a first parameter of type Exception and then the string text instead of this ( string format, string arg0 ) overload to log the exception, or calls this overload explicitely with the Exception.Message string.", "arg0" );
                @this.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0 ) );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0, arg1 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0, arg1, arg2 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, args ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, text() );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn<T>( this IActivityLogger @this, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, text( param ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn<T1, T2>( this IActivityLogger @this, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, text( param1, param2 ) );
            return @this;
        }
        #endregion

        #region Error

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as an error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, text );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error )
            {
                if( arg0 is Exception ) throw new ArgumentException( "Possible use of the wrong overload: Use the form that takes a first parameter of type Exception and then the string text instead of this ( string format, string arg0 ) overload to log the exception, or calls this overload explicitely with the Exception.Message string.", "arg0" );
                @this.UnfilteredLog( LogLevel.Error, String.Format( format, arg0 ) );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, arg0, arg1 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, arg0, arg1, arg2 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, args ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, text() );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error<T>( this IActivityLogger @this, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, text( param ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error<T1, T2>( this IActivityLogger @this, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, text( param1, param2 ) );
            return @this;
        }
        #endregion

        #region Fatal

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a fatal error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, text );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal )
            {
                if( arg0 is Exception ) throw new ArgumentException( "Possible use of the wrong overload: Use the form that takes a first parameter of type Exception and then the string text instead of this ( string format, string arg0 ) overload to log the exception, or calls this overload explicitely with the Exception.Message string.", "arg0" );
                @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0 ) );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0, arg1 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0, arg1, arg2 ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, args ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, text() );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal<T>( this IActivityLogger @this, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, text( param ) );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal<T1, T2>( this IActivityLogger @this, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, text( param1, param2 ) );
            return @this;
        }
        #endregion

        #endregion

        #region IActivityLogger Trace( Exception ), Warn( Exception ), Error( Exception ), Fatal( Exception ), OpenGroup( Exception ).

        #region Trace

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, null, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as a trace.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, text, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( LogLevel.Trace, String.Format( format, args ), ex );
            return @this;
        }

        #endregion

        #region Info

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, null, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as an information.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, text, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, arg0 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( LogLevel.Info, String.Format( format, args ), ex );
            return @this;
        }

        #endregion

        #region Warn

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, null, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as a warning.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, text, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( LogLevel.Warn, String.Format( format, args ), ex );
            return @this;
        }

        #endregion

        #region Error

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, null, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as an error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, text, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, arg0 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( LogLevel.Error, String.Format( format, args ), ex );
            return @this;
        }

        #endregion

        #region Fatal

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, null, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as a fatal error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, text, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, arg0, arg1 ), ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( LogLevel.Fatal, String.Format( format, args ), ex );
            return @this;
        }

        #endregion

        #region OpenGroup

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Exception ex )
        {
            return @this.OpenGroup( level, null, null, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">The group title.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Exception ex, string text )
        {
            return @this.OpenGroup( level, null, text, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Exception ex, string format, object arg0 )
        {
            return @this.OpenGroup( level, null, String.Format( format, arg0 ), ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Exception ex, string format, object arg0, object arg1 )
        {
            return @this.OpenGroup( level, null, String.Format( format, arg0, arg1 ), ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            return @this.OpenGroup( level, null, String.Format( format, arg0, arg1, arg2 ), ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">A composite format for the group title.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, LogLevel level, Exception ex, string format, params object[] arguments )
        {
            return @this.OpenGroup( level, null, String.Format( format, arguments ), ex );
        }

        #endregion

        #endregion
    }
}

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
    /// Provides extension methods for <see cref="IActivityLogger"/> to open groups and emit logs with tags.
    /// </summary>
    public static partial class ActivityLoggerExtension
    {
        #region IActivityLogger.OpenGroup( ... )

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">The log level of the group.</param>
        /// <param name="text">The text associated to the opening of the log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, string text )
        {
            return @this.OpenGroup( tags, level, null, text, DateTime.UtcNow );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
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
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Func<string> getConclusionText, string format, params object[] arguments )
        {
            return @this.OpenGroup( tags, level, getConclusionText, String.Format( format, arguments ), DateTime.UtcNow );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, string format, params object[] arguments )
        {
            return @this.OpenGroup( tags, level, null, String.Format( format, arguments ), DateTime.UtcNow );
        }

        #endregion

        #region IActivityLogger Trace(...), Info(...), Warn(...), Error(...) and Fatal(...).

        #region Trace

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a trace.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, text, DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace )
            {
                if( arg0 is Exception ) throw new ArgumentException( R.PossibleWrongOverloadUseWithException, "arg0" );
                @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, arg0 ), DateTime.UtcNow );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, arg0, arg1 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, args ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, text(), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace<T>( this IActivityLogger @this, CKTrait tags, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, text( param ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Trace"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace<T1, T2>( this IActivityLogger @this, CKTrait tags, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, text( param1, param2 ), DateTime.UtcNow );
            return @this;
        }
        #endregion

        #region Info

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as an info.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, text, DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info )
            {
                if( arg0 is Exception ) throw new ArgumentException( R.PossibleWrongOverloadUseWithException, "arg0" );
                @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, arg0 ), DateTime.UtcNow );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, arg0, arg1 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an info.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, args ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, text(), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info<T>( this IActivityLogger @this, CKTrait tags, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, text( param ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info<T1, T2>( this IActivityLogger @this, CKTrait tags, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, text( param1, param2 ), DateTime.UtcNow );
            return @this;
        }
        #endregion

        #region Warn

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a warning.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, text, DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn )
            {
                if( arg0 is Exception ) throw new ArgumentException( R.PossibleWrongOverloadUseWithException, "arg0" );
                @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, arg0 ), DateTime.UtcNow );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, arg0, arg1 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, args ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, text(), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary><param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn<T>( this IActivityLogger @this, CKTrait tags, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, text( param ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn<T1, T2>( this IActivityLogger @this, CKTrait tags, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, text( param1, param2 ), DateTime.UtcNow );
            return @this;
        }
        #endregion

        #region Error

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as an error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, text, DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error )
            {
                if( arg0 is Exception ) throw new ArgumentException( R.PossibleWrongOverloadUseWithException, "arg0" );
                @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, arg0 ), DateTime.UtcNow );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, arg0, arg1 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, args ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, text(), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error<T>( this IActivityLogger @this, CKTrait tags, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, text( param ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error<T1, T2>( this IActivityLogger @this, CKTrait tags, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, text( param1, param2 ), DateTime.UtcNow );
            return @this;
        }
        #endregion

        #region Fatal

        /// <summary>
        /// Logs the text if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Text to log as a fatal error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, text, DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with one placeholder/parameter if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal )
            {
                if( arg0 is Exception ) throw new ArgumentException( R.PossibleWrongOverloadUseWithException, "arg0" );
                @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, arg0 ), DateTime.UtcNow );
            }
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with two placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, arg0, arg1 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with three placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text with placeholders/parameters if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, args ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Func<string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, text(), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <typeparam name="T">Type of the parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="param">Parameter of the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal<T>( this IActivityLogger @this, CKTrait tags, T param, Func<T, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, text( param ), DateTime.UtcNow );
            return @this;
        }

        /// <summary>
        /// Logs a formatted text by calling a delegate if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Fatal"/> or above.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter that <paramref name="text"/> accepts.</typeparam>
        /// <typeparam name="T2">Type of the second parameter that <paramref name="text"/> accepts.</typeparam>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="param1">First parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="param2">Second parameter for the <paramref name="text"/> delegate.</param>
        /// <param name="text">Delegate that returns a string.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal<T1, T2>( this IActivityLogger @this, CKTrait tags, T1 param1, T2 param2, Func<T1, T2, string> text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, text( param1, param2 ), DateTime.UtcNow );
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
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, null, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as a trace.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, text, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, arg0 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a trace.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a trace.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Trace( this IActivityLogger @this, CKTrait tags, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Trace ) @this.UnfilteredLog( tags, LogLevel.Trace, String.Format( format, args ), DateTime.UtcNow, ex );
            return @this;
        }

        #endregion

        #region Info

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, null, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as an information.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, text, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, arg0 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an information if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Info"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an information.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Info( this IActivityLogger @this, CKTrait tags, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Info ) @this.UnfilteredLog( tags, LogLevel.Info, String.Format( format, args ), DateTime.UtcNow, ex );
            return @this;
        }

        #endregion

        #region Warn

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, null, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as a warning.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, text, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, arg0 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as a warning if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Warn"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a warning.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Warn( this IActivityLogger @this, CKTrait tags, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Warn ) @this.UnfilteredLog( tags, LogLevel.Warn, String.Format( format, args ), DateTime.UtcNow, ex );
            return @this;
        }

        #endregion

        #region Error

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, null, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as an error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, text, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, arg0 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception as an error if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevel.Error"/> or above.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as an error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Error( this IActivityLogger @this, CKTrait tags, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Error ) @this.UnfilteredLog( tags, LogLevel.Error, String.Format( format, args ), DateTime.UtcNow, ex );
            return @this;
        }

        #endregion

        #region Fatal

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Exception ex )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, null, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">Text to log as a fatal error.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Exception ex, string text )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, text, DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, arg0 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
            return @this;
        }

        /// <summary>
        /// Logs the exception (except if current <see cref="IActivityLogger.Filter"/> is <see cref="LogLevelFilter.Off"/>).
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="format">Text format to log as a fatal error.</param>
        /// <param name="args">Multiple parameters to format.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public static IActivityLogger Fatal( this IActivityLogger @this, CKTrait tags, Exception ex, string format, params object[] args )
        {
            if( (int)@this.Filter <= (int)LogLevel.Fatal ) @this.UnfilteredLog( tags, LogLevel.Fatal, String.Format( format, args ), DateTime.UtcNow, ex );
            return @this;
        }

        #endregion

        #region OpenGroup

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Exception ex )
        {
            return @this.OpenGroup( tags, level, null, null, DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">The group title.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Exception ex, string text )
        {
            return @this.OpenGroup( tags, level, null, text, DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Exception ex, string format, object arg0 )
        {
            return @this.OpenGroup( tags, level, null, String.Format( format, arg0 ), DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
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
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Exception ex, string format, object arg0, object arg1 )
        {
            return @this.OpenGroup( tags, level, null, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
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
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            return @this.OpenGroup( tags, level, null, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityLogger.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityLogger"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityLogger.Filter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">A composite format for the group title.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityLogger.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityLogger @this, CKTrait tags, LogLevel level, Exception ex, string format, params object[] arguments )
        {
            return @this.OpenGroup( tags, level, null, String.Format( format, arguments ), DateTime.UtcNow, ex );
        }

        #endregion

        #endregion
    }
}

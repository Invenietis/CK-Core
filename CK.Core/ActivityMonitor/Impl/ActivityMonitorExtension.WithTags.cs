#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorExtension.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityMonitor"/> to open groups and emit logs with tags.
    /// </summary>
    public static partial class ActivityMonitorExtension
    {
        #region IActivityMonitor.OpenGroup( ... )

        /// <summary>
        /// Opens a log level. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="level">The log level of the group.</param>
        /// <param name="text">The text associated to the opening of the log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, string text )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, text, DateTime.UtcNow, null );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing.</param>
        /// <param name="format">A composite format for the group title.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Func<string> getConclusionText, string format, params object[] arguments )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, getConclusionText, String.Format( format, arguments ), DateTime.UtcNow, null );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, string format, object arg0 )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arg0 ), DateTime.UtcNow, null );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Parameter to format (placeholder {1}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, string format, object arg0, object arg1 )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arg0, arg1 ), DateTime.UtcNow, null );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Parameter to format (placeholder {2}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, string format, object arg0, object arg1, object arg2 )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, null );
        }

        /// <summary>
        /// Opens a log level. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="format">Format of the string.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, string format, params object[] arguments )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arguments ), DateTime.UtcNow, null );
        }

        #endregion

        #region OpenGroup

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="ex">The exception to log.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Exception ex )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, null, DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="text">The group title.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Exception ex, string text )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, text, DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">Parameter to format (placeholder {0}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Exception ex, string format, object arg0 )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arg0 ), DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Exception ex, string format, object arg0, object arg1 )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arg0, arg1 ), DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">Text format for group title.</param>
        /// <param name="arg0">First parameter to format (placeholder {0}).</param>
        /// <param name="arg1">Second parameter to format (placeholder {1}).</param>
        /// <param name="arg2">Third parameter to format (placeholder {2}).</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Exception ex, string format, object arg0, object arg1, object arg2 )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arg0, arg1, arg2 ), DateTime.UtcNow, ex );
        }

        /// <summary>
        /// Opens a log level associated to an <see cref="Exception"/>. <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/> object.</param>
        /// <param name="tags">Tags to associate to the log.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="IActivityMonitor.ActualFilter">Filter</see> is ignored.</param>
        /// <param name="ex">Exception to log.</param>
        /// <param name="format">A composite format for the group title.</param>
        /// <param name="arguments">Arguments to format.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="IActivityMonitor.CloseGroup">CloseGroup</see> is called.
        /// </remarks>
        public static IDisposable OpenGroup( this IActivityMonitor @this, CKTrait tags, LogLevel level, Exception ex, string format, params object[] arguments )
        {
            return FilteredGroup( @this, level & LogLevel.Mask ) ?? @this.UnfilteredOpenGroup( tags, level | LogLevel.IsFiltered, null, String.Format( format, arguments ), DateTime.UtcNow, ex );
        }

        #endregion

    }
}

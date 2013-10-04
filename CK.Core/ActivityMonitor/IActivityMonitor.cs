#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\IActivityMonitor.cs) is part of CiviKey. 
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
using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Defines the core Activity Monitor interface. Small is beautiful. 
    /// </summary>
    /// <remarks>
    /// This is not the same as a classical logging framework: the "activity" captures by an activity monitor is structured. 
    /// It can be seen as a "Story Writer": its output can be displayed to an end user (even if some structured information 
    /// can easily be injected).
    /// Furthemore, activities can be tracked (with the help of the developper of course) accross threads, tasks or application domain.
    /// </remarks>
    public interface IActivityMonitor
    {
        /// <summary>
        /// Gets or sets the tags of this monitor: any subsequent logs will be tagged by these tags.
        /// The <see cref="CKTrait"/> must be registered in <see cref="ActivityMonitor.RegisteredTags"/>.
        /// Modifications to this property are scoped to the current Group since when a Group is closed, this
        /// property (and <see cref="Filter"/>) is automatically restored to its original value (captured when the Group was opened).
        /// </summary>
        CKTrait AutoTags { get; set; }

        /// <summary>
        /// Gets or sets a filter for the log level.
        /// Modifications to this property are scoped to the current Group since when a Group is closed, this
        /// property (and <see cref="AutoTags"/>) is automatically restored to its original value (captured when the Group was opened).
        /// </summary>
        LogLevelFilter Filter { get; set; }
        
        /// <summary>
        /// Gets the actual filter level for logs: this combines the configured <see cref="Filter"/> and the minimal requirements
        /// of any <see cref="IActivityMonitorBoundClient"/> that specifies such a minimal filter level.
        /// </summary>
        /// <remarks>
        /// This does NOT take into account the static (application-domain) <see cref="ActivityMonitor.DefaultFilter"/>.
        /// This global default must be used if this ActualFilter is <see cref="LogLevelFilter.None"/>: the <see cref="ActivityMonitorExtension.ShouldLog">ShouldLog</see>
        /// extension method takes it into account.
        /// </remarks>
        LogLevelFilter ActualFilter { get; }

        /// <summary>
        /// Logs a text regardless of <see cref="ActalFilter"/> level. 
        /// Each call to log is considered as a unit of text: depending on the rendering engine, a line or a 
        /// paragraph separator (or any appropriate separator) should be appended between each text if 
        /// the <paramref name="level"/> is the same as the previous one.
        /// See remarks.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.RegisteredTags"/>) to associate to the log, unioned with current <see cref="AutoTags"/>.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log. Ignored if null or empty.</param>
        /// <param name="logTimeUtc">Timestamp of the log entry (must be UTC).</param>
        /// <param name="ex">Optional exception associated to the log. When not null, a Group is automatically created.</param>
        /// <returns>This monitor to enable fluent syntax.</returns>
        /// <remarks>
        /// A null or empty <paramref name="text"/> is not logged.
        /// If needed, the special text <see cref="ActivityMonitor.ParkLevel"/> ("PARK-LEVEL") breaks the current <see cref="LogLevel"/>
        /// and resets it: the next log, even with the same LogLevel, will be treated as if a different LogLevel is used.
        /// </remarks>
        IActivityMonitor UnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc, Exception ex = null );

        /// <summary>
        /// Opens a log level. <see cref="CloseGroup"/> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="tags">Tags (from <see cref="ActivityMonitor.RegisteredTags"/>) to associate to the log, unioned with current <see cref="AutoTags"/>.</param>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="ActualFilter"/> is ignored.</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing.</param>
        /// <param name="text">Text to log (the title of the group). Null text is valid and considered as <see cref="String.Empty"/> or assigned to the <see cref="Exception.Message"/> if it exists.</param>
        /// <param name="logTimeUtc">Timestamp of the log entry (must be UTC).</param>
        /// <param name="ex">Optional exception associated to the group.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not filtered since any subordinated logs may occur with a much higher level.
        /// It is left to the implementation to handle (or not) filtering when <see cref="CloseGroup"/> is called.
        /// </remarks>
        IDisposable OpenGroup( CKTrait tags, LogLevel level, Func<string> getConclusionText, string text, DateTime logTimeUtc, Exception ex = null );

        /// <summary>
        /// Closes the current Group. Optional parameter is polymorphic. It can be a string, a <see cref="ActivityLogGroupConclusion"/>, 
        /// a <see cref="List{T}"/> or an <see cref="IEnumerable{T}"/> of ActivityLogGroupConclusion, or any object with an overriden <see cref="Object.ToString"/> method. 
        /// See remarks (especially for List&lt;ActivityLogGroupConclusion&gt;).
        /// </summary>
        /// <param name="userConclusion">Optional string, ActivityLogGroupConclusion object, enumerable of ActivityLogGroupConclusion or object to conclude the group. See remarks.</param>
        /// <param name="logTimeUtc">Timestamp of the closing of the group.</param>
        /// <remarks>
        /// An untyped object is used here to easily and efficiently accomodate both string and already existing ActivityLogGroupConclusion.
        /// When a List&lt;ActivityLogGroupConclusion&gt; is used, it will be direclty used to collect conclusion objects (new conclusions will be added to it). This is an optimization.
        /// </remarks>
        void CloseGroup( DateTime logTimeUtc, object userConclusion = null );

        /// <summary>
        /// Gets the <see cref="IActivityMonitorOutput"/> for this monitor.
        /// </summary>
        IActivityMonitorOutput Output { get; }
    }

}

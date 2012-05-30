#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerHybridClient.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Base class that explicitely implements <see cref="IActivityLoggerClient"/> (to hide it from public interface)
    /// and <see cref="IMuxActivityLoggerClient"/> that redirects all of its calls to the single logger client 
    /// implementation: must be used when multiple origin loggers can be ignored (log streams are merged regardless
    /// of their originator <see cref="IActivityLogger"/>).
    /// </summary>
    public class ActivityLoggerHybridClient : IActivityLoggerClient, IMuxActivityLoggerClient 
    {
        /// <summary>
        /// Empty <see cref="IActivityLoggerClient"/> and <see cref="IMuxActivityLoggerClient"/> (null object design pattern).
        /// </summary>
        public static readonly ActivityLoggerHybridClient Empty = new ActivityLoggerHybridClient();

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerHybridClient"/> that does nothing.
        /// </summary>
        public ActivityLoggerHybridClient()
        {
        }

        /// <summary>
        /// Called when <see cref="IActivityLogger.Filter"/> is about to change.
        /// Does nothing by default.
        /// </summary>
        /// <param name="current">Current level filter.</param>
        /// <param name="newValue">The new level filter.</param>
        protected virtual void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
        }

        /// <summary>
        /// Called for each <see cref="IActivityLogger.UnfilteredLog"/>.
        /// Does nothing by default.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        protected virtual void OnUnfilteredLog( LogLevel level, string text )
        {
        }

        /// <summary>
        /// Called for each <see cref="IActivityLogger.OpenGroup"/>.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        protected virtual void OnOpenGroup( IActivityLogGroup group )
        {
        }

        /// <summary>
        /// Called once the conclusion is known at the group level (if it exists, the <see cref="ActivityLogGroupConclusion.Emitter"/> is the <see cref="IActivityLogger"/> itself) 
        /// but before the group is actually closed: clients can update the conclusions for the group.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">Mutable conclusions associated to the closing group.</param>
        protected virtual void OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
        }

        /// <summary>
        /// Called when the group is actually closed.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Text that conclude the group. Never null but can be empty.</param>
        protected virtual void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
        }

        #region IActivityLoggerClient Members

        void IActivityLoggerClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            OnFilterChanged( current, newValue );
        }

        void IActivityLoggerClient.OnUnfilteredLog( LogLevel level, string text )
        {
            OnUnfilteredLog( level, text );
        }

        void IActivityLoggerClient.OnOpenGroup( IActivityLogGroup group )
        {
            OnOpenGroup( group );
        }

        void IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosing( group, conclusions );
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosed( group, conclusions );
        }

        #endregion

        #region IMuxActivityLoggerClient relayed to protected implementation.

        void IMuxActivityLoggerClient.OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue )
        {
            OnFilterChanged( current, newValue );
        }

        void IMuxActivityLoggerClient.OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text )
        {
            OnUnfilteredLog( level, text );
        }

        void IMuxActivityLoggerClient.OnOpenGroup( IActivityLogger sender, IActivityLogGroup group )
        {
            OnOpenGroup( group );
        }

        void IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, IList<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosing( group, conclusions );
        }

        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosed( group, conclusions );
        }

        #endregion

    }
}

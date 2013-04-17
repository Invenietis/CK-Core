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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Base class that explicitely implements <see cref="IActivityLoggerClient"/> (to hide it from public interface).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ActivityLoggerClient : IActivityLoggerClient
    {
        /// <summary>
        /// Empty <see cref="IActivityLoggerClient"/> (null object design pattern).
        /// </summary>
        public static readonly ActivityLoggerClient Empty = new ActivityLoggerClient();

        /// <summary>
        /// Initialize a new <see cref="ActivityLoggerClient"/> that does nothing.
        /// </summary>
        public ActivityLoggerClient()
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
        /// <param name="tags">Tags (from <see cref="ActivityLogger.RegisteredTags"/>) associated to the log.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        /// <param name="logTimeUtc">Timestamp of the log.</param>
        protected virtual void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
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
        /// Called once the user conclusions and the <see cref="ActivityLogger.Group.GetConclusionText"/> are known at the group level but before 
        /// the group is actually closed: clients can update the conclusions for the group.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">
        /// Mutable conclusions associated to the closing group. 
        /// This can be null if no conclusions have been added yet. 
        /// It is up to the first client that wants to add a conclusion to instanciate a new List object to carry the conclusions.
        /// </param>
        protected virtual void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }

        /// <summary>
        /// Called when the group is actually closed.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Text that conclude the group. Never null but can be empty.</param>
        protected virtual void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
        }

        #region IActivityLoggerClient Members

        void IActivityLoggerClient.OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            OnFilterChanged( current, newValue );
        }

        void IActivityLoggerClient.OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            OnUnfilteredLog( tags, level, text, logTimeUtc );
        }

        void IActivityLoggerClient.OnOpenGroup( IActivityLogGroup group )
        {
            OnOpenGroup( group );
        }

        void IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosing( group, ref conclusions );
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            OnGroupClosed( group, conclusions );
        }

        #endregion

    }
}

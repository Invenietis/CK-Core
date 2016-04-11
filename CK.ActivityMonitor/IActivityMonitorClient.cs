using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Listener for <see cref="IActivityMonitor"/> registered in a <see cref="IActivityMonitorOutput"/>.
    /// </summary>
    public interface IActivityMonitorClient
    {
        /// <summary>
        /// Called for each <see cref="IActivityMonitor.UnfilteredLog"/>.
        /// The <see cref="ActivityMonitorLogData.Exception"/> is always null since exceptions
        /// are carried by groups.
        /// </summary>
        /// <param name="data">Log data. Never null.</param>
        void OnUnfilteredLog( ActivityMonitorLogData data );

        /// <summary>
        /// Called for each <see cref="IActivityMonitor.UnfilteredOpenGroup"/>.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        void OnOpenGroup( IActivityLogGroup group );

        /// <summary>
        /// Called once the user conclusions are known at the group level but before 
        /// the group is actually closed: clients can update the conclusions for the group.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusions">
        /// Mutable conclusions associated to the closing group. 
        /// This can be null if no conclusions have been added yet. 
        /// It is up to the first client that wants to add a conclusion to instantiate a new List object to carry the conclusions.
        /// </param>
        void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions );

        /// <summary>
        /// Called when the group is actually closed.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusions">Texts that conclude the group. Never null but can be empty.</param>
        void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions );

        /// <summary>
        /// Called when a new <see cref="IActivityMonitor.Topic"/> is set.
        /// </summary>
        /// <param name="newTopic">The new topic string (never null but can be empty).</param>
        /// <param name="fileName">Source file name where <see cref="IActivityMonitor.SetTopic"/> has been called.</param>
        /// <param name="lineNumber">Source line number where IActivityMonitor.SetTopic has been called.</param>
        void OnTopicChanged( string newTopic, string fileName, int lineNumber );

        /// <summary>
        /// Called when <see cref="IActivityMonitor.AutoTags"/> changed.
        /// </summary>
        /// <param name="newTrait">The new tags (never null but can be empty).</param>
        void OnAutoTagsChanged( CKTrait newTrait );
    }

}

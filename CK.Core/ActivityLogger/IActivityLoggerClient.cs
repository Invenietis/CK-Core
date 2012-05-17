using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Listener for <see cref="IActivityLogger"/> registered in a <see cref="IMuxActivityLoggerClientRegistrar"/>.
    /// </summary>
    public interface IActivityLoggerClient : IActivityLoggerClientBase
    {
        /// <summary>
        /// Called when <see cref="IActivityLogger.Filter"/> is about to change.
        /// </summary>
        /// <param name="current">Current level filter.</param>
        /// <param name="newValue">The new level filter.</param>
        void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue );

        /// <summary>
        /// Called for each <see cref="IActivityLogger.UnfilteredLog"/>.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text (not null).</param>
        void OnUnfilteredLog( LogLevel level, string text );

        /// <summary>
        /// Called for each <see cref="IActivityLogger.OpenGroup"/>.
        /// </summary>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        void OnOpenGroup( IActivityLogGroup group );

        /// <summary>
        /// Called once the <paramref name="conclusion"/> is known at the group level but before the group
        /// is actually closed: clients can update or set the conclusion for the group.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        /// <returns>The new conclusion that should be associated to the group. Returning null has no effect on the current conclusion.</returns>
        string OnGroupClosing( IActivityLogGroup group, string conclusion );

        /// <summary>
        /// Called when the group is actually closed.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        void OnGroupClosed( IActivityLogGroup group, string conclusion );

    }

}

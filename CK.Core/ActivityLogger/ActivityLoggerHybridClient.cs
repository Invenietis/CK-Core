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
        /// Called once the <paramref name="conclusion"/> is known at the group level but before the group
        /// is actually closed: clients can update or set the conclusion for the group.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        /// <returns>The new conclusion that should be associated to the group. Returning null has no effect on the current conclusion.</returns>
        protected virtual string OnGroupClosing( IActivityLogGroup group, string conclusion )
        {
            return null;
        }

        /// <summary>
        /// Called when the group is actually closed.
        /// Does nothing by default.
        /// </summary>
        /// <param name="group">The closed group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        protected virtual void OnGroupClosed( IActivityLogGroup group, string conclusion )
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

        string IActivityLoggerClient.OnGroupClosing( IActivityLogGroup group, string conclusion )
        {
            return OnGroupClosing( group, conclusion );
        }

        void IActivityLoggerClient.OnGroupClosed( IActivityLogGroup group, string conclusion )
        {
            OnGroupClosed( group, conclusion );
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

        string IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            return OnGroupClosing( group, conclusion );
        }

        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            OnGroupClosed( group, conclusion );
        }

        #endregion

    }
}

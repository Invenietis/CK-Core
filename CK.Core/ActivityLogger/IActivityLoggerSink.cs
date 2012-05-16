using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Defines sink for <see cref="ActivityLoggerTap"/>.
    /// Inherits from this interface to implement your own logger (ie: XmlLogger).
    /// Each method described below provides an easy way to react to <see cref="IActivityLogger"/> calls.
    /// </summary>
    public interface IActivityLoggerSink
    {
        /// <summary>
        /// Called for the first text of a <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="level">The new current log level.</param>
        /// <param name="text">Text to start.</param>
        void OnEnterLevel( LogLevel level, string text );

        /// <summary>
        /// Called for text with the same <see cref="LogLevel"/> as the previous ones.
        /// </summary>
        /// <param name="level">The current log level.</param>
        /// <param name="text">Text to append.</param>
        void OnContinueOnSameLevel( LogLevel level, string text );

        /// <summary>
        /// Called when current log level changes.
        /// </summary>
        /// <param name="level">The previous log level.</param>
        void OnLeaveLevel( LogLevel level );

        /// <summary>
        /// Called whenever a group is opened.
        /// </summary>
        /// <param name="group">The newly opened group.</param>
        void OnGroupOpen( IActivityLogGroup group );

        /// <summary>
        /// Called once the <paramref name="conclusion"/> is known but before the group
        /// is actually closed.
        /// </summary>
        /// <param name="group">The closing group.</param>
        /// <param name="conclusion">Text that concludes the group. Never null but can be empty.</param>
        void OnGroupClose( IActivityLogGroup group, string conclusion );
    }
}

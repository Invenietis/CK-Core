using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Simple activity logger implementation for <see cref="DefaultActivityLogger"/>.
    /// Inherits from this interface to implement your own logger (ie: XmlLogger).
    /// Each method described below provides an easy way to react to <see cref="IActivityLogger"/> calls.
    /// </summary>
    public interface IDefaultActivityLoggerSink
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
        /// <param name="g">The newly opened group.</param>
        void OnGroupOpen( DefaultActivityLogger.Group g );

        /// <summary>
        /// Called whenever a group is closed.
        /// </summary>
        /// <param name="g">The closing group.</param>
        /// <param name="conclusion">Conclusion text.</param>
        void OnGroupClose( DefaultActivityLogger.Group g, string conclusion );
    }
}

using System;
using System.Collections.Generic;

namespace CK.Core
{
    /// <summary>
    /// Simple activity logger for end user communication. This is not the same as a classical logging framework: this 
    /// is dedicated to capture activities in order to display it to a end user.
    /// </summary>
    public interface IActivityLogger
    {        
        /// <summary>
        /// Gets or sets a filter based on the log level.
        /// This filter applies to the currently opened group.
        /// </summary>
        LogLevelFilter Filter { get; set; }

        /// <summary>
        /// Logs a text regardless of <see cref="Filter"/> level. 
        /// Each call to log is considered as a line: a paragraph (or line separator) is appended
        /// between each text if the <paramref name="level"/> is the same as the previous one.
        /// See remarks.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        /// <remarks>
        /// A null <paramref name="text"/> is not logged in itself but instead breaks the current <see cref="LogLevel"/>
        /// (as if a different <see cref="LogLevel"/> was used).
        /// </remarks>
        IActivityLogger UnfilteredLog( LogLevel level, string text );

        /// <summary>
        /// Opens a log level. <see cref="CloseGroup"/> must be called in order to
        /// close the group, or the returned object must be disposed.
        /// </summary>
        /// <param name="level">Log level. Since we are opening a group, the current <see cref="Filter"/> is ignored.</param>
        /// <param name="text">Text to log (the title of the group).</param>
        /// <param name="getConclusionText">Optional function that will be called on group closing.</param>
        /// <returns>A disposable object that can be used to close the group.</returns>
        /// <remarks>
        /// A group opening is not be filtered since any subordinated logs may occur.
        /// It is left to the implementation to handle (or not) filtering when <see cref="CloseGroup"/> is called.
        /// </remarks>
        IDisposable OpenGroup( LogLevel level, string text, Func<string> getConclusionText );

        /// <summary>
        /// Closes the current group level, appending an optional conclusion to the opening logged information.
        /// </summary>
        void CloseGroup( string conclusion );
    }

}

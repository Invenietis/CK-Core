
namespace CK.Core
{
    /// <summary>
    /// Five standard log levels in increasing order used by <see cref="IActivityLogger"/>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// A trace logging level (the most verbose level).
        /// </summary>
        Trace = 0,
        /// <summary>
        /// An info logging level.
        /// </summary>
        Info = 1,
        /// <summary>
        /// A warn logging level.
        /// </summary>
        Warn = 2,
        /// <summary>
        /// An error logging level: denotes an error for the current activity. 
        /// This error does not necessarily abort the activity.
        /// </summary>
        Error = 3,
        /// <summary>
        /// A fatal error logging level: denotes an error that breaks (aborts)
        /// the current activity. This kind of error may have important side effects
        /// on the system.
        /// </summary>
        Fatal = 4
    }

}

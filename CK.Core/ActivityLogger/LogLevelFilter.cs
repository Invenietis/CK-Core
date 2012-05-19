
namespace CK.Core
{
    /// <summary>
    /// Defines filters for <see cref="LogLevel"/>.
    /// </summary>
    public enum LogLevelFilter
    {
        /// <summary>
        /// No filter (same effect as <see cref="LogLevelFilter.Trace"/>.
        /// </summary>
        None = 0,
        /// <summary>
        /// Everything is logged (<see cref="LogLevel.Trace"/>).
        /// </summary>
        Trace = 1,
        /// <summary>
        /// Only <see cref="LogLevel.Info"/> and above is logged.
        /// </summary>
        Info = 2,
        /// <summary>
        /// Only <see cref="LogLevel.Warn"/> and above is logged.
        /// </summary>
        Warn = 3,
        /// <summary>
        /// Only <see cref="LogLevel.Error"/> and above is logged.
        /// </summary>
        Error = 4,
        /// <summary>
        /// Only <see cref="LogLevel.Fatal"/> is logged.
        /// </summary>
        Fatal = 5,
        /// <summary>
        /// Do not log anything.
        /// </summary>
        Off = 6
    }
}

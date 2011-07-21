
namespace CK.Core
{
    /// <summary>
    /// Defines filters for <see cref="LogLevel"/>.
    /// </summary>
    public enum LogLevelFilter
    {
        /// <summary>
        /// Everythnig is logged (<see cref="LogLevel.Trace"/>).
        /// </summary>
        Trace = 0,
        /// <summary>
        /// Only <see cref="LogLevel.Info"/> and above is logged.
        /// </summary>
        Info = 1,
        /// <summary>
        /// Only <see cref="LogLevel.Warn"/> and above is logged.
        /// </summary>
        Warn = 2,
        /// <summary>
        /// Only <see cref="LogLevel.Error"/> and above is logged.
        /// </summary>
        Error = 3,
        /// <summary>
        /// Only <see cref="LogLevel.Fatal"/> is logged.
        /// </summary>
        Fatal = 4,
        /// <summary>
        /// Do not log anything.
        /// </summary>
        Off = 5
    }
}

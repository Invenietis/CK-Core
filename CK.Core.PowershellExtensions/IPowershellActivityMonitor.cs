using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.PowershellExtensions
{
    /// <summary>
    /// Public interface for a powershell useable activity monitor. 
    /// It exposes an internal storage of logs and methods without having to import extension methods (that is an issue in powershell)
    /// </summary>
    public interface IPowershellActivityMonitor : IActivityMonitor
    {
        /// <summary>
        /// Empty the internal storage of logs.
        /// </summary>
        void Clear();

        /// <summary>
        /// Read all log lines in the internal storage.
        /// </summary>
        /// <returns>All logs available in the internal storage</returns>
        IEnumerable<string> ReadAllLines();

        /// <summary>
        /// Writes an error log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteError( string log );

        /// <summary>
        /// Writes a fatal log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteFatal( string log );

        /// <summary>
        /// Writes an info log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteInfo( string log );

        /// <summary>
        /// Writes a trace log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteTrace( string log );

        /// <summary>
        /// Writes a warn log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteWarn( string log );
    }
}
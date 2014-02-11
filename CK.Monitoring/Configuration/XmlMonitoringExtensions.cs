using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CK.Core;

namespace CK.Monitoring
{
    /// <summary>
    /// Helpers to read XML configurations.
    /// </summary>
    public static class XmlMonitoringExtensions
    {
        /// <summary>
        /// Reads a <see cref="LogFilter"/> that must exist.
        /// </summary>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        /// <returns>A LogFilter.</returns>
        static public LogFilter GetRequiredAttributeLogFilter( this XElement @this, string name )
        {
            return LogFilter.Parse( @this.AttributeRequired( name ).Value );
        }

        /// <summary>
        /// Reads a <see cref="LogFilter"/>.
        /// </summary>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="fallbackToUndefined">True to return <see cref="LogFilter.Undefined"/> instead of null when not found.</param>
        /// <returns>A nullable LogFilter.</returns>
        static public LogFilter? GetAttributeLogFilter( this XElement @this, string name, bool fallbackToUndefined )
        {
            XAttribute a = @this.Attribute( name );
            return a != null ? LogFilter.Parse( a.Value ) : (fallbackToUndefined ? LogFilter.Undefined : (LogFilter?)null);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.RouteConfig;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    public class BinaryFileConfiguration : HandlerConfiguration
    {
        public BinaryFileConfiguration( string name )
            : base( name )
        {
        }

        /// <summary>
        /// Gets or sets the path of the file. When not rooted (see <see cref="System.IO.Path.IsPathRooted"/>),
        /// it is a sub path in <see cref="SystemActivityMonitor.RootLogPath"/>.
        /// It defaults to null: it must be specified.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the minimal filter for this file handler.
        /// Defaults to <see cref="LogFilter.Undefined"/>: unless specified, the handler will log 
        /// whatever it receives thanks to the other minimal filters in the system.
        /// </summary>
        public LogFilter MinimalFilter { get; set; }

        protected internal override void Initialize( XElement xml )
        {
            Path = xml.AttributeRequired( "Path" ).Value;
            MinimalFilter = xml.GetAttributeLogFilter( "MinimalFilter" );
        }
    }
}

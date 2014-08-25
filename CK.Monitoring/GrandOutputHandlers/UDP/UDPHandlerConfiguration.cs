using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers.UDP
{
    /// <summary>
    /// Configuration object for <see cref="UDPHandler"/>.
    /// </summary>
    [HandlerType( typeof( UDPHandler ) )]
    public class UDPHandlerConfiguration : HandlerConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="UDPHandlerConfiguration"/>.
        /// </summary>
        /// <param name="name">The configuration name</param>
        public UDPHandlerConfiguration( string name )
            : base( name )
        {
        }

        public int Port { get; set; }

        protected override void Initialize( Core.IActivityMonitor m, System.Xml.Linq.XElement xml )
        {
            Port = xml.GetAttributeInt( "Port", Port );
        }
    }

}

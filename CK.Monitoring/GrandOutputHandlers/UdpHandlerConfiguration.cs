using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Configuration object for <see cref="UDPHandler"/>.
    /// </summary>
    [HandlerType( typeof( UdpHandler ) )]
    public class UdpHandlerConfiguration : HandlerConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="UdpHandlerConfiguration"/>.
        /// </summary>
        /// <param name="name">The configuration name</param>
        public UdpHandlerConfiguration( string name )
            : base( name )
        {
            Port = 3712;
            MaxPacketSize = 1280;
        }

        /// <summary>
        /// Gets or sets the Udp Port. Defaults to 3712.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the max Udp Packet Size. Defaults to 1280
        /// </summary>
        public int MaxPacketSize { get; set; }

        protected override void Initialize( Core.IActivityMonitor m, System.Xml.Linq.XElement xml )
        {
            Port = xml.GetAttributeInt( "Port", Port );
            MaxPacketSize = xml.GetAttributeInt( "MaxPacketSize", MaxPacketSize );
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    [HandlerType( typeof( FakeHandler ) )]
    class FakeHandlerConfiguration : HandlerConfiguration
    {
        public FakeHandlerConfiguration( string name )
            : base( name )
        {
            ExtraLoad = -1;
        }

        protected override void Initialize( IActivityMonitor m, XElement xml )
        {
            int s = xml.GetAttributeInt( "ExtraLoad", -1 );
            ExtraLoad = s < 0 ? -1 : s;
        }

        public int ExtraLoad { get; set; }
    }
}

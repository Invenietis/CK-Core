using System.Xml.Linq;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    public abstract class HandlerConfiguration : ActionConfiguration
    {
        protected HandlerConfiguration( string name )
            : base( name )
        {
        }

        internal protected abstract void Initialize( XElement xml );
    }
}

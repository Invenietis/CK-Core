using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig
{
    public class ActionSequenceConfiguration : Impl.ActionCompositeConfiguration
    {
        public ActionSequenceConfiguration( string name )
            : base( name, false )
        {
        }

        public ActionSequenceConfiguration AddAction( ActionConfiguration a )
        {
            base.Add( a );
            return this;
        }
    }
}

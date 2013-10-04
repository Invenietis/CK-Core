using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.RouteConfig
{
    public class ActionParallelConfiguration : Impl.ActionCompositeConfiguration
    {
        public ActionParallelConfiguration( string name )
            : base( name, true )
        {
        }

        public ActionParallelConfiguration AddAction( ActionConfiguration a )
        {
            base.Add( a );
            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig;

namespace CK.Monitoring
{
    [AttributeUsage( AttributeTargets.Class )]
    class ConfiguredSinkTypeAttribute : Attribute
    {
        public ConfiguredSinkTypeAttribute( Type sinkType )
        {
            SinkType = sinkType;
        }

        public readonly Type SinkType;
    }

}

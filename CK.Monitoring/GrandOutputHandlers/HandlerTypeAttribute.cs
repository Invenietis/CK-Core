using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    [AttributeUsage( AttributeTargets.Class )]
    public class HandlerTypeAttribute : Attribute
    {
        public HandlerTypeAttribute( Type handlerType )
        {
            HandlerType = handlerType;
        }

        public readonly Type HandlerType;
    }

}

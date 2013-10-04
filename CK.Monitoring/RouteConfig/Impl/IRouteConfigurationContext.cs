using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    public interface IRouteConfigurationContext
    {
        IActivityMonitor Monitor { get; }

        IProtoRoute ProtoRoute { get; }

        IEnumerable<ActionConfigurationResolved> CurrentActions { get; }

        ActionConfigurationResolved FindExisting( string name );

        bool RemoveAction( string name );

        bool AddDeclaredAction( string name, string declaredName, bool fromDeclaration );
    }

}

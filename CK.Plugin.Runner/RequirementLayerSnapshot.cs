using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// 
    /// </summary>
    public class RequirementLayerSnapshot
    {
        public class PluginRequirementIdentifier
        {
            internal RunningRequirement Requirement { get; set; }
            public Guid PluginId { get; internal set; }
        }

        public class ServiceRequirementIdentifier
        {
            internal RunningRequirement Requirement { get; set; }
            public string AssemblyQualifiedName { get; internal set; }
        }

        public string LayerName { get; private set; }

        public IReadOnlyCollection<PluginRequirementIdentifier> PluginRequirements { get; private set; }

        public IReadOnlyCollection<ServiceRequirementIdentifier> ServiceRequirements { get; private set; }

        internal RequirementLayerSnapshot( RequirementLayer l )
        {
            LayerName = l.LayerName;

            var plugins = l.PluginRequirements.Select( ( r, idx ) => new PluginRequirementIdentifier() { PluginId = r.PluginId, Requirement = r.Requirement } ).ToArray();
            PluginRequirements = new ReadOnlyCollectionOnICollection<PluginRequirementIdentifier>( plugins );

            var services = l.ServiceRequirements.Select( ( r, idx ) => new ServiceRequirementIdentifier() { AssemblyQualifiedName = r.AssemblyQualifiedName, Requirement = r.Requirement } ).ToArray();
            ServiceRequirements = new ReadOnlyCollectionOnICollection<ServiceRequirementIdentifier>( services );
        }
    }
}

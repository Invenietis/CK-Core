using System;
using System.Xml;
using System.Diagnostics;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Represents the state that a <see cref="RequirementLayer"/> (Context, Keboard, Layout) requires for a specific plugin 
    /// </summary>
    public class PluginRequirement
    {
        PluginRequirementCollection _holder;
        RunningRequirement _req;
        
        internal PluginRequirement NextElement;

        /// <summary>
        /// Unique identifier of the plugin.
        /// </summary>
        public Guid PluginId { get; private set; }

        /// <summary>
        /// Gets the <see cref="RunningRequirement"/> corresponding to this PluginRequirement instance.
        /// </summary>
        public RunningRequirement Requirement
        {
            get { return _req; }
            internal set { _req = value; }
        }

        internal PluginRequirementCollection Holder
        {
            get { return _holder; }
            set { _holder = value; }
        }

        internal PluginRequirement( PluginRequirementCollection holder )
        {
            Debug.Assert( holder != null );
            _holder = holder;
        }

        internal PluginRequirement( PluginRequirementCollection holder, Guid pluginID, RunningRequirement requirement )
            : this( holder )
        {
            Debug.Assert( pluginID != null );
            PluginId = pluginID;
            _req = requirement;
        }
    }
}

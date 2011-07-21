using System;
using System.Diagnostics;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Represents the state that a <see cref="RequirementLayer"/> (Context, Keboard, Layout) requires for a specific service.
    /// </summary>
    public class ServiceRequirement
    {
        ServiceRequirementCollection _holder;
        RunningRequirement _req;

        internal ServiceRequirement NextElement;

        /// <summary>
        /// Full name of the required service.
        /// </summary>
        public string AssemblyQualifiedName { get; private set; }

        /// <summary>
        /// Gets the <see cref="RunningRequirement"/> corresponding to this ServiceRequirement instance.
        /// </summary>
        public RunningRequirement Requirement
        {
            get { return _req; }
            internal set { _req = value; }
        }

        internal ServiceRequirementCollection Holder
        {
            get { return _holder; }
            set { _holder = value; }
        }

        internal ServiceRequirement( ServiceRequirementCollection holder )
        {
            Debug.Assert( holder != null );
            _holder = holder;
        }

        internal ServiceRequirement( ServiceRequirementCollection holder, string serviceAssemblyQualifiedName, RunningRequirement requirement )
            : this( holder )
        {
            Debug.Assert( serviceAssemblyQualifiedName != null );
            AssemblyQualifiedName = serviceAssemblyQualifiedName;
            _req = requirement;
        }
    }
}

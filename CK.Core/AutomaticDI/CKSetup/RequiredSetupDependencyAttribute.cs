using System;


namespace CK.Setup
{

    /// <summary>
    /// Assembly attribute that declares a setup dependency.
    /// This is typically used by "Model" (see <see cref="IsModelAttribute"/>) to define one or more associated engines.
    /// <para>
    /// This assembly attribute, just like <see cref="ExcludeFromSetupAttribute"/>, <see cref="IsModelAttribute"/> and <see cref="IsSetupDependencyAttribute"/>)
    /// is defined here so that dependent assemblies can easily apply them on their own assemblies but they are used by CKSetup for which the full
    /// name is enough (duck typing): any CK.Setup.RequiredSetupDependencyAttribute attribute, even locally defined will do the job.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    public class RequiredSetupDependencyAttribute : Attribute
    {
        /// <summary>
        /// Default <see cref="MinDependencyVersion"/> value is to consider that the dependency version
        /// is synchronized with the version of this component:  "UseThisVersion".
        /// </summary>
        public const string MinDependencyVersionIsThisVersion = "UseThisVersion";

        /// <summary>
        /// Initializes a new required setup dependency attribute.
        /// </summary>
        /// <param name="assemblyName">Name of the setup dependency assembly.</param>
        /// <param name="minDependencyVersion">
        /// Optional version. By default, the dependency must have at least the version of this component.
        /// Setting it to null removes all version constraints and setting it to a specific version
        /// states that subsequent version of the dependency should continue to be able to handle this component.
        /// </param>
        public RequiredSetupDependencyAttribute( string assemblyName, string? minDependencyVersion = MinDependencyVersionIsThisVersion )
        {
            AssemblyName = assemblyName;
            MinDependencyVersion = minDependencyVersion;
        }

        /// <summary>
        /// Gets the name of the setup dependency assembly.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets an optional version for the setup dependency assembly.
        /// Optional version. By default, the dependency must have at least the version of this component
        /// (via the special string <see cref="MinDependencyVersionIsThisVersion"/> = "UseThisVersion").
        /// Setting it to null removes all version constraints and setting it to a specific version
        /// states that subsequent version of the setup dependency should continue to be able to handle this component.
        /// </summary>
        public string? MinDependencyVersion { get; }
    }
}

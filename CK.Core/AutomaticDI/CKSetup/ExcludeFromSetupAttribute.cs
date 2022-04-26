using System;


namespace CK.Setup
{
    /// <summary>
    /// Marks an assembly that even if it depends on Models should not participate in Setup.
    /// <para>
    /// This assembly attribute, just like <see cref="IsModelAttribute"/>, <see cref="IsSetupDependencyAttribute"/> and <see cref="RequiredSetupDependencyAttribute"/>)
    /// is defined here so that dependent assemblies can easily apply them on their own assemblies but they are used by CKSetup for which the full
    /// name is enough (duck typing): any CK.Setup.ExcludeFromSetupAttribute attribute, even locally defined will do the job.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
    public class ExcludeFromSetupAttribute : Attribute
    {
    }
}

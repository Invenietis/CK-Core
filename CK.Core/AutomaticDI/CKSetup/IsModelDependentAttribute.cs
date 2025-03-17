using System;

namespace CK.Setup;

/// <summary>
/// Marks an assembly as being a "ModelDependent" even if it doesn't depend on a "Model" assembly.
/// See <see cref="IsModelAttribute"/>.
/// <para>
/// This assembly attribute, just like <see cref="ExcludeFromSetupAttribute"/>, <see cref="IsSetupDependencyAttribute"/>, <see cref="IsModelAttribute"/>
/// and <see cref="RequiredSetupDependencyAttribute"/>) is defined here so that dependent assemblies can easily apply them on their own assemblies but they are
/// used by CKSetup for which the full name is enough (duck typing): any CK.Setup.IsModelAttribute attribute, even locally defined will do the job.
/// </para>
/// </summary>
[Obsolete( "Replaced by IsPFeature.", error: true )]
[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
public class IsModelDependentAttribute : Attribute
{
}

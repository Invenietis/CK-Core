using System;


namespace CK.Setup;

/// <summary>
/// Marks an assembly as being a setup dependency: this assembly is an "engine" that CKSetup will use to
/// process an application-wide post compilation step (typically global code generation).
/// A setup dependency can have other <see cref="RequiredSetupDependencyAttribute"/> just like Models.
/// <para>
/// This assembly attribute, just like <see cref="ExcludeFromSetupAttribute"/>, <see cref="IsModelAttribute"/>, <see cref="IsModelDependentAttribute"/>
/// and <see cref="RequiredSetupDependencyAttribute"/>) is defined here so that dependent assemblies can easily apply them on their own assemblies but they are used by CKSetup for which the full
/// name is enough (duck typing): any CK.Setup.IsSetupDependencyAttribute attribute, even locally defined will do the job.
/// </para>
/// </summary>
[Obsolete( "Replaced by IsEngine.", error: true )]
[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
public class IsSetupDependencyAttribute : Attribute
{
}

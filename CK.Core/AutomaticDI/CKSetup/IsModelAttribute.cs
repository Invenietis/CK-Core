using System;


namespace CK.Setup;

/// <summary>
/// Marks an assembly as being a Model. A "Model" typically defines abstract constructs and attributes that
/// are referenced and used by "ModelDependent" assemblies.
/// <para>
/// Models are usually also marked with a <see cref="RequiredSetupDependencyAttribute"/> that identifies an associated "engine"
/// component that actually handles the Model implementation. 
/// </para>
///<para>
/// The "ModelDependent" assemblies are the ones that must be processed by CKSetup post compilation
/// and unification mechanism (by using the appropriate engine).
///</para>
/// <para>
/// This assembly attribute, just like <see cref="ExcludeFromSetupAttribute"/>, <see cref="IsSetupDependencyAttribute"/>, <see cref="IsModelDependentAttribute"/>
/// and <see cref="RequiredSetupDependencyAttribute"/>) is defined here so that dependent assemblies can easily apply them on their own assemblies but they are
/// used by CKSetup for which the full name is enough (duck typing): any CK.Setup.IsModelAttribute attribute, even locally defined will do the job.
/// </para>
/// </summary>
[Obsolete( "Replaced by IsPFeatureDefiner.", error: true )]
[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
public class IsModelAttribute : Attribute
{
}

using System;

namespace CK.Core;

/// <summary>
/// Attribute that excludes a type from the Automatic DI discovering.
/// </summary>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
public class ExcludeCKTypeAttribute : Attribute
{
}

using System;

namespace CK.Core
{
    /// <summary>
    /// Attribute that excludes a type from the Automatic DI discovering.
    /// </summary>
    /// <para>
    /// This attribute, just like <see cref="IRealObject"/>, <see cref="IAutoService"/>, <see cref="IScopedAutoService"/>
    /// and <see cref="ISingletonAutoService"/> can be created anywhere: as long as the name is "ExcludeCKTypeAttribute" 
    /// (regardless of the namespace), it will be honored.
    /// </para>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
    public class ExcludeCKTypeAttribute : Attribute
    {
    }

}

using System;

namespace CK.Core
{
    /// <summary>
    /// Marks a class or an interface with a singleton lifetime, but don't handle automatic DI for it (unlike
    /// the <see cref="ISingletonAutoService"/> interface): the service must be manually registered in the DI
    /// container.
    /// <para>
    /// It is not required to be this exact type: any attribute named "SingletonServiceAttribute" defined in any
    /// namespace will be considered as a valid marker.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false )]
    public class SingletonServiceAttribute : Attribute
    {
    }

}

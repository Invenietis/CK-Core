using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core;

/// <summary>
/// Shortcuts any <see cref="Object.GetHashCode()"/> and <see cref="Object.Equals(object)"/> overrides
/// by relying only on strict object reference equality.
/// </summary>
/// <typeparam name="T">The type of the object.</typeparam>
public class PureObjectRefEqualityComparer<T> : IEqualityComparer<T> where T : class
{
    /// <summary>
    /// Gets the default instance.
    /// </summary>
    public static readonly IEqualityComparer<T> Default = new PureObjectRefEqualityComparer<T>();

    /// <summary>
    /// Simple relay to <see cref="Object.ReferenceEquals(object, object)"/>.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>true if objA is the same instance as objB or if both are null; otherwise, false.</returns>
    public bool Equals( T? x, T? y ) => ReferenceEquals( x, y );

    /// <summary>
    /// Simple relay to <see cref="RuntimeHelpers.GetHashCode(object)"/>.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>The object reference hash code.</returns>
    public int GetHashCode( T obj ) => RuntimeHelpers.GetHashCode( obj );
}

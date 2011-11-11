using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CK.Core
{
    /// <summary>
    /// Definition of an <see cref="IObservableReadOnlyCollection{T}"/> that is <see cref="IReadOnlyList{T}"/> (the index of the elements make sense).
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface IObservableReadOnlyList<out T> : IObservableReadOnlyCollection<T>, IReadOnlyList<T>
    {
    }

}

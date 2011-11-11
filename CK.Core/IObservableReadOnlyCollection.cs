using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CK.Core
{
    /// <summary>
    /// Definition of a <see cref="IReadOnlyCollection{T}"/> that is observable through <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
    /// It has no properties nor methods by itself: it is only here to federate its 3 base interfaces.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public interface IObservableReadOnlyCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }

}

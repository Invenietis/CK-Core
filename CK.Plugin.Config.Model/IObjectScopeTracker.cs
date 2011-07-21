using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections;

namespace CK.Plugin.Config
{

    /// <summary>
    /// Defines the return of the <see cref="IObjectScopeTracker.Updated"/> method.
    /// </summary>
    struct ObjectScopeTrackerUpdateResult
    {
        /// <summary>
        /// An optional scope tracker for the new value.
        /// </summary>
        public IObjectScopeTracker ScopeForNewValue;

        /// <summary>
        /// An optional set of potential key objects that must be <see cref="IConfigContainer.Destroy(object)">destroyed</see>.
        /// </summary>
        public IEnumerable KeyObjectsToDestroy;
    }


    /// <summary>
    /// Defines a kind of life time manager for configuration objects.
    /// </summary>
    interface IObjectScopeTracker
    {
        /// <summary>
        /// Called when a new entry appear for an object.
        /// </summary>
        /// <param name="c">The container that holds the properties.</param>
        /// <param name="o">Object that carries the properties (it is already associated to this <see cref="IObjectScopeTracker"/>).</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>
        /// An optional <see cref="IObjectScopeTracker"/> (can be null or typically this) that will become
        /// the tracker associated to the <paramref name="value"/>.
        /// </returns>
        IObjectScopeTracker Added( IConfigContainer c, object o, INamedVersionedUniqueId p, string k, object value );


        /// <summary>
        /// Called when an entry is removed for an object.
        /// </summary>
        /// <param name="c">The container that holds the properties.</param>
        /// <param name="o">Object that carries the properties (it is already associated to this <see cref="IObjectScopeTracker"/>).</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="value">Current value that must be removed.</param>
        /// <returns>
        /// An optional <see cref="IEnumerable"/> of potential key objects that must be <see cref="IConfigContainer.Destroy(object)">destroyed</see>.
        /// </returns>
        IEnumerable Removed( IConfigContainer c, object o, INamedVersionedUniqueId p, string k, object value );

        /// <summary>
        /// Combined added and removed operations in one.
        /// </summary>
        /// <param name="c">The container that holds the properties.</param>
        /// <param name="o">Object that carries the properties (it is already associated to this <see cref="IObjectScopeTracker"/>).</param>
        /// <param name="p">Plugin identifier.</param>
        /// <param name="k">Key for the data.</param>
        /// <param name="oldValue">Previous value.</param>
        /// <param name="newValue">New value.</param>
        /// <returns>Structure that combines the return of <see cref="Removed"/> and <see cref="Added"/> operations (resp. for <paramref name="oldValue"/> and <paramref name="newValue"/>).</returns>
        ObjectScopeTrackerUpdateResult Updated( IConfigContainer c, object o, INamedVersionedUniqueId p, string k, object oldValue, object newValue );
    }
}

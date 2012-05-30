#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config.Model\IObjectScopeTracker.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using CK.Core;
//using System.Collections;

//namespace CK.Plugin.Config
//{

//    /// <summary>
//    /// Defines the return of the <see cref="IObjectScopeTracker.Updated"/> method.
//    /// </summary>
//    struct ObjectScopeTrackerUpdateResult
//    {
//        /// <summary>
//        /// An optional scope tracker for the new value.
//        /// </summary>
//        public IObjectScopeTracker ScopeForNewValue;

//        /// <summary>
//        /// An optional set of potential key objects that must be <see cref="IConfigContainer.Destroy(object)">destroyed</see>.
//        /// </summary>
//        public IEnumerable KeyObjectsToDestroy;
//    }


//    /// <summary>
//    /// Defines a kind of life time manager for configuration objects.
//    /// </summary>
//    interface IObjectScopeTracker
//    {
//        /// <summary>
//        /// Called when a new entry appear for an object.
//        /// </summary>
//        /// <param name="c">The container that holds the properties.</param>
//        /// <param name="o">Object that carries the properties (it is already associated to this <see cref="IObjectScopeTracker"/>).</param>
//        /// <param name="p">Plugin identifier.</param>
//        /// <param name="k">Key for the data.</param>
//        /// <param name="value">Value to set.</param>
//        /// <returns>
//        /// An optional <see cref="IObjectScopeTracker"/> (can be null or typically this) that will become
//        /// the tracker associated to the <paramref name="value"/>.
//        /// </returns>
//        IObjectScopeTracker Added( IConfigContainer c, object o, INamedVersionedUniqueId p, string k, object value );


//        /// <summary>
//        /// Called when an entry is removed for an object.
//        /// </summary>
//        /// <param name="c">The container that holds the properties.</param>
//        /// <param name="o">Object that carries the properties (it is already associated to this <see cref="IObjectScopeTracker"/>).</param>
//        /// <param name="p">Plugin identifier.</param>
//        /// <param name="k">Key for the data.</param>
//        /// <param name="value">Current value that must be removed.</param>
//        /// <returns>
//        /// An optional <see cref="IEnumerable"/> of potential key objects that must be <see cref="IConfigContainer.Destroy(object)">destroyed</see>.
//        /// </returns>
//        IEnumerable Removed( IConfigContainer c, object o, INamedVersionedUniqueId p, string k, object value );

//        /// <summary>
//        /// Combined added and removed operations in one.
//        /// </summary>
//        /// <param name="c">The container that holds the properties.</param>
//        /// <param name="o">Object that carries the properties (it is already associated to this <see cref="IObjectScopeTracker"/>).</param>
//        /// <param name="p">Plugin identifier.</param>
//        /// <param name="k">Key for the data.</param>
//        /// <param name="oldValue">Previous value.</param>
//        /// <param name="newValue">New value.</param>
//        /// <returns>Structure that combines the return of <see cref="Removed"/> and <see cref="Added"/> operations (resp. for <paramref name="oldValue"/> and <paramref name="newValue"/>).</returns>
//        ObjectScopeTrackerUpdateResult Updated( IConfigContainer c, object o, INamedVersionedUniqueId p, string k, object oldValue, object newValue );
//    }
//}

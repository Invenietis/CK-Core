#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActionSequence\IActionSequence.cs) is part of CiviKey. 
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

using System;

namespace CK.Storage
{
    /// <summary>
    /// Provides a way to stack <see cref="Action"/>s and to defer their execution. 
    /// This interface only defines append behavior.
    /// </summary>
    public interface IActionSequence
    {
        /// <summary>
        /// Gets a boolean that states whether this sequence is read-only. 
        /// When a sequence is read-only, any attempt to append an action is an error (an exception must be thrown).
        /// Defaults to false.
        /// </summary>
        bool ReadOnly { get; }

        /// <summary>
        /// Appends an event raising.
        /// </summary>
        /// <param name="e">The <see cref="EventHandler"/> delegate.</param>
        /// <param name="source">Source of the event.</param>
        /// <param name="eventArgs">Event argument.</param>
        void Append( EventHandler e, object source, EventArgs eventArgs );

        /// <summary>
        /// Appends an event raising.
        /// </summary>
        /// <typeparam name="T">Must be a class that inherits from <see cref="EventArgs"/>.</typeparam>
        /// <param name="e">The <see cref="EventHandler{T}"/> delegate.</param>
        /// <param name="source">Source of the event.</param>
        /// <param name="eventArgs">Event argument.</param>
        void Append<T>( EventHandler<T> e, object source, T eventArgs ) where T : EventArgs;

        /// <summary>
        /// Appends an action (without parameters).
        /// </summary>
        /// <param name="action">No parameter <see cref="Action"/> delegate.</param>
        void Append( Action action );

        /// <summary>
        /// Appends an action (with one parameter).
        /// </summary>
        /// <param name="action">One parameter <see cref="Action"/> delegate.</param>
        /// <param name="parameter">Action parameter.</param>
        void Append<T>( Action<T> action, T parameter );

        /// <summary>
        /// Appends an action (with two parameters).
        /// </summary>
        /// <param name="action">Two parameters <see cref="Action"/> delegate.</param>
        /// <param name="p1">First action parameter.</param>
        /// <param name="p2">Second action parameter.</param>
        void Append<T1, T2>( Action<T1, T2> action, T1 p1, T2 p2 );

        /// <summary>
        /// Appends an action (with three parameters).
        /// </summary>
        /// <param name="action">Three parameters <see cref="Action"/> delegate.</param>
        /// <param name="p1">First action parameter.</param>
        /// <param name="p2">Second action parameter.</param>
        /// <param name="p3">Third action parameter.</param>
        void Append<T1, T2, T3>( Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3 );

        /// <summary>
        /// Removes all recorded actions.
        /// </summary>
        void Clear();

    }
}

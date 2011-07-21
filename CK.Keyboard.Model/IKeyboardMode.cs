#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\IKeyboardMode.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using CK.Core;
using System;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines the modes of a keyboard. A mode is an immutable object, associated to a unique string, that can be atomic ("Alt", "Home", "Ctrl") or 
    /// combined ("Alt+Ctrl", "Alt+Ctrl+Home"). The only way to obtain a <see cref="IKeyboardMode"/> is to call <see cref="IKeyboardContextMode.ObtainMode"/> (from 
    /// a string) or to use one of the available combination methods (<see cref="Add"/>, <see cref="Remove"/>, <see cref="Toggle"/> or <see cref="Intersect"/> ).
    /// </summary>
    public interface IKeyboardMode : IComparable<IKeyboardMode>
    {
        /// <summary>
        /// Gets the <see cref="IKeyboardContextMode"/>. 
        /// </summary>
        IKeyboardContextMode Context { get; }

        /// <summary>
        /// Gets the atomic modes that this mode contains.
        /// </summary>
        IReadOnlyList<IKeyboardMode> AtomicModes { get; }

        /// <summary>
        /// Gets a boolean indicating whether this mode is the empty mode (<see cref="AtomicModes"/> is empty
        /// and <see cref="Fallbacks"/> contains only itself).
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets a boolean indicating whether this mode contains zero 
        /// (the empty mode is considered as an atomic mode) or only one atomic mode.
        /// </summary>
        /// <remarks>
        /// For atomic modes (an the empty mode itself), <see cref="Fallbacks"/> contains only the <see cref="IKeyboardContextMode.EmptyMode"/>.
        /// </remarks>
        bool IsAtomic { get; }

        /// <summary>
        /// Checks if the each and every atomic modes of <paramref name="mode" /> exists in this mode.
        /// </summary>
        /// <param name="mode">The mode(s) to find.</param>
        /// <returns>True if all the specified modes appear in this mode.</returns>
        /// <remarks>
        /// Note that <see cref="IContext.EmptyMode"/> is contained (in the sense of this ContainsAll method) by definition in any mode 
        /// (including itself): this is the opposite of the <see cref="ContainsOne"/> method.
        /// </remarks>
        bool ContainsAll( IKeyboardMode mode );

        /// <summary>
        /// Checks if one of the atomic modes of <paramref name="mode" /> exists in this mode.
        /// </summary>
        /// <param name="mode">The mode(s) to find.</param>
        /// <returns>Returns true if one of the specified modes appears in this mode.</returns>
        /// <remarks>
        /// When true, this ensures that <see cref="Intersect"/>( <paramref name="mode"/> ) != <see cref="IKeyboardContextMode.EmptyMode"/>. 
        /// The empty mode is not contained (in the sense of this ContainsOne method) in any mode (including itself). This is the opposite
        /// as the <see cref="ContainsAll"/> method.
        /// </remarks>
        bool ContainsOne( IKeyboardMode mode );

        /// <summary>
        /// Removes the <see cref="IKeyboardMode"/> specified by the parameter. 
        /// </summary>
        /// <param name="mode">Mode(s) to remove.</param>
        /// <returns>The resulting mode.</returns>
        IKeyboardMode Remove( IKeyboardMode mode );

        /// <summary>
        /// Adds the <see cref="IKeyboardMode"/> specified by the parameter. 
        /// </summary>
        /// <param name="mode">Mode(s) to add.</param>
        /// <returns>The resulting mode.</returns>
        IKeyboardMode Add( IKeyboardMode mode );

        /// <summary>
        /// Removes (resp. adds) the atomic modes of <paramref name="mode" /> depending 
        /// on whether they exist (resp. do not exist) in this mode. 
        /// </summary>
        /// <param name="mode">Mode(s) to toggle.</param>
        /// <returns>The resulting mode.</returns>
        IKeyboardMode Toggle( IKeyboardMode mode );

        /// <summary>
        /// Removes the atomic modes from this mode that do not appear in <paramref name="mode"/>.
        /// </summary>
        /// <param name="mode">Mode(s) that must be kept.</param>
        /// <returns>The resulting mode.</returns>
        IKeyboardMode Intersect( IKeyboardMode mode );

        /// <summary>
        /// Gets the list of fallbacks to consider for this mode ordered from best to worst.
        /// The <see cref="IKeyboardContextMode.EmptyMode"/> always ends this list.
        /// </summary>
        /// <remarks>
        /// For atomic modes (an the empty mode itself), <see cref="Fallbacks"/> contains only the <see cref="IKeyboardContextMode.EmptyMode"/>.
        /// </remarks>
        IReadOnlyList<IKeyboardMode> Fallbacks { get; }

    }
}

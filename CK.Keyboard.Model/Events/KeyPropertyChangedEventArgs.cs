#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\KeyPropertyChangedEventArgs.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines a key property changed event argument: <see cref="PropertyName"/> holds the name of the 
    /// property that changed.
    /// </summary>
    public class KeyPropertyChangedEventArgs : KeyEventArgs
    {
        /// <summary>
        /// Gets the object that holds the property.
        /// It can be this <see cref="KeyEventArgs.Key">key</see>, its <see cref="ILayoutKey"/> or one 
        /// of its <see cref="IKeyMode"/> or <see cref="ILayoutKeyMode"/>.
        /// </summary>
        public IKeyPropertyHolder PropertyHolder { get; private set; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="KeyPropertyChangedEventArgs"/>. For specialized events, the <see cref="PropertyHolder"/> is typically 
        /// masked by a more precise signature.
        /// </summary>
        public KeyPropertyChangedEventArgs( IKeyPropertyHolder propertyHolder, string propertyName )
            : base( propertyHolder.Key )
        {
            PropertyName = propertyName;
            PropertyHolder = propertyHolder;
        }
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\EventArgs\ContextEventArgs.cs) is part of CiviKey. 
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

namespace CK.Context
{
    /// <summary>
    /// Defines a context event: it simply offers an access to the context.
    /// </summary>
    public class ContextEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IContext"/> to which this event is related.
        /// </summary>
        public IContext Context { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="ContextEventArgs"/> boud to a <see cref="Context"/>.
        /// </summary>
        /// <param name="ctx"></param>
        public ContextEventArgs( IContext ctx )
        {
            if( ctx == null ) throw new ArgumentNullException( "ctx" );
            Context = ctx;
        }
    }
}

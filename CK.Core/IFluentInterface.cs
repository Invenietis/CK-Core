#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\IFluentInterface.cs) is part of CiviKey. 
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
using System.ComponentModel;

namespace CK.Core
{
    /// <summary>
    /// Helper interface used to hide the base <see cref="Object"/> members from the fluent API to make 
    /// for much cleaner Visual Studio intellisense experience. (Excellent idea borrowed from EntLib.)
    /// Use it on an interface: it must be the first interface (if more than one interface are supported) 
    /// to hide those useless methods. 
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public interface IFluentInterface
    {
        /// <summary/>
        [EditorBrowsable( EditorBrowsableState.Never )]
        Type GetType();

        /// <summary/>
        [EditorBrowsable( EditorBrowsableState.Never )]
        int GetHashCode();

        /// <summary/>
        [EditorBrowsable( EditorBrowsableState.Never )]
        string ToString();

        /// <summary/>
        [EditorBrowsable( EditorBrowsableState.Never )]
        bool Equals( object obj );
    }
}
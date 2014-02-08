#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ICKObservableReadOnlyList.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CK.Core
{
    /// <summary>
    /// Definition of a <see cref="IObservableReadOnlyList{T}"/> that extends <see cref="ICKReadOnlyList{T}"/> (for the 
    /// contravariant <see cref="ICKReadOnlyCollection{T}.Contains"/> and <see cref="ICKReadOnlyList{T}.IndexOf"/> methods).
    /// It has no properties nor methods by itself: it is only here to federate its 2 real base interfaces (the <see cref="ICKObservableReadOnlyCollection{T}"/> 
    /// enforces the fact that any list is actually a collection).
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface ICKObservableReadOnlyList<out T> : IObservableReadOnlyList<T>, ICKReadOnlyList<T>, ICKObservableReadOnlyCollection<T>
    {
    }

}

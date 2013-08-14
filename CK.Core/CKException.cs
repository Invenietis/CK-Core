#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\CKException.cs) is part of CiviKey. 
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
using System.Runtime.Serialization;

namespace CK.Core
{
    /// <summary>
    /// Basic <see cref="Exception"/> that eases message formatting thanks to its contructors.
    /// </summary>
    [Serializable]
    public class CKException : Exception
    {
        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CKException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="message">Simple message.</param>
        public CKException( string message )
            : base( message )
        {
        }
        
        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="messageFormat">Format string with optional placeholders.</param>
        /// <param name="args">Varying number of arguments to format.</param>
        public CKException( string messageFormat, params object[] args )
            : base( String.Format( messageFormat, args ) )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="message">Simple message.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public CKException( string message, Exception innerException )
            : base( message, innerException )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="innerException">Exception that caused this one.</param>
        /// <param name="messageFormat">Format string with optional placeholders.</param>
        /// <param name="args">Varying number of arguments to format.</param>
        public CKException( Exception innerException, string messageFormat, params object[] args )
            : base( String.Format( messageFormat, args ), innerException )
        {
        }

    }
}

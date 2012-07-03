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
    /// Basic <see cref="Exception"/>.
    /// </summary>
    [Serializable]
    public class CKException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected CKException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public CKException( string message )
            : base( message )
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="args"></param>
        public CKException( string messageFormat, params object[] args )
            : base( String.Format( messageFormat, args ) )
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CKException( string message, Exception innerException )
            : base( message, innerException )
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="innerException"></param>
        /// <param name="messageFormat"></param>
        /// <param name="args"></param>
        public CKException( Exception innerException, string messageFormat, params object[] args )
            : base( String.Format( messageFormat, args ), innerException )
        {
        }

    }
}

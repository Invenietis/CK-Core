#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\InvalidFileException.cs) is part of CiviKey. 
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
using System.Text;
using System.Runtime.Serialization;

namespace CK.Core
{
    /// <summary>
    /// <see cref="CKException"/> raised when something in a file goes wrong.
    /// </summary>
    [Serializable]
    public class InvalidFileException : CKException
    {
        /// <summary>
        /// Gets the path of the current file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Protected constructor, that will intialize the <see cref="CKException"/> with
        /// a <see cref="SerializationInfo"/> and a <see cref="StreamingContext"/>.
        /// </summary>
        /// <param name="info">Given to initialize the <see cref="CKException"/>.</param>
        /// <param name="context">Given to initialize the <see cref="CKException"/>.</param>
        protected InvalidFileException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
            info.AddValue( "FilePath", FilePath );
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            base.GetObjectData( info, context );
            FilePath = info.GetString( "FilePath" );
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        public InvalidFileException( string filePath, string message )
            : base( message )
        {
            FilePath = filePath;
        }
         /// <summary>
         /// Constructor
         /// </summary>
         /// <param name="filePath"></param>
         /// <param name="messageFormat"></param>
         /// <param name="args"></param>
        public InvalidFileException( string filePath, string messageFormat, params object[] args )
            : base( messageFormat, args )
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InvalidFileException( string filePath, string message, Exception innerException )
            : base( message, innerException )
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="innerException"></param>
        /// <param name="messageFormat"></param>
        /// <param name="args"></param>
        public InvalidFileException( string filePath, Exception innerException, string messageFormat, params object[] args )
            : base( innerException, messageFormat, args )
        {
            FilePath = filePath;
        }

    }
}

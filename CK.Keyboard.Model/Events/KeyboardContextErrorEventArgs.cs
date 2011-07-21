#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Model\Context\Events\ContextErrorEventArgs.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines the error event. This may wrap an exception or simply defines an error message.
    /// </summary>
    public class KeyboardContextErrorEventArgs : KeyboardContextEventArgs
    {
        /// <summary>
        /// Gets the error message. It must be localized (if possible). When not specified 
        /// during construction, it is the <see cref="Exception.Message">message</see> of the <see cref="Exception"/>.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the exception. Can be null if only a <see cref="Message"/> is specified.
        /// </summary>
        public Exception Exception { get; private set; }

        public KeyboardContextErrorEventArgs( IKeyboardContext ctx, string message )
            : base( ctx )
        {
            if( message == null || message.Trim().Length == 0 ) throw new ArgumentNullException( "message" );
            Exception = null;
            Message = message;
        }

        public KeyboardContextErrorEventArgs( IKeyboardContext ctx, Exception ex )
            : base( ctx )
        {
            Exception = ex;
            Message = ex.Message;
        }

        public KeyboardContextErrorEventArgs( IKeyboardContext ctx, Exception ex, string message )
            : base( ctx )
        {
            Exception = ex;
            Message = message != null && message.Length > 0 ? message : ex.Message;
        }
    }
}

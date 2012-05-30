#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\ActivityLogGroupConclusion.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Describes a conclusion emitted by a <see cref="IActivityLoggerClientBase"/>.
    /// </summary>
    public struct ActivityLogGroupConclusion
    {
        /// <summary>
        /// The log client that emitted the conclusion. Never null.
        /// </summary>
        public readonly IActivityLoggerClientBase Emitter;

        /// <summary>
        /// The conclusion (never null). Its <see cref="Object.ToString"/>
        /// method SHOULD provide a correct display of the conclusion (it
        /// can be a string).
        /// </summary>
        public readonly object Conclusion;

        /// <summary>
        /// Initializes a new conclusion.
        /// </summary>
        /// <param name="emitter">Must not be null.</param>
        /// <param name="conclusion">Must not be null and its ToString should not be null, empty nor white space.</param>
        public ActivityLogGroupConclusion( IActivityLoggerClientBase emitter, object conclusion )
            : this( conclusion, emitter )
        {
            if( emitter == null ) throw new ArgumentNullException( "emitter" );
            if( conclusion == null ) throw new ArgumentException( "conclusion" );
        }

        internal ActivityLogGroupConclusion( object conclusion, IActivityLoggerClientBase emitter )
        {
            Debug.Assert( conclusion != null && emitter != null );
            Emitter = emitter;
            Conclusion = conclusion;
        }

        /// <summary>
        /// Overriden to return <see cref="Conclusion"/>.ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Conclusion.ToString();
        }
    }

}

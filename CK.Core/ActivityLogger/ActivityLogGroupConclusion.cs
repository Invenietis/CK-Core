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
    /// Describes the conclusion of a group. Conclusions are simply <see cref="Text"/> <see cref="Tag"/>ged with a <see cref="CKTrait"/>.
    /// </summary>
    public struct ActivityLogGroupConclusion
    {
        /// <summary>
        /// The tag (never null).
        /// It may be combined but is often atomic like <see cref="ActivityLogger.TagUserConclusion"/>, 
        /// or <see cref="ActivityLogger.TagGetTextConclusion"/>.
        /// </summary>
        public readonly CKTrait Tag;

        /// <summary>
        /// The conclusion (never null).
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Initializes a new conclusion for a group.
        /// </summary>
        /// <param name="conclusion">Must not be null (may be empty).</param>
        /// <param name="tag">Must not be null and be registered in <see cref="ActivityLogger.RegisteredTags"/>.</param>
        public ActivityLogGroupConclusion( string conclusion, CKTrait tag )
        {
            if( tag == null || tag.Context != ActivityLogger.RegisteredTags ) throw new ArgumentException( R.TagMustBeRegisteredInActivityLogger, "tag" );
            if( conclusion == null ) throw new ArgumentNullException( "conclusion" );
            Tag = tag;
            Text = conclusion;
        }

        internal ActivityLogGroupConclusion( CKTrait t, string conclusion )
        {
            Debug.Assert( t != null && t.Context == ActivityLogger.RegisteredTags );
            Debug.Assert( conclusion != null );
            Tag = t;
            Text = conclusion;
        }

        /// <summary>
        /// Explicit test for <see cref="Text"/> and <see cref="Tag"/> equality.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <returns>True when equal.</returns>
        public override bool Equals( object obj )
        {
            if( obj is ActivityLogGroupConclusion )
            {
                ActivityLogGroupConclusion c = (ActivityLogGroupConclusion)obj;
                return c.Tag == Tag && c.Text == Text;
            }
            return false;
        }

        /// <summary>
        /// Computes the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Tag.GetHashCode() ^ Text.GetHashCode();
        }

        /// <summary>
        /// Overriden to return <see cref="Text"/>.
        /// </summary>
        /// <returns>Text field.</returns>
        public override string ToString()
        {
            return Text;
        }
    }

}

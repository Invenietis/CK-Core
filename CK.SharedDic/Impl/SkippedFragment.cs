#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\SkippedFragment.cs) is part of CiviKey. 
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
using CK.Storage;
using CK.Core;

namespace CK.SharedDic
{
	/// <summary>
	/// Holds skipped fragments during read of Xml document.
	/// </summary>
    internal sealed class SkippedFragment
	{
		/// <summary>
		/// Object that holds this fragment.
		/// </summary>
		public readonly object Obj;

        /// <summary>
        /// Plugin identifier for this fragment.
        /// </summary>
        public readonly Guid PluginId;

        /// <summary>
		/// Storage bookmark for this fragment.
        /// It can be set since a skipped fragment may be replaced by a new one (during a past operation
        /// for example).
		/// </summary>
		public IStructuredReaderBookmark Bookmark;

        internal SkippedFragment( object o, Guid p, Version version, IStructuredReaderBookmark bookmark )
		{
			Obj = o;
			PluginId = p;
            // It is currently useless to store the Version (since the bookmark is the outer xml element, it contains the version attribute).
            // If needed, the code is ready.
            // Version = version;
			Bookmark = bookmark;
		}

        internal SkippedFragment Clone()
        {
            return (SkippedFragment)MemberwiseClone();
        }

        internal void Restore( SharedDictionaryImpl dic, MergeMode mergeMode )
        {
            using( IStructuredReader sr = Bookmark.Restore( dic.ServiceProvider ) )
            {
                using( var r = dic.RegisterReader( sr, mergeMode ) )
                {
                    r.ReadPluginsData( Obj );
                }
            }
        }

        internal bool TryRestore( SharedDictionaryImpl dic, MergeMode mergeMode )
        {
            INamedVersionedUniqueId uid = dic.FindPlugin( PluginId );
            if( uid != null )
            {
                Restore( dic, mergeMode );
                return true;
            }
            return false;
        }
    }

}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\SharedDictionaryImpl.SkippedFragments.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Xml;
using CK.Core;
using CK.Plugin.Config;
using CK.Storage;

namespace CK.SharedDic
{
    internal partial class SharedDictionaryImpl
    {
        Dictionary<object, List<SkippedFragment>> _fragments;

        internal Dictionary<object, List<SkippedFragment>> Fragments { get { return _fragments; } }

        internal List<SkippedFragment> FindOrCreateFragments( object knownObject )
        {
            if( Contains( knownObject ) )
            {
                List<SkippedFragment> f;
                if( !_fragments.TryGetValue( knownObject, out f ) )
                {
                    f = new List<SkippedFragment>();
                    _fragments.Add( knownObject, f );
                }
                return f;
            }
            return null;
        }

        internal void ClearFragments( object o )
        {
            ClearFragments( f => f.Obj == o );
        }

        internal void ClearFragments( Guid id )
        {
            ClearFragments( f => f.PluginId == id );
        }

        internal void ClearFragments( INamedVersionedUniqueId id )
        {
            ClearFragments( f => f.PluginId == id.UniqueId );
        }

        internal List<SkippedFragment> GetSkippedFragments( object o )
        {
            return _fragments.GetValueWithDefault( o, null );
        }

        internal void ImportFragments( Dictionary<object, List<SkippedFragment>> source, MergeMode mergeMode )
        {
            foreach( KeyValuePair<object,List<SkippedFragment>> s in source )
            {
                List<SkippedFragment> mine = FindOrCreateFragments( s.Key );
                if( mine != null )
                {
                    if( mergeMode == MergeMode.None )
                    {
                        mine.Clear();
                        foreach( SkippedFragment sF in s.Value )
                        {
                            // If the imported fragment is for a live plugin,
                            // we must restore it.
                            if( !sF.TryRestore( this, mergeMode ) )
                            {
                                mine.Add( sF.Clone() );
                            }
                        }
                    }
                    else
                    {
                        foreach( SkippedFragment sF in s.Value )
                        {
                            int iMyFragment = IndexOf( mine, sF.PluginId );
                            if( iMyFragment < 0 )
                            {
                                // We did not find the fragment here. It may be because the
                                // plugin is alive: if this is the case, we must restore the fragment.
                                if( !sF.TryRestore( this, mergeMode ) )
                                {
                                    mine.Add( sF.Clone() );
                                }
                            }
                            else
                            {
                                if( mergeMode == MergeMode.ErrorOnDuplicate ) throw new CKException( "Duplicate fragment." );
                                if( mergeMode == MergeMode.ReplaceExisting )
                                {
                                    mine.RemoveAt( iMyFragment );
                                    mine.Add( sF.Clone() );
                                }
                            }
                        }
                    }

                    // remove the skippedFragment if the list is empty
                    if( mine.Count == 0 ) _fragments.Remove( s.Key );
                }
            }
        }

        static int IndexOf( List<SkippedFragment> list, Guid id )
        {
            for( int i = 0; i < list.Count; ++i )
                if( list[i].PluginId == id ) return i;
            return -1;
        }

        internal void StoreSkippedFragment( object o, Guid p, Version version, IStructuredReaderBookmark fragment )
        {
            List<SkippedFragment> f;
            if( !_fragments.TryGetValue( o, out f ) )
            {
                f = new List<SkippedFragment>();
                _fragments.Add( o, f );
                f.Add( new SkippedFragment( o, p, version, fragment ) );
                return;
            }
            foreach( SkippedFragment already in f )
            {
                if( already.PluginId == p )
                {
                    already.Bookmark = fragment;
                    return;
                }
            }
            f.Add( new SkippedFragment( o, p, version, fragment ) );
        }

        /// <summary>
        /// Clears values matching the filter parameter.     
        /// </summary>
        /// <param name="filter"></param>
        /// <remarks>If the entry has no values anymore, the entry is removed from the skippedFragments dictionary</remarks>
        internal void ClearFragments( Predicate<SkippedFragment> filter )
        {
            List<object> entriesToRemove = ClearFragments( _fragments, filter );
            // Removes empty fragments.
            foreach( object entry in entriesToRemove ) _fragments.Remove( entry );
        }

        static internal List<object> ClearFragments( IEnumerable<KeyValuePair<object, List<SkippedFragment>>> fragments, Predicate<SkippedFragment> filter )
        {
            List<object> entriesToRemove = new List<object>();
            foreach( KeyValuePair<object,List<SkippedFragment>> e in fragments )
            {
                e.Value.RemoveAll( filter );
                if( e.Value.Count == 0 ) entriesToRemove.Add( e.Key );
            }
            return entriesToRemove;
        }
    }

}

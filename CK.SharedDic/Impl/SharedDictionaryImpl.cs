#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\Impl\SharedDictionaryImpl.cs) is part of CiviKey. 
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
using System.Linq;

namespace CK.SharedDic
{
    internal partial class SharedDictionaryImpl : ISharedDictionary
    {
        ISimpleServiceContainer _serviceContainer;
        Dictionary< SharedDictionaryEntry, SharedDictionaryEntry > _values;
        Dictionary< object, PluginConfigByObject > _byObject;
        Dictionary< Guid, PluginConfigByPlugin > _byPlugin;
        Dictionary<SharedDictionaryEntry, FinalDictionary> _finalDictionary;

        public event EventHandler<ConfigChangedEventArgs> Changed;

        public SharedDictionaryImpl( IServiceProvider serviceProvider )
        {
            _serviceContainer = new SimpleServiceContainer( serviceProvider );
            _serviceContainer.Add<ISimpleTypeFinder>( SimpleTypeFinder.WeakDefault );
            _fragments = new Dictionary<object, List<SkippedFragment>>();
            _values = new Dictionary<SharedDictionaryEntry, SharedDictionaryEntry>();
            _byObject = new Dictionary<object, PluginConfigByObject>();
            _byPlugin = new Dictionary<Guid, PluginConfigByPlugin>();
            _finalDictionary = new Dictionary<SharedDictionaryEntry, FinalDictionary>( _comparerForFinalDictionaryMap );
        }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceContainer; }
        }

        public bool Contains( object o )
        {
            return _byObject.ContainsKey( o );
        }

        public void Ensure( object o )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( !_byObject.ContainsKey( o ) ) CreatePluginConfigByObject( o );
        }

        public void Clear( object o )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            DoRemove( o, false );
        }

        public void Destroy( object o )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            DoRemove( o, true );
        }

        void DoRemove( object o, bool definitive )
        {
            PluginConfigByObject co;
            if( _byObject.TryGetValue( o, out co ) )
            {
                if( co.Count > 0 )
                {
                    HashSet<INamedVersionedUniqueId> pluginsTouched = new HashSet<INamedVersionedUniqueId>();
                    foreach( SharedDictionaryEntry e in co )
                    {
                        _values.Remove( e );
                        PluginConfigByPlugin cp = _byPlugin[e.PluginId.UniqueId];
                        cp.Remove( e );
                        _finalDictionary.Remove( e );
                        // HashSet.Add simply returns false if the object already exists.
                        pluginsTouched.Add( e.PluginId );
                    }
                    co.Clear();
                    if( Changed != null )
                    {
                        bool allPluginsConcerned = pluginsTouched.Count == _byPlugin.Count;
                        
                        ChangeStatus changeStatus;
                        if ( definitive ) changeStatus = ChangeStatus.ContainerDestroy;
                        else changeStatus= ChangeStatus.ContainerClear;

                        Changed( this, new ConfigChangedEventArgs( o, new ReadOnlyCollectionOnISet<INamedVersionedUniqueId>( pluginsTouched ), allPluginsConcerned, changeStatus ) );
                    }
                }
                if( definitive ) _byObject.Remove( o );
            }
            // Clears fragment in any case (the object beeing known or not).
            ClearFragments( o );
        }

        public INamedVersionedUniqueId FindPlugin( Guid pluginIdentifier )
        {
            PluginConfigByPlugin cp;
            if( _byPlugin.TryGetValue( pluginIdentifier, out cp ) ) return cp.PluginId;
            return null;
        }
        
        public bool Contains( INamedVersionedUniqueId p )
        {
            if( p == null ) throw new ArgumentNullException( "p" );
            PluginConfigByPlugin cp;
            return _byPlugin.TryGetValue( p.UniqueId, out cp ) && cp.PluginId == p;
        }

        public INamedVersionedUniqueId Ensure( INamedVersionedUniqueId p )
        {
            if( p == null ) throw new ArgumentNullException( "p" );
            PluginConfigByPlugin cp;
            if( !_byPlugin.TryGetValue( p.UniqueId, out cp ) )
            {
                cp = CreatePluginConfigByPlugin( p );
            }
            return cp.PluginId;
        }

        public void Clear( INamedVersionedUniqueId p )
        {
            if( p == null ) throw new ArgumentNullException( "p" );
            DoRemove( p, false );
        }

        public void Destroy( INamedVersionedUniqueId p )
        {
            if( p == null ) throw new ArgumentNullException( "p" );
            DoRemove( p, true );
        }

        void DoRemove( INamedVersionedUniqueId p, bool definitive )
        {
            PluginConfigByPlugin cp;
            if( _byPlugin.TryGetValue( p.UniqueId, out cp ) && cp.PluginId == p )
            {
                if( cp.Count > 0 )
                {
                    HashSet<object> objectsTouched = new HashSet<object>();
                    foreach( SharedDictionaryEntry e in cp )
                    {
                        _values.Remove( e );
                        PluginConfigByObject co = _byObject[e.Obj];
                        co.Remove( e );
                        _finalDictionary.Remove( e );
                        objectsTouched.Add( e.Obj );
                    }
                    cp.Clear();
                    if( Changed != null )
                    {
                        bool allObjectsConcerned = objectsTouched.Count == _byObject.Count;

                        ChangeStatus changeStatus;
                        if ( definitive ) changeStatus = ChangeStatus.ContainerDestroy;
                        else changeStatus = ChangeStatus.ContainerClear;

                        Changed( this, new ConfigChangedEventArgs( new ReadOnlyCollectionOnISet<object>( objectsTouched ), allObjectsConcerned, p, changeStatus ) );
                    }
                }
                if( definitive ) _byPlugin.Remove( p.UniqueId );
            }
            // Clears fragment in any case (the plugin beeing known or not).
            ClearFragments( p );
        }

        public int Count( object o, INamedVersionedUniqueId p )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            FinalDictionary d;
            return _finalDictionary.TryGetValue( new SharedDictionaryEntry( o, p, null ), out d ) ? d.Count : 0;
        }

        public bool Contains( object o, INamedVersionedUniqueId p )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            return _finalDictionary.ContainsKey( new SharedDictionaryEntry( o, p, null ) );
        }

        public bool Contains( object o, INamedVersionedUniqueId p, string k )
        {
            return _values.ContainsKey( new SharedDictionaryEntry( o, p, k ) );
        }

        public void Add( object o, INamedVersionedUniqueId p, string k, object value )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );
            Add( new SharedDictionaryEntry( o, p, k, value ) );
        }

        public void Import( ISharedDictionary source, MergeMode mergeMode )
        {
            SharedDictionaryImpl dicSource = (SharedDictionaryImpl)source;
            if( dicSource != null && dicSource != this && dicSource._values.Count > 0 )
            {
                ImportFragments( dicSource._fragments, mergeMode );
                if( mergeMode == MergeMode.None ) ClearAll();
                foreach( SharedDictionaryEntry s in dicSource._values.Values )
                {
                    ImportValue( new SharedDictionaryEntry( s.Obj, s.PluginId, s.Key, s.Value ), mergeMode );
                }
            }
        }

        public void ClearAll()
        {
            var previousByObject = _byObject;
            var previousByPlugin = _byPlugin;

            for( int i = _values.Keys.Count - 1; i >= 0; i-- ) _values.Remove( _values.ElementAt( i ).Key );                         
            foreach ( var key in _byObject.Keys ) _byObject[key].Clear();
            foreach ( var key in _byPlugin.Keys ) _byPlugin[key].Clear();            
            foreach ( var key in _finalDictionary.Keys ) _finalDictionary[key].Clear();
            
            _fragments.Clear();

            if ( Changed != null )
            {
                var pluginsWrapper = new ReadOnlyCollectionTypeConverter<INamedVersionedUniqueId, Guid>( previousByPlugin.Keys, g => previousByPlugin[g].PluginId, uid => uid.UniqueId );
                Changed( this, new ConfigChangedEventArgs( new ReadOnlyCollectionOnICollection<object>( previousByObject.Keys ), true, pluginsWrapper, true, ChangeStatus.ContainerClear ) );
            }
        }

        public void DestroyAll()
        {
            var previousByObject = _byObject;
            var previousByPlugin = _byPlugin;
            _values.Clear();
            _byObject = new Dictionary<object, PluginConfigByObject>();
            _byPlugin = new Dictionary<Guid, PluginConfigByPlugin>();
            _finalDictionary.Clear();
            _fragments.Clear();
            if( Changed != null )
            {
                var pluginsWrapper = new ReadOnlyCollectionTypeConverter<INamedVersionedUniqueId, Guid>( previousByPlugin.Keys, g => previousByPlugin[g].PluginId, uid => uid.UniqueId );
                Changed( this, new ConfigChangedEventArgs( new ReadOnlyCollectionOnICollection<object>( previousByObject.Keys ), true, pluginsWrapper, true, ChangeStatus.ContainerDestroy ) );
            }
        }

        private void Add( SharedDictionaryEntry eWithValue )
        {
            // First ensures that the plugin is registered.
            // This may throw an ArgumentException if the Guid is associated to another IVersionedUniqueId.
            PluginConfigByPlugin cp;
            if( !_byPlugin.TryGetValue( eWithValue.PluginId.UniqueId, out cp ) )
            {
                cp = CreatePluginConfigByPlugin( eWithValue.PluginId );
            }
            // If the entry already exists, an ArgumentException is thrown.
            // If this happens it means that the IVersionedUniqueId is already registered:
            // we do not have to clean up the plugin registration.
            _values.Add( eWithValue, eWithValue );
            // No exception: let us add into the other indices.

            cp.Add( eWithValue );

            PluginConfigByObject co;
            if( !_byObject.TryGetValue( eWithValue.Obj, out co ) )
            {
                co = CreatePluginConfigByObject( eWithValue.Obj );
            }
            co.Add( eWithValue );

            // Increment the final dictionary count.
            FinalDictionary d;
            if( !_finalDictionary.TryGetValue( eWithValue, out d ) )
            {
                d = new FinalDictionary( this, eWithValue.Obj, eWithValue.PluginId );
                _finalDictionary.Add( eWithValue, d );
            }
            d.Count++;
            if( Changed != null ) Changed( this, new ConfigChangedEventArgs( eWithValue, eWithValue, ChangeStatus.Add ) );
        }

        /// <summary>
        /// Throws an ArgumentException if the Guid is already associated to 
        /// another IVersionedUniqueId.
        /// </summary>
        private PluginConfigByPlugin CreatePluginConfigByPlugin( INamedVersionedUniqueId p )
        {
            PluginConfigByPlugin cp = new PluginConfigByPlugin( p );
            _byPlugin.Add( p.UniqueId, cp );
            var entriesToRemove = ClearFragments( _fragments.ToReadOnlyCollection(),  f =>
            {
                if( f.PluginId == p.UniqueId )
                {
                    f.Restore( this, MergeMode.ErrorOnDuplicate );
                    return true;
                }
                return false;
            } );
            foreach( object entry in entriesToRemove ) _fragments.Remove( entry );
            return cp;
        }

        private PluginConfigByObject CreatePluginConfigByObject( object o )
        {
            PluginConfigByObject co = new PluginConfigByObject();
            _byObject.Add( o, co );
            return co;
        }

        IObjectPluginConfig IConfigContainer.GetObjectPluginConfig( object o, INamedVersionedUniqueId p, bool ensure )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            return GetFinalDictionary( new SharedDictionaryEntry( o, p, null ), ensure );
        }

        internal FinalDictionary GetFinalDictionary( object o, INamedVersionedUniqueId p, bool ensure )
        {
            return GetFinalDictionary( new SharedDictionaryEntry( o, p, null ), ensure );
        }

        internal FinalDictionary GetFinalDictionary( SharedDictionaryEntry e, bool ensure )
        {
            FinalDictionary d;
            if( !_finalDictionary.TryGetValue( e, out d ) )
            {
                if( ensure )
                {
                    if( !_byObject.ContainsKey( e.Obj ) ) CreatePluginConfigByObject( e.Obj );
                    d = new FinalDictionary( this, e.Obj, e.PluginId );
                    _finalDictionary.Add( e, d );
                    if( !_byPlugin.ContainsKey( e.PluginId.UniqueId ) ) CreatePluginConfigByPlugin( e.PluginId );
                }
            }
            return d;
        }

        public object this[object o, INamedVersionedUniqueId p, string k]
        {
            get
            {
                if( o == null ) throw new ArgumentNullException( "o" );
                if( p == null ) throw new ArgumentNullException( "p" );
                if( k == null ) throw new ArgumentNullException( "k" );

                SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
                SharedDictionaryEntry result;
                return _values.TryGetValue( e, out result ) ? result.Value : null;
            }
            set 
            { 
                Set( o, p, k, value ); 
            }
        }

        public ChangeStatus Set( object o, INamedVersionedUniqueId p, string k, object value )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );

            SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                if( !Equals( result.Value, value ) )
                {
                    result.Value = value;
                    if( Changed != null ) Changed( this, new ConfigChangedEventArgs( result, result, ChangeStatus.Update ) );
                    return ChangeStatus.Update;
                }
                return ChangeStatus.None;
            }
            e.Value = value;
            Add( e );
            return ChangeStatus.Add;
        }

        public T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, T value )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );

            SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                return (T)result.Value;
            }
            e.Value = value;
            Add( e );
            return value;
        }

        public T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, T value, Func<object, T> converter )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );
            if( converter == null ) throw new ArgumentNullException( "converter" );
            
            SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                if( result.Value is T ) return (T)result.Value;
                T valT = converter( result.Value );
                result.Value = valT;
                if( Changed != null ) Changed( this, new ConfigChangedEventArgs( result, result, ChangeStatus.Update ) );
                return valT;
            }
            e.Value = value;
            Add( e );
            return value;
        }

        internal void ImportValue( SharedDictionaryEntry entryWithValue, MergeMode mergeMode )
        {
            if( mergeMode == MergeMode.None || mergeMode == MergeMode.ReplaceExisting ) this[entryWithValue.Obj,entryWithValue.PluginId,entryWithValue.Key] = entryWithValue.Value;
            else if( mergeMode == MergeMode.ErrorOnDuplicate ) Add( entryWithValue );
            else if( mergeMode == MergeMode.PreserveExisting )
            {
                GetOrSet( entryWithValue.Obj, entryWithValue.PluginId, entryWithValue.Key, entryWithValue.Value );
            }
            else if( mergeMode == MergeMode.ReplaceExistingTryMerge )
            {
                if( !Merge( entryWithValue ) )
                    this[entryWithValue.Obj, entryWithValue.PluginId, entryWithValue.Key] = entryWithValue.Value;
            }
        }

        bool Merge( SharedDictionaryEntry e )
        {
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                IMergeable existing = result.Value as IMergeable;
                if( existing != null ) return existing.Merge( e.Value );
            }
            return false;
        }

        public T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, Func<T> value )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );
            
            SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                return (T)result.Value;
            }
            T val = value == null ? default( T ) : value();
            e.Value = val;
            Add( e );
            return val;
        }

        public T GetOrSet<T>( object o, INamedVersionedUniqueId p, string k, Func<T> value, Func<object, T> converter )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );
            
            if( converter == null ) throw new ArgumentNullException( "converter" );
            SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                if( result.Value is T ) return (T)result.Value;
                T valT = converter( result.Value );
                result.Value = valT;
                if( Changed != null ) Changed( this, new ConfigChangedEventArgs( result, result, ChangeStatus.Update ) );
                return valT;
            }
            T val = value == null ? default( T ) : value();
            e.Value = val;
            Add( e );
            return val;
        }

        public bool Remove( object o, INamedVersionedUniqueId p, string k )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            if( k == null ) throw new ArgumentNullException( "k" );

            SharedDictionaryEntry e = new SharedDictionaryEntry( o, p, k );
            SharedDictionaryEntry result;
            if( _values.TryGetValue( e, out result ) )
            {
                PluginConfigByObject co = _byObject[e.Obj];
                co.Remove( result );
                PluginConfigByPlugin cp = _byPlugin[e.PluginId.UniqueId];
                cp.Remove( result );
                _finalDictionary[e].Count--;
                _values.Remove( result );
                result.Value = null;
                if( Changed != null ) Changed( this, new ConfigChangedEventArgs( e, e, ChangeStatus.Delete ) );
                return true;
            }
            return false;
        }

        public void Clear( object o, INamedVersionedUniqueId p )
        {
            if( o == null ) throw new ArgumentNullException( "o" );
            if( p == null ) throw new ArgumentNullException( "p" );
            
            FinalDictionary d;
            if( !_finalDictionary.TryGetValue( new SharedDictionaryEntry( o, p, null ), out d ) || d.Count == 0 ) return;
            PluginConfigByObject co = _byObject[o];
            PluginConfigByPlugin cp = _byPlugin[p.UniqueId];
            for( int i = 0; i < co.Count; ++i )
            {
                SharedDictionaryEntry e = co[i];
                if( e.PluginId == p )
                {
                    co.RemoveAt( i );
                    cp.Remove( e );
                    _values.Remove( e );
                }
            }
            d.Count = 0;
            if( Changed != null ) Changed( this, new ConfigChangedEventArgs( o, p, ChangeStatus.ContainerClear ) );
        }

        internal void ForEach( object o, INamedVersionedUniqueId p, Action<SharedDictionaryEntry> a )
        {
            PluginConfigByObject co;
            if( _byObject.TryGetValue( o, out co ) && co.Count > 0 )
            {
                if( co.Count < 10 ) co.ForEach( p, a );
                else
                {
                    PluginConfigByPlugin cp;
                    if( _byPlugin.TryGetValue( p.UniqueId, out cp ) && cp.PluginId == p && cp.Count > 0 )
                    {
                        if( cp.Count < co.Count )
                            cp.ForEach( o, a );
                        else co.ForEach( p, a );
                    }
                }
            }
        }

        public ISharedDictionaryReader RegisterReader( IStructuredReader reader, MergeMode mergeMode )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );
            ISharedDictionaryReader rdr = new SharedDictionaryReader( this, reader, mergeMode );
            reader.ServiceContainer.Add<ISharedDictionaryReader>( rdr, Util.ActionDispose );
            return rdr;
        }

        public ISharedDictionaryWriter RegisterWriter( IStructuredWriter writer )
        {
            if( writer == null ) throw new ArgumentNullException( "writer" );
            ISharedDictionaryWriter wrt = new SharedDictionaryWriter( this, writer );
            writer.ServiceContainer.Add<ISharedDictionaryWriter>( wrt, Util.ActionDispose );
            return wrt;
        }

        /// <summary>
        /// Used by SharedDictionaryWriter.
        /// </summary>
        internal bool TryGetPluginConfigByObject( object o, out PluginConfigByObject co )
        {
            return _byObject.TryGetValue( o, out co );
        }

        #region IEqualityComparer implementation for FinalDictionary map.

        class FinalDictionaryComparer : IEqualityComparer<SharedDictionaryEntry>
        {
            public bool Equals( SharedDictionaryEntry x, SharedDictionaryEntry y )
            {
                return x.Obj == y.Obj && y.PluginId == x.PluginId;
            }

            public int GetHashCode( SharedDictionaryEntry e )
            {
                return e.Obj.GetHashCode() ^ e.PluginId.GetHashCode();
            }
        }

        static readonly IEqualityComparer<SharedDictionaryEntry> _comparerForFinalDictionaryMap = new FinalDictionaryComparer();

        #endregion

    }

}

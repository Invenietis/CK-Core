#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\Collections\PluginStatusCollection.cs) is part of CiviKey. 
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
using System.Linq;
using System.Collections.Generic;
using CK.Storage;
using CK.Core;
using System.Xml;
using System.Diagnostics;

namespace CK.Plugin.Config
{
    internal class PluginStatusCollection : IPluginStatusCollection, IStructuredSerializable
    {
        ConfigurationBase _holder;
        Dictionary<Guid, PluginStatus> _pluginStatusDic;
        ReadOnlyCollectionOnICollection<PluginStatus> _pluginStatusReadOnlyCollection;

        public event EventHandler<PluginStatusCollectionChangingEventArgs> Changing;

        public event EventHandler<PluginStatusCollectionChangedEventArgs> Changed;

        internal PluginStatusCollection( ConfigurationBase holder )
        {
            _holder = holder;
            _pluginStatusDic = new Dictionary<Guid, PluginStatus>();
            _pluginStatusReadOnlyCollection = new ReadOnlyCollectionOnICollection<PluginStatus>( _pluginStatusDic.Values );
        }

        internal bool CanChange( ChangeStatus action, Guid pluginId, ConfigPluginStatus status )
        {
            if( Changing != null )
            {
                PluginStatusCollectionChangingEventArgs eCancel = new PluginStatusCollectionChangingEventArgs( this, action, pluginId, status );
                Changing( this, eCancel );
                return !eCancel.Cancel;
            }
            return true;
        }

        internal void Change( ChangeStatus action, Guid pluginId, ConfigPluginStatus status )
        {
            _holder.OnPluginStatusCollectionChanged( action, pluginId, status );
            if( Changed != null )
            {
                PluginStatusCollectionChangedEventArgs e = new PluginStatusCollectionChangedEventArgs( this, action, pluginId, status );
                Changed( this, e );
            }
        }

        public void SetStatus( Guid pluginId, ConfigPluginStatus status )
        {
            PluginStatus currentPluginStatus;
            if ( _pluginStatusDic.TryGetValue( pluginId, out currentPluginStatus ) )
            {
                currentPluginStatus.Status = status;
            }
            else if ( CanChange( ChangeStatus.Add, pluginId, status ) )
            {
                PluginStatus newStatus = new PluginStatus( this, pluginId, status );
                _pluginStatusDic.Add( pluginId, newStatus );
                Change( ChangeStatus.Add, pluginId, status );
            }
        }

        public ConfigPluginStatus GetStatus( Guid pluginID, ConfigPluginStatus defaultStatus )
        {
            if( _pluginStatusDic.ContainsKey( pluginID ) )
                return _pluginStatusDic[pluginID].Status;
            return defaultStatus;
        }

        public IPluginStatus GetPluginStatus( Guid pluginID )
        {
            if( _pluginStatusDic.ContainsKey( pluginID ) )
                return _pluginStatusDic[pluginID];
            return null;
        }

        internal void FireResetEvent()
        {
            Change( ChangeStatus.ContainerUpdate, Guid.Empty, 0 );
        }

        /// <summary>
        /// Clears all plugin status configuration
        /// Sends Changing and Changed
        /// </summary>
        public void Clear()
        {
            if( CanChange( ChangeStatus.ContainerClear, Guid.Empty, 0 ) )
            {
                _pluginStatusDic.Clear();
                Change( ChangeStatus.ContainerClear, Guid.Empty, 0 );
            }
        }

        public void Clear( Guid pluginId )
        {
            PluginStatus value;
            if( _pluginStatusDic.TryGetValue( pluginId, out value ) ) value.Destroy();
        }

        /// <summary>
        /// Deletes the PluginStatus set as parameter.
        /// Sends StatusChanging & StatusChanged
        /// </summary>
        /// <param name="status">The <see cref="PluginStatus"/> to remove</param>
        /// <returns>True if deletion has been completed, false otherwise</returns>
        internal bool OnDestroy( PluginStatus status )
        {
            if( CanChange( ChangeStatus.Delete, status.PluginId, status.Status ) )
            {
                if( _pluginStatusDic.ContainsValue( status ) )
                {
                    _pluginStatusDic.Remove( status.PluginId );

                    Change( ChangeStatus.Delete, status.PluginId, status.Status );
                    return true;
                }
            }
            return false;
        }

        #region IReadOnlyCollection

        public bool Contains( object item )
        {
            return _pluginStatusReadOnlyCollection.Contains( item );
        }

        public int Count
        {
            get { return _pluginStatusReadOnlyCollection.Count; }
        }

        public IEnumerator<IPluginStatus> GetEnumerator()
        {
            return _pluginStatusReadOnlyCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _pluginStatusReadOnlyCollection.GetEnumerator();
        }

        #endregion

        public void ReadContent( IStructuredReader sr )
        {
            XmlReader r = sr.Xml;
            r.Read();
            Dictionary<Guid,PluginStatus> newContent = new Dictionary<Guid, PluginStatus>();
            while( r.IsStartElement( "PluginStatus" ) )
            {
                Guid guid = new Guid( r.GetAttribute( "Guid" ) );
                ConfigPluginStatus status = (ConfigPluginStatus)Enum.Parse( typeof( ConfigPluginStatus ), r.GetAttribute( "Status" ) );
                newContent[guid] = new PluginStatus( this, guid, status );
                r.Skip();
            }
            _pluginStatusDic.Clear();
            _pluginStatusDic.AddRange( newContent );

            //may eventually be replaced by some code in SolvedPluginConfiguration
            //Without that, SolvedPluginConfiguration's ResolveConfiguration method is not called
            Change(ChangeStatus.ContainerUpdate, Guid.Empty, 0);
        }

        public void WriteContent( IStructuredWriter sw )
        {
            XmlWriter w = sw.Xml;
            foreach( IPluginStatus p in this )
            {
                w.WriteStartElement( "PluginStatus" );
                w.WriteAttributeString( "Guid", p.PluginId.ToString() );
                w.WriteAttributeString( "Status", p.Status.ToString() );
                w.WriteFullEndElement();
            }
        }

    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\ConfigurationBase.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using CK.Storage;
using System.ComponentModel;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Base class for <see cref="UserConfiguration"/> and <see cref="SystemConfiguration"/>. 
    /// </summary>
    internal abstract class ConfigurationBase : IStructuredSerializable, INotifyPropertyChanged
    {
        protected ConfigurationBase( ConfigManagerImpl configManager, string uriHistoryEntryName )
        {
            ConfigManager = configManager;
            PluginStatusCollection = new PluginStatusCollection( this );
            UriHistoryCollection = new UriHistoryCollection( this, uriHistoryEntryName );
        }

        protected readonly ConfigManagerImpl ConfigManager;

        internal readonly PluginStatusCollection PluginStatusCollection;

        internal readonly UriHistoryCollection UriHistoryCollection;

        public event PropertyChangedEventHandler  PropertyChanged;

        void IStructuredSerializable.ReadContent( IStructuredReader sr )
        {
            sr.Xml.Read();
            sr.ReadInlineObjectStructuredElement( "PluginStatusCollection", PluginStatusCollection );
            sr.ReadInlineObjectStructuredElement( UriHistoryCollection.EntryName + "Collection", UriHistoryCollection );
            sr.GetService<ISharedDictionaryReader>( true ).ReadPluginsDataElement( "Plugins", this );
        }

        void IStructuredSerializable.WriteContent( IStructuredWriter sw )
        {
            sw.Xml.WriteAttributeString( "Version", "1.0.0.0" );
            sw.WriteInlineObjectStructuredElement( "PluginStatusCollection", PluginStatusCollection );
            sw.WriteInlineObjectStructuredElement( UriHistoryCollection.EntryName + "Collection", UriHistoryCollection );
            sw.GetService<ISharedDictionaryWriter>( true ).WritePluginsDataElement( "Plugins", this );
        }


        internal void OnPluginStatusCollectionChanged( ChangeStatus action, Guid pluginId, ConfigPluginStatus status )
        {
            OnCollectionChanged(); 
        }

        internal virtual void OnCollectionChanged()
        {
        }

        internal void FireCurrentHistoChangedEvent( string uriHistoEntryName )
        {
            FirePropertyChangedEvent( "Current" + uriHistoEntryName );
        }

        internal void FirePropertyChangedEvent( string propertyName )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

    }
}

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

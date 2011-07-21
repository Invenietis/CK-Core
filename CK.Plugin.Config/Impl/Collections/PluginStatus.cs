using System;
using CK.Core;

namespace CK.Plugin.Config
{
    internal class PluginStatus : IPluginStatus
    {
        PluginStatusCollection _holder;
        ConfigPluginStatus _status;
        Guid _pluginId;

        public Guid PluginId
        {
            get { return _pluginId; }
            set { _pluginId = value; }
        }

        public ConfigPluginStatus Status
        { 
            get { return _status; } 
            set 
            {
                if( _status != value &&  _holder.CanChange( ChangeStatus.Update, _pluginId, value ) )
                {
                    _status = value;
                    _holder.Change( ChangeStatus.Update, _pluginId, value );
                }
            } 
        }

        public void Destroy()
        {
            if( _holder.OnDestroy( this ) ) _holder = null;
        }

        public PluginStatus(PluginStatusCollection holder, Guid pluginId, ConfigPluginStatus status)
        {
            _holder = holder;
            _pluginId = pluginId;
            _status = status;
        }
    }
}

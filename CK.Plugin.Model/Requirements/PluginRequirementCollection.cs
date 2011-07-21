using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CK.Core;
using System.Diagnostics;
using System.ComponentModel;

namespace CK.Plugin
{
    internal class PluginRequirementCollection : IPluginRequirementCollection
    {
        PluginRequirement _first;
        int _count;

        public PluginRequirementCollection()
        {
        }

        public event EventHandler<PluginRequirementCollectionChangingEventArgs>  Changing;
        
        public event EventHandler<PluginRequirementCollectionChangedEventArgs>  Changed;
        
        internal bool CanChange( ChangeStatus action, Guid pluginID, RunningRequirement requirement )
        {
            if( Changing != null )
            {
                PluginRequirementCollectionChangingEventArgs eCancel = new PluginRequirementCollectionChangingEventArgs( this, action, pluginID, requirement );
                Changing( this, eCancel );
                return !eCancel.Cancel;
            }
            return true;
        }

        internal void Change( ChangeStatus action, Guid pluginID, RunningRequirement requirement )
        {
            if( Changed != null )
            {
                PluginRequirementCollectionChangedEventArgs e = new PluginRequirementCollectionChangedEventArgs( this, action, pluginID, requirement );
                Changed( this, e );
            }
        }

        public PluginRequirement AddOrSet( Guid pluginID, RunningRequirement requirement )
        {
            PluginRequirement req = this.FirstOrDefault( r => r.PluginId == pluginID );
            if( req != null ) 
            {
                if( req.Requirement != requirement && CanChange( ChangeStatus.Update, pluginID, requirement ) )
                {
                    req.Requirement = requirement;
                    Change( ChangeStatus.Update, pluginID, requirement );
                }
            }
            else if( CanChange( ChangeStatus.Add, pluginID, requirement ) )
            {
                req = new PluginRequirement( this, pluginID, requirement );
                req.NextElement = _first;
                _first = req;
                _count++;
                Change( ChangeStatus.Add, pluginID, requirement );
            }
            return req;
        }

        public PluginRequirement Find( Guid pluginID )
        {
            return this.FirstOrDefault( r => r.PluginId == pluginID );
        }

        public bool Remove( Guid pluginId )
        {
            PluginRequirement req = Find( pluginId ) as PluginRequirement;
            if( req != null )
            {
                Debug.Assert( req.Holder == this );
                if( !CanChange( ChangeStatus.Delete, req.PluginId, req.Requirement ) ) return false;
                if( _first == req ) _first = req.NextElement;
                else this.First( r => r.NextElement == req ).NextElement = req.NextElement;
                req.Holder = null;
                --_count;
                Change( ChangeStatus.Delete, req.PluginId, req.Requirement );
            }
            return true;
        }

        public bool Clear()
        {
            if( !CanChange( ChangeStatus.ContainerClear, Guid.Empty, RunningRequirement.Optional ) ) return false;
            foreach( PluginRequirement r in this ) r.Holder = null;
            _first = null;
            _count = 0;
            Change( ChangeStatus.ContainerClear, Guid.Empty, RunningRequirement.Optional );
            return true;
        }

        public bool Contains( object item )
        {
            PluginRequirement e = item as PluginRequirement;
            return e != null ? e.Holder == this : false;
        }

        public int Count
        {
            get { return _count; }
        }
            
        public IEnumerator<PluginRequirement> GetEnumerator()
        {
            PluginRequirement e = _first;
            while( e != null )
            {
                yield return e;
                e = e.NextElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

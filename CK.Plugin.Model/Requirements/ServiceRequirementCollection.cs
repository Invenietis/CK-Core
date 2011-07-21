using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin
{
    internal class ServiceRequirementCollection : IServiceRequirementCollection
    {
        ServiceRequirement _first;
        int _count;

        public ServiceRequirementCollection()
        {
        }

        public event EventHandler<ServiceRequirementCollectionChangingEventArgs> Changing;

        public event EventHandler<ServiceRequirementCollectionChangedEventArgs> Changed;

        internal bool CanChange( ChangeStatus action, string serviceAssemblyQualifiedName, RunningRequirement requirement )
        {
            if( Changing != null )
            {
                ServiceRequirementCollectionChangingEventArgs eCancel = new ServiceRequirementCollectionChangingEventArgs( this, action, serviceAssemblyQualifiedName, requirement );
                Changing( this, eCancel );
                return !eCancel.Cancel;
            }
            return true;
        }

        internal void Change( ChangeStatus action, string serviceAssemblyQualifiedName, RunningRequirement requirement )
        {
            if( Changed != null )
            {
                ServiceRequirementCollectionChangedEventArgs e = new ServiceRequirementCollectionChangedEventArgs( this, action, serviceAssemblyQualifiedName, requirement );
                Changed( this, e );
            }
        }

        public ServiceRequirement AddOrSet( string serviceAssemblyQualifiedName, RunningRequirement requirement )
        {
            ServiceRequirement req = GetEnumerable().FirstOrDefault( r => r.AssemblyQualifiedName == serviceAssemblyQualifiedName );
            if( req != null )
            {
                if( req.Requirement != requirement && CanChange( ChangeStatus.Update, serviceAssemblyQualifiedName, requirement ) )
                {
                    req.Requirement = requirement;
                    Change( ChangeStatus.Update, serviceAssemblyQualifiedName, requirement );
                }
            }
            else if ( CanChange( ChangeStatus.Add, serviceAssemblyQualifiedName, requirement ) )
            {
                req = new ServiceRequirement( this, serviceAssemblyQualifiedName, requirement );
                req.NextElement = _first;
                _first = req;
                _count++;
                Change( ChangeStatus.Add, serviceAssemblyQualifiedName, requirement );
            }
            return req;
        }

        public ServiceRequirement Find( string serviceAssemblyQualifiedName )
        {
            return GetEnumerable().FirstOrDefault( r => r.AssemblyQualifiedName == serviceAssemblyQualifiedName );
        }

        public bool Remove( string serviceAssemblyQualifiedName )
        {
            ServiceRequirement req = Find( serviceAssemblyQualifiedName ) as ServiceRequirement;
            if( req != null )
            {
                Debug.Assert( req.Holder == this );
                if( !CanChange( ChangeStatus.Delete, req.AssemblyQualifiedName, req.Requirement ) ) return false;
                if( _first == req ) _first = req.NextElement;
                else GetEnumerable().First( r => r.NextElement == req ).NextElement = req.NextElement;
                req.Holder = null;
                --_count;
                Change( ChangeStatus.Delete, req.AssemblyQualifiedName, req.Requirement );
            }
            return true;
        }

        public bool Clear()
        {
            if( !CanChange( ChangeStatus.ContainerClear, string.Empty, RunningRequirement.Optional ) ) return false;
            foreach( ServiceRequirement r in GetEnumerable() ) r.Holder = null;
            _first = null;
            _count = 0;
            Change( ChangeStatus.ContainerClear, string.Empty, RunningRequirement.Optional );
            return true;
        }

        public bool Contains( object item )
        {
            ServiceRequirement e = item as ServiceRequirement;
            return e != null ? e.Holder == this : false;
        }

        public int Count
        {
            get { return _count; }
        }

        IEnumerable<ServiceRequirement> GetEnumerable()
        {
            ServiceRequirement e = _first;
            while( e != null )
            {
                yield return e;
                e = e.NextElement;
            }
        }

        IEnumerator<ServiceRequirement> IEnumerable<ServiceRequirement>.GetEnumerator()
        {
            return Wrapper<ServiceRequirement>.CreateEnumerator<ServiceRequirement>( GetEnumerable() );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }
    }
}

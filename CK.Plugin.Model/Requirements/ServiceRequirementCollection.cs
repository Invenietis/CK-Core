#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Requirements\ServiceRequirementCollection.cs) is part of CiviKey. 
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
using CK.Core;
using System.Diagnostics;
using System.Xml;

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
            ServiceRequirement req = this.FirstOrDefault( r => r.AssemblyQualifiedName == serviceAssemblyQualifiedName );
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
            return this.FirstOrDefault( r => r.AssemblyQualifiedName == serviceAssemblyQualifiedName );
        }

        public bool Remove( string serviceAssemblyQualifiedName )
        {
            ServiceRequirement req = Find( serviceAssemblyQualifiedName ) as ServiceRequirement;
            if( req != null )
            {
                Debug.Assert( req.Holder == this );
                if( !CanChange( ChangeStatus.Delete, req.AssemblyQualifiedName, req.Requirement ) ) return false;
                if( _first == req ) _first = req.NextElement;
                else this.First( r => r.NextElement == req ).NextElement = req.NextElement;
                req.Holder = null;
                --_count;
                Change( ChangeStatus.Delete, req.AssemblyQualifiedName, req.Requirement );
            }
            return true;
        }

        public bool Clear()
        {
            if( !CanChange( ChangeStatus.ContainerClear, string.Empty, RunningRequirement.Optional ) ) return false;
            foreach( ServiceRequirement r in this ) r.Holder = null;
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

        public IEnumerator<ServiceRequirement> GetEnumerator()
        {
            ServiceRequirement e = _first;
            while( e != null )
            {
                yield return e;
                e = e.NextElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}

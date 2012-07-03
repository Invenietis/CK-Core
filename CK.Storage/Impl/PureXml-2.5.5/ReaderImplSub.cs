#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\ReaderImplSub.cs) is part of CiviKey. 
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
using System.Text;
using CK.Core;
using System.Xml;

namespace CK.Storage
{
    internal sealed class ReaderImplSub : ReaderBase, ISubStructuredReader
    {
        internal readonly ReaderBase Parent;
        ISimpleServiceContainer _serviceContainer;
        IDisposable _jail;

        internal ReaderImplSub( ReaderImpl rootReader, ReaderBase parent )
            : base( rootReader )
        {
            Parent = parent;
            _jail = Root.CreateJail();
            Root.Current = this;
        }

        public override XmlReader Xml
        {
            get { return Root.Xml; }
        }

        public override Version StorageVersion
        {
            get { return Root.StorageVersion; }
        }

        public ActionSequence RootDeserializationActions
        {
            get { return Root.DeserializationActions; }
        }

        public override IStructuredReaderBookmark CreateBookmark()
        {
            return new ReaderBookmark( this );
        }

        public override IServiceProvider BaseServiceProvider
        {
            get { return Root.BaseServiceProvider; }
        }

        public override ISimpleServiceContainer ServiceContainer
        {
            get { return _serviceContainer ?? (_serviceContainer = new SimpleServiceContainer( Parent )); }
        }

        public override object GetService( Type serviceType )
        {
            return _serviceContainer != null ? _serviceContainer.GetService( serviceType ) : Parent.GetService( serviceType );
        }

        //public void EnterScope( string name )
        //{
        //    RootReader.EnterScope( name );
        //}

        //public void LeaveScope()
        //{
        //    _rootReader.LeaveScope();
        //}

        //public IReadOnlyList<string> ScopePath
        //{
        //    get { return _rootReader.ScopePath; }
        //}

        protected override void OnDispose()
        {
            while( Root.Current != this ) Root.Current.Dispose();
            Root.Current = Parent;
            if( _serviceContainer != null ) _serviceContainer.Clear();
            _jail.Dispose();
        }

        protected override void PropagateReadEvent( ObjectReadExDataEventArgs e )
        {
            Parent.RaiseReadEvent( e );
        }

    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\WriterImplSub.cs) is part of CiviKey. 
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
    internal sealed class WriterImplSub : WriterBase, ISubStructuredWriter
    {
        internal readonly WriterBase Parent;
        SimpleServiceContainer _serviceContainer;

        internal WriterImplSub( WriterImpl rootWriter, WriterBase parent )
            : base( rootWriter )
        {
            Parent = parent;
            Root.Current = this;
        }

        public override XmlWriter Xml
        {
            get { return Root.Xml; }
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

        protected override void PropagateWriteEvent( ObjectWriteExDataEventArgs e )
        {
            Parent.RaiseWriteEvent( e );
        }

        protected override void OnDispose()
        {
            while( Root.Current != this ) Root.Current.Dispose();
            Root.Current = Parent;
            if( _serviceContainer != null ) _serviceContainer.Dispose();
        }

    }
}

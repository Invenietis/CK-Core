#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Impl\ChannelFactory.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.RouteConfig;
using CK.Monitoring.GrandOutputHandlers;

namespace CK.Monitoring.Impl
{
    internal sealed class ChannelFactory : RouteActionFactory<HandlerBase, IChannel>, IChannel
    {
        public readonly GrandOutput _grandOutput;
        public readonly EventDispatcher _dispatcher;
        public readonly EventDispatcher.FinalReceiver CommonSinkOnlyReceiver;

        #region EmptyChannel direct implementation

        void IChannel.Initialize()
        {
        }

        void IChannel.Handle( GrandOutputEventInfo logEvent, bool sendToCommonSink )
        {
            if( sendToCommonSink ) _dispatcher.Add( logEvent, CommonSinkOnlyReceiver );
        }

        LogFilter IChannel.MinimalFilter
        {
            get { return LogFilter.Undefined; }
        }

        void IChannel.PreHandleLock()
        {
        }

        void IChannel.CancelPreHandleLock()
        {
        }

        #endregion

        internal ChannelFactory( GrandOutput grandOutput, EventDispatcher dispatcher )
        {
            _grandOutput = grandOutput;
            _dispatcher = dispatcher;
            CommonSinkOnlyReceiver = new EventDispatcher.FinalReceiver( grandOutput.CommonSink, Util.EmptyArray<HandlerBase>.Empty, null );
        }

        protected internal override IChannel DoCreateEmptyFinalRoute()
        {
            return this;
        }

        protected override HandlerBase DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c )
        {
            var a = (HandlerTypeAttribute)Attribute.GetCustomAttribute( c.GetType(), typeof( HandlerTypeAttribute ), true );
            if( a == null ) throw new CKException( "A [HandlerType(typeof(H))] attribute (where H is a CK.Monitoring.GrandOutputHandlers.HandlerBase class) is missing on class '{0}'.", c.GetType().FullName );
            return (HandlerBase)Activator.CreateInstance( a.HandlerType, c );
        }

        protected override HandlerBase DoCreateParallel( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionParallelConfiguration c, HandlerBase[] children )
        {
            return new ParallelHandler( c, children );  
        }

        protected override HandlerBase DoCreateSequence( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionSequenceConfiguration c, HandlerBase[] children )
        {
            return new SequenceHandler( c, children );
        }

        protected internal override IChannel DoCreateFinalRoute( IActivityMonitor monitor, IRouteConfigurationLock configLock, HandlerBase[] actions, string configurationName, object configData, IReadOnlyList<IChannel> routePath )
        {
            return new StandardChannel( _grandOutput.CommonSink, _dispatcher, configLock, actions, configurationName, (GrandOutputChannelConfigData)configData );
        }
    }
}

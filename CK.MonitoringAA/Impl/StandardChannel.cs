#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Impl\StandardChannel.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.GrandOutputHandlers;
using CK.Monitoring.Impl;
using CK.RouteConfig;

namespace CK.Monitoring
{
    internal sealed class StandardChannel : IChannel
    {
        readonly EventDispatcher _dispatcher;
        readonly EventDispatcher.FinalReceiver _receiver;
        readonly EventDispatcher.FinalReceiver _receiverNoCommonSink;
        readonly string _configurationName;
        LogFilter _minimalFilter;

        internal StandardChannel( IGrandOutputSink commonSink, EventDispatcher dispatcher, IRouteConfigurationLock configLock, HandlerBase[] handlers, string configurationName, GrandOutputChannelConfigData configData )
        {
            _dispatcher = dispatcher;
            _receiver = new EventDispatcher.FinalReceiver( commonSink, handlers, configLock );
            _receiverNoCommonSink = new EventDispatcher.FinalReceiver( null, handlers, configLock );
            _configurationName = configurationName;
            if( configData != null ) _minimalFilter = configData.MinimalFilter;
        }

        public void Initialize()
        {
            ChannelOption option = new ChannelOption( _minimalFilter );
            foreach( var s in _receiver.Handlers ) s.CollectChannelOption( option );
            _minimalFilter = option.CurrentMinimalFilter;
        }

        public void Handle( GrandOutputEventInfo logEvent, bool sendToCommonSink )
        {
            if( sendToCommonSink ) _dispatcher.Add( logEvent, _receiver );
            else _dispatcher.Add( logEvent, _receiverNoCommonSink );
        }

        public LogFilter MinimalFilter 
        {
            get { return _minimalFilter; } 
        }

        public void PreHandleLock()
        {
            _receiver.ConfigLock.Lock();
        }

        public void CancelPreHandleLock()
        {
            _receiver.ConfigLock.Unlock();
        }

    }
}

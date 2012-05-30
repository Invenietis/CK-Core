#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\ServiceHost.cs) is part of CiviKey. 
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
using System.Text;
using System.Reflection;
using System.Diagnostics;
using CK.Core;

namespace CK.Plugin.Hosting
{
    internal class ServiceHost : IServiceHost, ILogCenter
    {
        Dictionary<Type,ServiceProxyBase> _proxies;
        CatchExceptionGeneration _catchMode;
        int _nextLSN;
        int _currentDepth;
        object _eventSender;
        ISimpleServiceHostConfiguration _defaultConfiguration;
        List<IServiceHostConfiguration> _configurations;

        public event EventHandler<LogEventArgs> EventCreating;
        public event EventHandler<LogEventArgs> EventCreated;

        public ServiceHost( CatchExceptionGeneration catchMode )
        {
            _eventSender = this;
            _catchMode = catchMode;
            _proxies = new Dictionary<Type, ServiceProxyBase>();
            _currentDepth = 0;
            _defaultConfiguration = new SimpleServiceHostConfiguration();
            _configurations = new List<IServiceHostConfiguration>();
            _configurations.Add( _defaultConfiguration );

            _untrackedErrors = new List<ILogErrorCaught>();
            UntrackedErrors = new ReadOnlyListOnIList<ILogErrorCaught>( _untrackedErrors );
        }

        public ISimpleServiceHostConfiguration DefaultConfiguration 
        { 
            get { return _defaultConfiguration; } 
        }

        private IList<ILogErrorCaught> _untrackedErrors;
        public IReadOnlyList<ILogErrorCaught> UntrackedErrors { get; private set; }

        public void Add( IServiceHostConfiguration configurator )
        {
            if( configurator == null ) throw new ArgumentNullException();
            if( !_configurations.Contains( configurator ) )
            {
                _configurations.Add( configurator );
            }
        }

        public void Remove( IServiceHostConfiguration configurator )
        {
            if( configurator == null ) throw new ArgumentNullException();
            if( configurator != _defaultConfiguration )
            {
                _configurations.Remove( configurator );
            }
        }

        internal object EventSender
        {
            get { return _eventSender; }
            set { _eventSender = value; }
        }

        internal ServiceProxyBase EnsureProxy( Type interfaceType )
        {
            return EnsureProxy( interfaceType, false );
        }

        internal ServiceProxyBase EnsureProxyForExternalService( Type interfaceType, object currentImplementation )
        {
            ServiceProxyBase proxy = EnsureProxy( interfaceType, true );
            proxy.SetExternalImplementation( currentImplementation );
            return proxy;
        }

        ServiceProxyBase EnsureProxy( Type interfaceType, bool isExternalService )
        {
            ServiceProxyBase proxy;
            if( !_proxies.TryGetValue( interfaceType, out proxy ) )
            {
                DefaultProxyDefinition definition = new DefaultProxyDefinition( interfaceType, _catchMode );
                proxy = ProxyFactory.CreateProxy( definition );
                proxy.Initialize( this, isExternalService );
                _proxies.Add( interfaceType, proxy );
                if( definition.IsDynamicService ) _proxies.Add( typeof( IService<> ).MakeGenericType( interfaceType ), proxy );
                ApplyConfiguration( proxy );
            }
            return proxy;
        }

        /// <summary>
        /// For tests only.
        /// </summary>
        internal ServiceProxyBase SetManualProxy( Type interfaceType, ServiceProxyBase proxy )
        {
            ServiceProxyBase current;
            if( _proxies.TryGetValue( interfaceType, out current ) )
            {
                _proxies[interfaceType] = proxy;
                proxy.SetPluginImplementation( current.Implementation );
            }
            else
            {
                _proxies.Add( interfaceType, proxy );
            }
            proxy.Initialize( this, false );
            ApplyConfiguration( proxy );
            return current;
        }

        public void ApplyConfiguration()
        {
            foreach( ServiceProxyBase proxy in _proxies.Values )
            {
                ApplyConfiguration( proxy );
            }
        }

        private void ApplyConfiguration( ServiceProxyBase proxy )
        {
            for( int i = 0; i < proxy.MethodEntries.Length; ++i )
            {
                ServiceLogMethodOptions o = ServiceLogMethodOptions.None;
                foreach( IServiceHostConfiguration cfg in _configurations )
                {
                    o |= cfg.GetOptions( proxy.MethodEntries[i].Method );
                }
                proxy.MethodEntries[i].LogOptions = o;
            }
            for( int i = 0; i < proxy.EventEntries.Length; ++i )
            {
                ServiceLogEventOptions o = ServiceLogEventOptions.None;
                foreach( IServiceHostConfiguration cfg in _configurations )
                {
                    o |= cfg.GetOptions( proxy.EventEntries[i].Event );
                }
                proxy.EventEntries[i].LogOptions = o;
            }
        }

        /// <summary>
        /// Called when a method is entered.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="logOptions"></param>
        /// <returns></returns>
        internal LogMethodEntry LogMethodEnter( MethodInfo m, ServiceLogMethodOptions logOptions )
        {
            Debug.Assert( logOptions != 0 );
            LogMethodEntry me = new LogMethodEntry();
            if( (logOptions & ServiceLogMethodOptions.Leave) == 0 )
            {
                me.InitClose( ++_nextLSN, _currentDepth, m );
                // Emits the "Created" event.
                EventHandler<LogEventArgs> h = EventCreated;
                if( h != null ) h( _eventSender, me );
            }
            else
            {
                me.InitOpen( ++_nextLSN, _currentDepth++, m );
                // Emits the "Creating" event.
                EventHandler<LogEventArgs> h = EventCreating;
                if( h != null ) h( _eventSender, me );
            }
            return me;
        }

        /// <summary>
        /// Called whenever an exception occured in a logged method.
        /// The existing entry may be closed or opened. If it is opened, we first
        /// send the EventCreated event for the error entry before sending 
        /// the EventCreated event for the method itself.
        /// We privilegiate here a hierarchical view: the error will be received before the end of the method.
        /// </summary>
        /// <param name="me">Existing entry.</param>
        /// <param name="ex">Exception raised.</param>
        internal void LogMethodError( LogMethodEntry me, Exception ex )
        {
            LogMethodEntryError l = new LogMethodEntryError( ++_nextLSN, me, ex );
            EventHandler<LogEventArgs> h = EventCreated;
            if( me.SetError( l ) )
            {
                // Entry was opened.
                --_currentDepth;
                if( h != null )
                {
                    // We first send the "Created" event for the error entry.
                    h( _eventSender, l );
                    // We then send the "Created" event for the method entry.
                    h( _eventSender, me );
                }
                else _untrackedErrors.Add( l );
            }
            else
            {
                // Entry is already closed: just send the error entry.
                if( h != null ) h( _eventSender, l );
                else _untrackedErrors.Add( l );
            }
            Debug.Assert( !me.IsCreating, "SetError closed the event, whatever its status was." );
        }

        /// <summary>
        /// Called whenever an exception occured in a non logged method.
        /// </summary>
        /// <param name="m">The culprit method.</param>
        /// <param name="ex">The exception raised.</param>
        internal void LogMethodError( MethodInfo m, Exception ex )
        {
            LogMethodError l = new LogMethodError( ++_nextLSN, _currentDepth, m, ex );
            // Send the "Created" event for the error entry.
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, l );
            else _untrackedErrors.Add( l );
        }

        /// <summary>
        /// Called when a method with an opened entry succeeds.
        /// </summary>
        /// <param name="me"></param>
        internal void LogMethodSuccess( LogMethodEntry me )
        {
            Debug.Assert( me.IsCreating );
            --_currentDepth;
            me.Close();
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, me );
        }

        internal LogEventEntry LogEventEnter( EventInfo e, ServiceLogEventOptions logOptions )
        {
            Debug.Assert( logOptions != 0 );
            LogEventEntry ee = new LogEventEntry();
            if( (logOptions & ServiceLogEventOptions.EndRaise) == 0 )
            {
                ee.InitClose( ++_nextLSN, _currentDepth, e );
                // Emits the "Created" event.
                EventHandler<LogEventArgs> h = EventCreated;
                if( h != null ) h( _eventSender, ee );
            }
            else //if( (logOptions & ServiceLogEventOptions.StartRaise) != 0) //if we are only logging the EndRaise, we should NOT log through LogEventEnter
            {
                ee.InitOpen( ++_nextLSN, _currentDepth++, e );
                // Emits the "Creating" event.
                EventHandler<LogEventArgs> h = EventCreating;
                if( h != null ) h( _eventSender, ee );
            }
            return ee;
        }

        /// <summary>
        /// Called at the end of an event raising that have an existing opened log entry.
        /// Entries that have been created by <see cref="LogEventError(EventInfo,MethodInfo,Exception)"/> (because an exception 
        /// has been raised by at least one receiver) are not tracked.
        /// </summary>
        /// <param name="ee">The entry of the event that ended.</param>
        internal void LogEventEnd( LogEventEntry ee )
        {
            Debug.Assert( ee.IsCreating );
            --_currentDepth;
            ee.Close();
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, ee );
        }

        /// <summary>
        /// Called whenever the recipient of an event raises an exception and the event log already exists. 
        /// This appends the error to the error list of the event entry.
        /// </summary>
        /// <param name="ee">Existing event log entry.</param>
        /// <param name="target">Culprit method.</param>
        /// <param name="ex">Exception raised by the culprit method.</param>
        internal void LogEventError( LogEventEntry ee, MethodInfo target, Exception ex )
        {
            LogEventEntryError l = new LogEventEntryError( ++_nextLSN, ee, target, ex );
            ee.AddError( l );
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, l );
            else _untrackedErrors.Add( l );
        }

        /// <summary>
        /// Called whenever the recipient of an event raises an exception and the event is not 
        /// yet logged (no <see cref="LogEventEntry"/> exists). This creates the entry for the event 
        /// and the associated error.
        /// </summary>
        /// <param name="e">The reflected event info.</param>
        /// <param name="target">Culprit method.</param>
        /// <param name="ex">Exception raised by the culprit method.</param>
        /// <returns>The created event entry that holds the error.</returns>
        internal LogEventEntry LogEventError( EventInfo e, MethodInfo target, Exception ex )
        {
            // This LogEventEntry is an hidden one. We do not emit it.
            LogEventEntry ee = new LogEventEntry();
            LogEventEntryError l = new LogEventEntryError( ++_nextLSN, ee, target, ex );
            ee.InitError( ++_nextLSN, _currentDepth, e, l );

            // Emits the error.
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, l );
            else _untrackedErrors.Add( l );

            return ee;
        }

        /// <summary>
        /// Called when an event is raised by a stopped service and both <see cref="ServiceLogEventOptions.LogSilentEventRunningStatusError" /> 
        /// and <see cref="ServiceLogEventOptions.SilentEventRunningStatusError"/> are set.
        /// </summary>
        internal void LogEventNotRunningError( EventInfo eventInfo, bool serviceIsDisabled )
        {
            LogEventNotRunningError l = new LogEventNotRunningError( ++_nextLSN, _currentDepth, eventInfo, serviceIsDisabled );
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, l );
        }

        public void ExternalLog( string message, object extraData )
        {
            LogExternalEntry e = new LogExternalEntry( _nextLSN++, _currentDepth, message ?? String.Empty, extraData );
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, e );
        }

        public void ExternalLogError( Exception ex, MemberInfo optionalExplicitCulprit, string message, object extraData )
        {
            if( message == null ) message = String.Empty;
            if( ex == null )
            {
                ex = new ArgumentNullException();
                message = R.ExternalLogErrorMissException + message;
            }
            LogExternalErrorEntry e = new LogExternalErrorEntry( _nextLSN++, _currentDepth, ex, optionalExplicitCulprit, message, extraData );
            EventHandler<LogEventArgs> h = EventCreated;
            if( h != null ) h( _eventSender, e );
            else _untrackedErrors.Add( e );
        }

        #region IServiceHost Members

        object IServiceHost.InjectExternalService( Type interfaceType, object currentImplementation )
        {
            return EnsureProxyForExternalService( interfaceType, currentImplementation );
        }

        object IServiceHost.GetProxy( Type interfaceType )
        {
            ServiceProxyBase proxy;
            if( _proxies.TryGetValue( interfaceType, out proxy ) 
                && (!interfaceType.IsGenericType && proxy.Status == RunningStatus.Disabled) )
            {
                proxy = null;
            }
            return proxy;
        }

        object IServiceHost.GetRunningProxy( Type interfaceType )
        {
            ServiceProxyBase proxy;
            if( _proxies.TryGetValue( interfaceType, out proxy ) && proxy.Status <= RunningStatus.Stopped )
            {
                proxy = null;
            }
            return proxy;
        }

        #endregion

    }
}

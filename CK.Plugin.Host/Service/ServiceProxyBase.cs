#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\ServiceProxyBase.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Plugin;
using System.Reflection;
using System.Linq;
using CK.Core;

namespace CK.Plugin.Hosting
{
    internal struct MEntry
    {
        public MethodInfo Method;
        public ServiceLogMethodOptions LogOptions;
    }

    internal struct EEntry
    {
        public EventInfo Event;
        public ServiceLogEventOptions LogOptions;
    }
    
    internal abstract class ServiceProxyBase
	{
        readonly Type _typeInterface;
        readonly MEntry[] _mRefs;
        readonly EEntry[] _eRefs;
        PluginProxyBase _impl;
        object _unavailableImpl;
        ServiceHost _serviceHost;
        RunningStatus _status;
        bool _isExternalService;

		protected ServiceProxyBase( object unavailableImpl, Type typeInterface, IList<MethodInfo> mRefs, IList<EventInfo> eRefs )
		{
            Debug.Assert( mRefs.All( r => r != null ) && mRefs.Distinct().SequenceEqual( mRefs ) );
            _typeInterface = typeInterface;
            _status = RunningStatus.Disabled;
            RawImpl = _unavailableImpl = unavailableImpl;
            _mRefs = new MEntry[mRefs.Count];
            for( int i = 0; i < mRefs.Count; i++ )
            {
                _mRefs[i].Method = mRefs[i];
            }
            _eRefs = new EEntry[eRefs.Count];
            for( int i = 0; i < eRefs.Count; i++ )
            {
                _eRefs[i].Event = eRefs[i];
            }
        }

        internal void Initialize( ServiceHost serviceHost, bool isExternalService )
        {
            _serviceHost = serviceHost;
            _isExternalService = isExternalService;
            if( isExternalService ) _status = RunningStatus.Started;
        }

        internal MEntry[] MethodEntries
        {
            get { return _mRefs; }
        }
        
        internal EEntry[] EventEntries
        {
            get { return _eRefs; }
        }

        internal void SetStatusChanged( RunningStatus newOne )
        {
            SetStatusChanged( newOne, false );
        }

        internal void SetStatusChanged( RunningStatus newOne, bool allowErrorTransition )
		{
            Debug.Assert( _status.IsValidTransition( newOne, allowErrorTransition ) );
            RunningStatus previous = _status;
            _status = newOne;
            ConfigureRawImplFromPlugin();
            var h = ServiceStatusChanged;
            if( h != null )
			{
                ServiceStatusChangedEventArgs ev = new ServiceStatusChangedEventArgs( previous, _status, allowErrorTransition );
				h( this, ev );
			}
		}

		public event EventHandler<ServiceStatusChangedEventArgs> ServiceStatusChanged;

		public RunningStatus Status
		{
            get { return _status; }
		}

		protected abstract object RawImpl { get; set; }

        internal PluginProxyBase Implementation { get { return _impl; } }

        /// <summary>
        /// Currently, injection of external services must be totally independant of
        /// any Dynamic services: a Service is either a dynamic one, implemented by one (or more) plugin, 
        /// or an external one that is considered to be persistent and available.
        /// </summary>
        /// <param name="implementation"></param>
        public void SetExternalImplementation( object implementation )
        {
            if( !_isExternalService ) throw new CKException( R.ServiceIsPluginBased, _typeInterface );
            if( implementation == null )
            {
                RawImpl = _unavailableImpl;
            }
            else
            {
                RawImpl = implementation;
            }
        }

        public void SetPluginImplementation( PluginProxyBase implementation )
        {
            if( _isExternalService ) throw new CKException( R.ServiceIsAlreadyExternal, _typeInterface, implementation.GetType().AssemblyQualifiedName ); 
            _impl = implementation;
            Debug.Assert( _impl == null || (_impl.RealPlugin != null || _impl.Status == RunningStatus.Disabled), "Plugin.RealPlugin == null ==> Plugin.Status == Disabled" );
            ConfigureRawImplFromPlugin();
        }

        void ConfigureRawImplFromPlugin()
        {
            if( _impl == null || _status == RunningStatus.Disabled )
            {
                RawImpl = _unavailableImpl;
            }
            else RawImpl = _impl.RealPlugin;
        }

        #region Protected methods called by concrete concrete dynamic classes (event relaying).

        /// <summary>
        /// This method is called whenever a method not marked with <see cref="IgnoreServiceStoppedAttribute"/>
        /// is called. It throws a <see cref="ServiceStoppedException"/> if the service is stopped or disabled otherwise
        /// it returns the appropriate log configuration.
        /// </summary>
        /// <returns>The log configuration that must be used.</returns>
        [DebuggerNonUserCodeAttribute]
        protected ServiceLogMethodOptions GetLoggerForRunningCall( int iMethodMRef, out LogMethodEntry logger )
        {
            if( _impl == null || _impl.Status == RunningStatus.Disabled )
            {
                throw new ServiceNotAvailableException( _typeInterface );
            }
            if( _impl.Status == RunningStatus.Stopped )
            {
                throw new ServiceStoppedException( _typeInterface );
            }
            MEntry me = _mRefs[iMethodMRef];
            ServiceLogMethodOptions o = me.LogOptions;
            o &= ServiceLogMethodOptions.CreateEntryMask;
            logger = o == ServiceLogMethodOptions.None ? null : _serviceHost.LogMethodEnter( me.Method, o );
            return o;
        }

        /// <summary>
        /// Returns the appropriate log configuration after having checked that the dynamic service is not disabled.
        /// </summary>
        /// <returns>The log configuration that must be used.</returns>
        [DebuggerNonUserCodeAttribute]
        protected ServiceLogMethodOptions GetLoggerForNotDisabledCall( int iMethodMRef, out LogMethodEntry logger )
        {
            if( _impl == null || _impl.Status == RunningStatus.Disabled )
            {
                throw new ServiceNotAvailableException( _typeInterface );
            }
            MEntry me = _mRefs[iMethodMRef];
            ServiceLogMethodOptions o = me.LogOptions;
            o &= ServiceLogMethodOptions.CreateEntryMask;
            logger = o == ServiceLogMethodOptions.None ? null : _serviceHost.LogMethodEnter( me.Method, o );
            return o;
        }

        /// <summary>
        /// Returns the appropriate log configuration without any runtime status checks.
        /// </summary>
        /// <returns>The log configuration that must be used.</returns>
        [DebuggerNonUserCodeAttribute]
        protected ServiceLogMethodOptions GetLoggerForAnyCall( int iMethodMRef, out LogMethodEntry logger )
        {
            MEntry me = _mRefs[iMethodMRef];
            ServiceLogMethodOptions o = me.LogOptions;
            logger = o == ServiceLogMethodOptions.None ? null : _serviceHost.LogMethodEnter( me.Method, o );
            return o;
        }

        [DebuggerNonUserCodeAttribute]
        protected void LogEndCall( LogMethodEntry e )
        {
            Debug.Assert( e != null );
            _serviceHost.LogMethodSuccess( e );
        }

        [DebuggerNonUserCodeAttribute]
        protected void LogEndCallWithValue( LogMethodEntry e, object retValue )
        {
            Debug.Assert( e != null );
            e._returnValue = retValue;
            _serviceHost.LogMethodSuccess( e );
        }

        [DebuggerNonUserCodeAttribute]
        protected void OnCallException( int iMethodMRef, Exception ex, LogMethodEntry e )
        {
            if( e != null )
            {
                _serviceHost.LogMethodError( e, ex );
            }
            else
            {
                MEntry me = _mRefs[iMethodMRef];
                _serviceHost.LogMethodError( me.Method, ex );
            }
        }

        /// <summary>
        /// This method is called whenever an event not marked with <see cref="IgnoreServiceStoppedAttribute"/>
        /// is raised. If the service is actually running, it does nothing and returns true.
        /// If the service is stopped or disabled it throws a <see cref="ServiceStoppedException"/> or returns false
        /// if <see cref="ServiceLogEventOptions.SilentEventRunningStatusError"/> is set: the event will not be raised and no exceptions will be
        /// thrown back to the buggy service.
        /// </summary>
        [DebuggerNonUserCodeAttribute]
        protected bool GetLoggerEventForRunningCall( int iEventMRef, out LogEventEntry entry, out ServiceLogEventOptions logOptions )
        {
            EEntry e = _eRefs[iEventMRef];
            logOptions = e.LogOptions;
            bool isDisabled = _impl == null || _impl.Status == RunningStatus.Disabled;
            if( isDisabled || _impl.Status == RunningStatus.Stopped )
            {
                if( (logOptions & ServiceLogEventOptions.SilentEventRunningStatusError) != 0 )
                {
                    entry = null;
                    if( (logOptions & ServiceLogEventOptions.LogSilentEventRunningStatusError) != 0 )
                        _serviceHost.LogEventNotRunningError( e.Event, isDisabled );
                    return false;
                }
                if( isDisabled ) throw new ServiceNotAvailableException( _typeInterface );
                else throw new ServiceStoppedException( _typeInterface );
            }
            logOptions &= ServiceLogEventOptions.CreateEntryMask;
            entry = logOptions != 0 ? _serviceHost.LogEventEnter( e.Event, logOptions ) : null;   
            return true;
        }

        [DebuggerNonUserCodeAttribute]
        protected bool GetLoggerEventForNotDisabledCall( int iEventMRef, out LogEventEntry entry, out ServiceLogEventOptions logOptions )
        {
            EEntry e = _eRefs[iEventMRef];
            logOptions = e.LogOptions & ServiceLogEventOptions.CreateEntryMask;
            if( _impl == null || _impl.Status == RunningStatus.Disabled )
            {
                if( (logOptions & ServiceLogEventOptions.SilentEventRunningStatusError) != 0 )
                {
                    entry = null;
                    if( (logOptions & ServiceLogEventOptions.LogSilentEventRunningStatusError) != 0 )
                        _serviceHost.LogEventNotRunningError( e.Event, true );
                    return false;
                }
                throw new ServiceNotAvailableException( _typeInterface );
            }
            entry = logOptions != 0 ? _serviceHost.LogEventEnter( e.Event, logOptions ) : null;
            return true;
        }

        [DebuggerNonUserCodeAttribute]
        protected bool GetLoggerEventForAnyCall( int iEventMRef, out LogEventEntry entry, out ServiceLogEventOptions logOptions )
        {
            EEntry e = _eRefs[iEventMRef];
            logOptions = e.LogOptions & ServiceLogEventOptions.CreateEntryMask;
            entry = logOptions != 0 ? _serviceHost.LogEventEnter( e.Event, logOptions ) : null;
            return true;
        }

        [DebuggerNonUserCodeAttribute]
        protected void LogEndRaise( LogEventEntry e )
        {
            Debug.Assert( e != null );
            _serviceHost.LogEventEnd( e );
        }

        /// <summary>
        /// This method is called when an event subscriber raises an exception while receiving the notification.
        /// By returning true, this methods silently swallow the exception. By returning false, the event dispatching
        /// is stoppped (remaining subscribers will not receive the event) and the plugin receives the exception (this
        /// corresponds to the standard behavior).
        /// </summary>
        /// <param name="iEventMRef">The index of the event info.</param>
        /// <param name="target">The called method that raised the exception.</param>
        /// <param name="ex">The exception.</param>
        /// <param name="ee">The log entry if it has been created. Will be created if needed.</param>
        /// <returns>True to silently swallow the exception.</returns>
        [DebuggerNonUserCodeAttribute]
        protected bool OnEventHandlingException( int iEventMRef, MethodInfo target, Exception ex, ref LogEventEntry ee )
		{
            EEntry e = _eRefs[iEventMRef];
            if( (e.LogOptions & ServiceLogEventOptions.LogErrors) != 0 )
            {
                if( ee != null )
                {
                    _serviceHost.LogEventError( ee, target, ex );
                }
                else
                {
                    ee = _serviceHost.LogEventError( e.Event, target, ex );
                }
            }
            return (e.LogOptions&ServiceLogEventOptions.SilentEventError) != 0;
        }

        #endregion
    }

}

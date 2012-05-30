#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\IChoucrouteServiceHandMadeProxy.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CK.Plugin.Hosting;

namespace CK.Plugin.Host.Tests
{
	internal class IChoucrouteServiceHandMadeProxy : ServiceProxyBase, IService<IChoucrouteService>, IChoucrouteService
	{
		IChoucrouteService _impl;

        static MethodInfo _firstGeneric;
        static MethodInfo _secondGeneric;

        static IChoucrouteServiceHandMadeProxy()
        {
            var methods = from m in typeof( IChoucrouteService ).GetMethods() 
                where m.Name == "GenericFunc"
                select m;
            _firstGeneric = methods.First();
            _secondGeneric = methods.Skip( 1 ).First();
        }

        static public IChoucrouteServiceHandMadeProxy CreateProxy()
        {
            IChoucrouteServiceHandMadeProxy p = new IChoucrouteServiceHandMadeProxy( new ChoucrouteServiceNotAvailable_UN() );
            return p;
        }

		private IChoucrouteServiceHandMadeProxy( object unavailableImpl )
			: base(
            unavailableImpl, 
            typeof(IChoucrouteService),
            new MethodInfo[] 
            { 
                typeof( IChoucrouteServiceBase ).GetMethod( "CallFunc" ),               // 0
                typeof( IChoucrouteServiceBase ).GetMethod( "Div", new[]{typeof(int), typeof(int)} ),                   // 1
                typeof( IChoucrouteServiceBase ).GetMethod( "Div", new[]{typeof(int), typeof(int), typeof(int)} ),      // 2
                typeof( IChoucrouteServiceBase ).GetProperty( "ID" ).GetGetMethod(),               // 3
                typeof( IChoucrouteServiceBase ).GetProperty( "ID" ).GetSetMethod(),               // 4
                typeof( IChoucrouteServiceBase ).GetProperty( "Creation" ).GetGetMethod(),         // 5
                typeof( IChoucrouteServiceBase ).GetProperty( "Item", new[]{typeof(int)} ).GetGetMethod(),         // 6
                typeof( IChoucrouteService ).GetProperty( "Item", new[]{typeof(int),typeof(DateTime),typeof(IDynamicService),typeof(string),typeof(object),typeof(double),typeof(byte)} ).GetGetMethod(), // 7
                typeof( IChoucrouteService ).GetProperty( "Item", new[]{typeof(int),typeof(DateTime),typeof(IDynamicService),typeof(string),typeof(object),typeof(double),typeof(byte)} ).GetSetMethod(), // 8
                _firstGeneric, //9
                _secondGeneric, //10
                typeof( IChoucrouteService ).GetMethod( "RaiseAnEventGen" ) // 11
           },
           new EventInfo[]
           { 
                typeof( IChoucrouteService ).GetEvent( "AnEventGen" ),              // 0
                typeof( IChoucrouteService ).GetEvent( "AnEvent" )                  // 1
           } 

            )
		{
            _impl = (IChoucrouteService)unavailableImpl;
		}

		public IChoucrouteService Service
		{
			get { return this; }
		}

		protected override object RawImpl
		{
			get { return _impl; }
			set { _impl = (IChoucrouteService)value; }
		}

		public void CallFunc()
		{
            LogMethodEntry e;
            ServiceLogMethodOptions o = GetLoggerForRunningCall( 0, out e );
            try
			{
				_impl.CallFunc();
                if( (o & ServiceLogMethodOptions.Leave) != 0 ) LogEndCall( e );
			}
			catch( Exception ex )
			{
                if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 0, ex, e );
                throw;
			}
		}

		public int Div( int i, int j )
		{
			// This one has IgnoreServiceRunningStatus attribute.
            LogMethodEntry e;
            ServiceLogMethodOptions o = GetLoggerForNotDisabledCall( 1, out e );
            if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { i, j };
            try
			{
				return StandardHandleLogReturn( o, e, _impl.Div( i, j ) );
			}
			catch( Exception ex )
			{
                if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 1, ex, e );
                throw;
			}
		}

		public int Div( int i, int j, int k )
		{
            LogMethodEntry e;
            ServiceLogMethodOptions o = GetLoggerForRunningCall( 2, out e );
            if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { i, j, k };
            try
			{
				return StandardHandleLogReturn( o, e, _impl.Div( i, j, k ) );
			}
			catch( Exception ex )
			{
                if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 2, ex, e );
                throw;
			}
		}

		public Guid ID
		{
			get
			{
                LogMethodEntry e;
                ServiceLogMethodOptions o = GetLoggerForRunningCall( 3, out e );
				try
				{
                    return StandardHandleLogReturn( o, e, _impl.ID );
				}
				catch( Exception ex )
				{
                    if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 3, ex, e );
                    throw;
				}
			}
			set
			{
				// This one has IgnoreServiceRunningStatus attribute on its setter.
                LogMethodEntry e;
                ServiceLogMethodOptions o = GetLoggerForNotDisabledCall( 4, out e );
                if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { value };
                try
				{
                    _impl.ID = value;
                    if( (o&ServiceLogMethodOptions.Leave) != 0 ) LogEndCall( e );
				}
				catch( Exception ex )
				{
                    if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 4, ex, e );
                    throw;
				}
			}
		}

        T StandardHandleLogReturn<T>( ServiceLogMethodOptions o, LogMethodEntry e, T retValue )
        {
            if( (o&ServiceLogMethodOptions.Leave) != 0 )
            {
                if( (o & ServiceLogMethodOptions.LogReturnValue) != 0 )
                {
                    LogEndCallWithValue( e, retValue );
                }
                else
                {
                    LogEndCall( e );
                }
            }
            return retValue;
        }

		public DateTime Creation
		{
			get
			{
				// This one has IgnoreServiceRunningStatus attribute.
                LogMethodEntry e;
                ServiceLogMethodOptions o = GetLoggerForNotDisabledCall( 5, out e );
                try
				{
                    // Inlined code to handle return of the call.
                    // IL generated code is more efficient since
                    // it uses the local variable (copy) only 
                    // when LogEndCallWithValue must be called.
                    if( (o & ServiceLogMethodOptions.Leave) != 0 )
                    {
                        if( (o & ServiceLogMethodOptions.LogReturnValue) != 0 )
                        {
                            DateTime ret = _impl.Creation;
                            LogEndCallWithValue( e, ret );
                            return ret;
                        }
                        else
                        {
                            DateTime ret = _impl.Creation;
                            LogEndCall( e );
                            return ret;
                        }
                    }
                    return _impl.Creation;
				}
				catch( Exception ex )
				{
                    if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 5, ex, e );
                    throw;
				}
			}
		}

		public object this[int i]
		{
			get
			{
                LogMethodEntry e;
                ServiceLogMethodOptions o = GetLoggerForRunningCall( 6, out e );
                if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { i };
                try
				{
                    return StandardHandleLogReturn( o, e, _impl[i] );
				}
				catch( Exception ex )
				{
                    if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 6, ex, e );
                    throw;
				}
			}
		}

		public string this[int i, DateTime d, IDynamicService s, string t, object ob, double f, byte b]
		{
			get
			{
                LogMethodEntry e;
                ServiceLogMethodOptions o = GetLoggerForRunningCall( 7, out e );
                if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[]{ i, d, s, t, ob, f, b };
				try
				{
                    return StandardHandleLogReturn( o, e, _impl[i, d, s, t, ob, f, b] );
				}
				catch( Exception ex )
				{
                    if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 7, ex, e );
                    throw;
				}
			}
			set
			{
                LogMethodEntry e;
                ServiceLogMethodOptions o = GetLoggerForRunningCall( 8, out e );
                if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { i, d, s, t, o, f, b };
                try
				{
					_impl[i, d, s, t, o, f, b] = value;
                    if( (o & ServiceLogMethodOptions.Leave) != 0 ) LogEndCall( e );
				}
				catch( Exception ex )
				{
                    if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 8, ex, e );
                    throw;
				}
			}
		}


		public void GenericFunc<T>( T a )
		{
            LogMethodEntry e;
            ServiceLogMethodOptions o = GetLoggerForRunningCall( 9, out e );
            if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { a };
            try
			{
				_impl.GenericFunc( a );
                if( (o & ServiceLogMethodOptions.Leave) != 0 ) LogEndCall( e );
            }
			catch( Exception ex )
			{
                if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 9, ex, e );
                throw;
			}
		}

		public U GenericFunc<T, U, V>( T a, U b, V c ) 
            where U : struct 
            where V : class
		{
            LogMethodEntry e;
            ServiceLogMethodOptions o = GetLoggerForRunningCall( 10, out e );
            if( (o & ServiceLogMethodOptions.LogParameters) != 0 ) e._parameters = new object[] { a, b, c };
			try
			{
                return StandardHandleLogReturn( o, e, _impl.GenericFunc( a, b, c ) );
			}
			catch( Exception ex )
			{
                if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 10, ex, e );
                throw;
			}
		}

		#region event EventHandler<SpecEventArgs> AnEventGen
		Delegate _dAnEventGen;
		EventHandler<SpecEventArgs> _hookAnEventGen;

		public event EventHandler<SpecEventArgs> AnEventGen
		{
			add
			{
				if( _dAnEventGen == null )
				{
					if( _hookAnEventGen == null ) _hookAnEventGen = new EventHandler<SpecEventArgs>( _realService_AnEventGen );
					_impl.AnEventGen += _hookAnEventGen;
				}
				_dAnEventGen = Delegate.Combine( _dAnEventGen, value );
			}
			remove
			{
				_dAnEventGen = Delegate.Remove( _dAnEventGen, value );
				if( _dAnEventGen == null )
				{
					_impl.AnEventGen -= _hookAnEventGen;
				}
			}
		}

		void _realService_AnEventGen( object sender, SpecEventArgs args )
		{
			Debug.Assert( sender == _impl );
			Debug.Assert( _dAnEventGen != null, "Since we register only when needed and unregister when no more clients exist." );

			// AnEvent is not IgnoreServiceRunningStatus.
            LogEventEntry e;
            ServiceLogEventOptions o;
            if( GetLoggerEventForRunningCall( 0, out e, out o ) )
            {
                // Implementation maps to the Proxy.
                if( sender == _impl ) sender = this;

                // Fires the event
                foreach( Delegate d in _dAnEventGen.GetInvocationList() )
                {
                    try
                    {
                        ((EventHandler<SpecEventArgs>)d)( this, args );
                    }
                    catch( Exception ex )
                    {
                        if( !OnEventHandlingException( 0, d.Method, ex, ref e ) ) throw;
                    }
                }
                if( (o & ServiceLogEventOptions.EndRaise) != 0 ) LogEndRaise( e );
            }
		}

		public void RaiseAnEventGen()
		{
            LogMethodEntry e;
            ServiceLogMethodOptions o = GetLoggerForRunningCall( 11, out e );
            try
            {
                _impl.RaiseAnEventGen();
                if( (o & ServiceLogMethodOptions.Leave) != 0 ) LogEndCall( e );
            }
            catch( Exception ex )
            {
                if( (o & ServiceLogMethodOptions.LogError) != 0 ) OnCallException( 11, ex, e );
                throw;
            }
		}

		#endregion

		#region event EventHandler AnyMethodCalled

		Delegate _dAnyMethodCalled;
		EventHandler _hookAnyMethodCalled;

		public event EventHandler AnyMethodCalled
		{
			add 
			{
				if( _dAnyMethodCalled == null )
				{
					if( _hookAnyMethodCalled == null ) _hookAnyMethodCalled = new EventHandler( _realService_AnyMethodCalled );
					_impl.AnyMethodCalled += _hookAnyMethodCalled;
				}
				_dAnyMethodCalled = Delegate.Combine( _dAnyMethodCalled, value ); 
			}
			remove 
			{
				_dAnyMethodCalled = Delegate.Remove( _dAnyMethodCalled, value );
				if( _dAnyMethodCalled == null )
				{
					_impl.AnyMethodCalled -= _hookAnyMethodCalled;
				}
			}
		}

		void _realService_AnyMethodCalled( object sender, EventArgs e )
		{
			Debug.Assert( sender == _impl );
			Debug.Assert( _dAnyMethodCalled != null, "Since we register only when needed and unregister when no more clients exist." );

			// Don't call GetLoggerForRunningCall since AnyMethodCalled is IgnoreServiceRunningStatus.
			((EventHandler)_dAnyMethodCalled)( sender == _impl ? this : sender, e );
		}

		#endregion

		#region event AnEventHandler AnEvent

		Delegate _dAnEvent;
		AnEventHandler _hookAnEvent;

		public event AnEventHandler AnEvent
		{
			add 
			{
				if( _dAnEvent == null )
				{
					if( _hookAnEvent == null ) _hookAnEvent = new AnEventHandler( _realService_AnEvent );
					_impl.AnEvent += _hookAnEvent;
				}
				_dAnEvent = Delegate.Combine( _dAnEvent, value ); 
			}
			remove 
			{
				_dAnEvent = Delegate.Remove( _dAnEvent, value );
				if( _dAnEvent == null )
				{
					_impl.AnEvent -= _hookAnEvent;
				}
			}
		}

		void _realService_AnEvent( int i, bool b, string s, object sender )
		{
			Debug.Assert( sender == _impl );
			Debug.Assert( _dAnEvent != null, "Since we register only when needed and unregister when no more clients exist." );
			
			// AnEvent is not IgnoreServiceRunningStatus.
            ServiceLogEventOptions o;
            LogEventEntry e;
            if( GetLoggerEventForRunningCall( 1, out e, out o ) )
            {
                // Implementation maps to the Proxy.
                if( sender == _impl ) sender = this;

                // Fires the event
                foreach( Delegate d in _dAnEvent.GetInvocationList() )
                {
                    try
                    {
                        ((AnEventHandler)d)( i, b, s, sender );
                    }
                    catch( Exception ex )
                    {
                        if( !OnEventHandlingException( 1, d.Method, ex, ref e ) ) throw;
                    }
                }
                if( (o & ServiceLogEventOptions.EndRaise) != 0 ) LogEndRaise( e );
            }
		}

		public void RaiseAnEvent()
		{
            // Always fires AnEvent since tagged with [IgnoreServiceRunningStatus]...
            // the implementation MUST raise the event BUT this must raise an exception
            // since the AnEvent event itself is NOT [IgnoreServiceRunningStatus].
			_impl.RaiseAnEvent();
		}
		#endregion


	}
}

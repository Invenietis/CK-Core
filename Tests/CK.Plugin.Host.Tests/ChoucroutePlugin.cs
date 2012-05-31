#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\ChoucroutePlugin.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Diagnostics;
using CK.Plugin;

namespace CK.Plugin.Host.Tests
{
	public class ChoucroutePlugin : IPlugin, IChoucrouteService
	{

        #region IPlugin Members

        bool IPlugin.Setup( IPluginSetupInfo info )
        {
            return true;
        }

        void IPlugin.Start()
        {
        }

        void IPlugin.Teardown()
        {
        }

        void IPlugin.Stop()
        {
        }

        #endregion

		public void CallFunc()
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
		}

		public int Div( int i, int j )
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
			return i / j;
		}

		public int Div( int i, int j, int k )
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
			return i / j / k;
		}

		Guid _guid = Guid.NewGuid();

		public Guid ID
		{
			get
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				return _guid;
			}
			set
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				_guid = value;
			}
		}

		public DateTime Creation
		{
			get
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				return DateTime.Now;
			}
		}

		public object this[int i]
		{
			get
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				return i == 0 ? DBNull.Value : null;
			}
		}

		public string this[int i, DateTime d, IDynamicService s, string t, object o, double f, byte b]
		{
			get
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				return i.ToString() + d.Ticks.ToString() + t + o.ToString() +f.ToString() + b.ToString();
			}
			set
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
			}
		}

		public void GenericFunc<T>( T a )
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
		}

		public U GenericFunc<T, U, V>( T a, U b, V c ) 
            where U : struct 
            where V : class
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
			return b;
		}

		EventHandler _anyMethodCalled;

		// RegisterCall fires this event each time a method is called.
		public event EventHandler AnyMethodCalled
		{
			add 
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				_anyMethodCalled = (EventHandler)Delegate.Combine( _anyMethodCalled, value ); 
			}
			remove 
			{
				// RegisterCall after removing the handler (for the test) 
				// to avoid firing the event while removing it.
				_anyMethodCalled = (EventHandler)Delegate.Remove( _anyMethodCalled, value ); 
				RegisterCall( MethodInfo.GetCurrentMethod() );
			}
		}


		EventHandler<SpecEventArgs> _anEventGen;

		// RegisterCall fires this event each time a method is called.
		public event EventHandler<SpecEventArgs> AnEventGen
		{
			add 
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				_anEventGen = (EventHandler<SpecEventArgs>)Delegate.Combine( _anEventGen, value ); 
			}
			remove 
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				_anEventGen = (EventHandler<SpecEventArgs>)Delegate.Remove( _anEventGen, value ); 
			}
		}

		public void RaiseAnEventGen()
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
			if( _anEventGen != null ) _anEventGen( this, null );
		}

		AnEventHandler _anEvent;

		// RegisterCall fires this event each time a method is called.
		public event AnEventHandler AnEvent
		{
			add 
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				_anEvent = (AnEventHandler)Delegate.Combine( _anEvent, value ); 
			}
			remove 
			{
				RegisterCall( MethodInfo.GetCurrentMethod() );
				_anEvent = (AnEventHandler)Delegate.Remove( _anEvent, value ); 
			}
		}

		public void RaiseAnEvent()
		{
			RegisterCall( MethodInfo.GetCurrentMethod() );
			if( _anEvent != null ) _anEvent( 1, true, "test", this );
		}

		public bool AllMethodsHaveBeenCalled()
		{
			foreach( string call in _allInterfaceSig )
			{
				if( !_hasBeenCalled.Contains( call ) ) return false;
			}
			return true;
		}

		public string[] MethodsNotCalled()
		{
			List<string> c = new List<string>();
			foreach( string call in _allInterfaceSig )
			{
				if( !_hasBeenCalled.Contains( call ) ) c.Add( call );
			}
			return c.ToArray();
		}

		public int CalledMethodsCount
		{
			get { return _hasBeenCalled.Count; }
		}

		#region Member call tracking implementation
		static List<String> _allInterfaceSig;
		List<String> _hasBeenCalled = new List<string>();

		static ChoucroutePlugin()
		{
			_allInterfaceSig = new List<string>();
			foreach( MethodInfo m in typeof( IChoucrouteService ).GetMethods() )
			{
				_allInterfaceSig.Add( GenerateCallSignature( m ) );
			}
			foreach( PropertyInfo p in typeof( IChoucrouteService ).GetProperties() )
			{
				foreach( MethodInfo m in p.GetAccessors() )
				{
					_allInterfaceSig.Add( GenerateCallSignature( m ) );
				}
			}
			foreach( EventInfo e in typeof( IChoucrouteService ).GetEvents() )
			{
				_allInterfaceSig.Add( GenerateCallSignature( e.GetAddMethod() ) );
				_allInterfaceSig.Add( GenerateCallSignature( e.GetRemoveMethod() ) );
			}
		}

		void RegisterCall( MethodBase b )
		{
			Debug.Assert( b is MethodInfo );
			_hasBeenCalled.Add( GenerateCallSignature( b as MethodInfo ) );
			if( _anyMethodCalled != null ) _anyMethodCalled( this, EventArgs.Empty );
		}

		static string GenerateCallSignature( MethodInfo m )
		{
			return m.ToString();
		}

		#endregion

    }
}

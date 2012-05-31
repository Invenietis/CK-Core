#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\TestEmit.cs) is part of CiviKey. 
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
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections;
using CK.Plugin;

namespace CK.Plugin.Host.Tests.TestEmits
{

	public interface IService
	{
		bool IsEnabled { get; }
	}

	public interface IProxyObject
	{
		void RaiseStopped( EventArgs e );
		object Implementation { get; set; }
	}

	public abstract class ProxyBase : IProxyObject
	{
		public abstract object Implementation { get; set; }

		void IProxyObject.RaiseStopped( EventArgs e )
		{
			Console.WriteLine( "Overriding RaiseStarted" );
		}

	}

	public class C : ProxyBase
	{

		public override object Implementation
		{
			get { return this; }
			set { }
		}

	}

	public struct StringInt
	{
		int _value;

		public static implicit operator StringInt( int v )
		{
			StringInt i = new StringInt();
			i._value = v;
			return i;
		}

		public static implicit operator int( StringInt v )
		{
			return v._value;
		}

		public static implicit operator StringInt( string v )
		{
			StringInt i = new StringInt();
			i._value = int.Parse( v );
			return i;
		}

		public override string ToString()
		{
			return _value.ToString();
		}
	}

	[TestFixture]
	public class TestEmits
	{

		class OTest
		{
			int _v;

			public int V
			{
				get { return _v; }
				set { _v = value; }
			}

			public override bool Equals( object obj )
			{
				if( obj is OTest ) return ((OTest)obj)._v == _v;
				return false;
			}
            public override int GetHashCode()
            {
                return _v.GetHashCode();
            }
		}

		[Test]
		public void TestStringInt()
		{
			StringInt i = "556";
			Assert.AreEqual( i+12, 568 );
			StringInt ia = i;
			Assert.AreEqual( ia, i );
			i = i+2;
			Assert.AreNotEqual( ia, i );
			--i;
			Assert.AreEqual( ia+1, i );
			//i = i - "1";
			
			RunningStatus s = RunningStatus.Stopping;

			Console.WriteLine( s.ToString() );

			s = RunningStatus.Starting;

			Console.WriteLine( s.ToString() );

		}

		[Test]
		public void TestOverride()
		{
			string name = "DefineMethodOverrideExample";
			AssemblyName asmName = new AssemblyName( name );
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly( asmName, AssemblyBuilderAccess.Run );
			ModuleBuilder mb = ab.DefineDynamicModule( name );

			TypeBuilder tb = mb.DefineType( "C", 
				TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, 
				typeof( ProxyBase ),
				new Type[]{ typeof(IService) } );

			// IsEnabled = _impl != null.
			MethodBuilder isEnabledGet = tb.DefineMethod(
				"get_IsEnabled",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				CallingConventions.HasThis,
				typeof( bool ),
				Type.EmptyTypes );
			{
				ILGenerator g = isEnabledGet.GetILGenerator();
				// return this != null;
				g.Emit( OpCodes.Ldarg_0 );
				g.Emit( OpCodes.Ldnull );
				g.Emit( OpCodes.Ceq );
				g.Emit( OpCodes.Ldc_I4_0 );
				g.Emit( OpCodes.Ceq );
				g.Emit( OpCodes.Ret );
			}
			PropertyBuilder isEnabledProperty = tb.DefineProperty( "IsEnabled", PropertyAttributes.HasDefault, typeof( bool ), Type.EmptyTypes );
			isEnabledProperty.SetGetMethod( isEnabledGet );

			MethodBuilder mImplGet = tb.DefineMethod( "get_Implementation",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
				typeof(object),
				Type.EmptyTypes );
			{
				ILGenerator g = mImplGet.GetILGenerator();
				g.Emit( OpCodes.Ldarg_0 );
				g.Emit( OpCodes.Ret );
			}
			MethodBuilder mImplSet = tb.DefineMethod( "set_Implementation",
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
				typeof(void),
				new Type[] { typeof(object) } );
			{
				ILGenerator g = mImplSet.GetILGenerator();
				g.Emit( OpCodes.Ret );
			}
			Type tc = tb.CreateType();

			Object test = Activator.CreateInstance( tc );



		}

	}
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\IChoucrouteService.cs) is part of CiviKey. 
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
using CK.Plugin;

namespace CK.Plugin.Host.Tests
{


    public interface IChoucrouteServiceBase
    {
        void CallFunc();

        [IgnoreServiceStopped]
        int Div( int i, int j );

        int Div( int i, int j, int k );

        Guid ID
        {
            get;
            [IgnoreServiceStopped]
            set;
        }

        [IgnoreServiceStopped]
        DateTime Creation { get; }

        object this[int i] { get; }
    }

	public class SpecEventArgs : EventArgs
	{

	}

	public delegate void AnEventHandler( int i, bool b, string s, object source );

    public interface IChoucrouteService : IChoucrouteServiceBase, IDynamicService
	{
		string this[int i, DateTime d, IDynamicService s, string t, object o, double f, byte b] { get; set; }
		
		void GenericFunc<T>( T a );

        U GenericFunc<T, U, V>( T a, U b, V c )
            where U : struct
            where V : class;
		
		[IgnoreServiceStopped]
		event EventHandler AnyMethodCalled;

		event EventHandler<SpecEventArgs> AnEventGen;

		void RaiseAnEventGen();

        event AnEventHandler AnEvent;

        // We can call RaiseAnEvent when stopped: the AnEvent firing MUST 
		// trigger an exception since AnEvent is not IgnoreServiceRunningStatus.
		[IgnoreServiceStoppedAttribute]
		void RaiseAnEvent();
	}


}

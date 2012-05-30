#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Host.Tests\ChoucrouteServiceNotAvailable.cs) is part of CiviKey. 
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
	public class ChoucrouteServiceNotAvailable_UN : IChoucrouteService
	{
        public void CallFunc()
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

        public int Div( int i, int j )
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

        public int Div( int i, int j, int k )
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

        public Guid ID
        {
            get
            {
                throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
            }
            set
            {
                throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
            }
        }

        public DateTime Creation
        {
            get { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
        }

        public object this[int i]
        {
            get { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
        }

        public string this[int i, DateTime d, IDynamicService s, string t, object o, double f, byte b]
        {
            get
            {
                throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
            }
            set
            {
                throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
            }
        }

        public void GenericFunc<T>( T a )
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

        public U GenericFunc<T, U, V>( T a, U b, V c )
            where U : struct
            where V : class
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

        public event EventHandler AnyMethodCalled
        {
            add { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
            remove { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
        }

        public event EventHandler<SpecEventArgs> AnEventGen
        {
            add { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
            remove { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
        }


        public void RaiseAnEventGen()
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

        public event AnEventHandler AnEvent
        {
            add { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
            remove { throw new ServiceNotAvailableException( typeof( IChoucrouteService ) ); }
        }


        public void RaiseAnEvent()
        {
            throw new ServiceNotAvailableException( typeof( IChoucrouteService ) );
        }

    }
}

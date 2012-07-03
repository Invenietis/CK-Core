#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Animals.cs) is part of CiviKey. 
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

namespace Core
{
    public class Animal
    {
        public Animal( string name )
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return String.Format( "Animals: {0} ({1})", Name, GetHashCode() );
        }
    }

    public class Mammal : Animal
    {
        public Mammal( string name, int gestationPeriod = 12 )
            : base( name )
        {
            Name = name;
            GestationPeriod = gestationPeriod;
        }

        public int GestationPeriod { get; set; }

        public override string ToString()
        {
            return String.Format( "Mammals: {0}, {1} ({2})", Name, GestationPeriod, GetHashCode() );
        }

    }

    public class Canidae : Mammal
    {
        public Canidae( string name, int gestationPeriod = 9, bool isRetriever = false )
            : base( name, gestationPeriod )
        {
            Name = name;
            GestationPeriod = gestationPeriod;
            IsRetriever = isRetriever;
        }

        public bool IsRetriever { get; set; }

        public override string ToString()
        {
            return String.Format( "Canidae: {0}, {1}, {2} ({3})", Name, GestationPeriod, IsRetriever, GetHashCode() );
        }
    }
}

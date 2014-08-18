#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Storage.Tests\SerializableObjects.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml.Serialization;
using System.Xml;
using CK.Storage;
using CK.Core;

namespace Storage
{

    public enum TestEnumValues
    {
        First = 1,
        Second = 2
    }

    [Serializable]
    public class SerializableObject : IEquatable<SerializableObject>
    {
        public int Power;
        public string Name = String.Empty;

        public bool Equals( SerializableObject other )
        {
            return other != null && Name == other.Name && Power == other.Power;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as SerializableObject );
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Power;
        }

    }

    public class StructuredSerializableObject : IStructuredSerializable, IEquatable<StructuredSerializableObject>
    {
        public int OneInteger { get; set; }

        public string OneString { get; set; }

        public StructuredSerializableObject()
        {
            OneInteger = 0;
            OneString = "Test it";
        }

        public void ReadContent( IStructuredReader sr )
        {
            Assert.That( sr is ISubStructuredReader );
            OneInteger = sr.Xml.GetAttributeInt( "OneI", -7657 );
            sr.Xml.Read();
            OneString = sr.Xml.ReadString();
        }

        public void WriteContent( IStructuredWriter sw )
        {
            sw.Xml.WriteAttributeString( "OneI", OneInteger.ToString() );
            sw.Xml.WriteString( OneString );
        }
        public bool Equals( StructuredSerializableObject other )
        {
            return other != null && OneInteger == other.OneInteger && OneString == other.OneString;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as StructuredSerializableObject );
        }

        public override int GetHashCode()
        {
            return OneInteger.GetHashCode() ^ OneString.GetHashCode();
        }

    }

}

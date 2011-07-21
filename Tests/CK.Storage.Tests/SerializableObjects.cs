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

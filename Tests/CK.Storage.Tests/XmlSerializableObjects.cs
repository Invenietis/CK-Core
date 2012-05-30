#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Storage.Tests\XmlSerializableObjects.cs) is part of CiviKey. 
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
using NUnit.Framework;
using System.Xml.Serialization;
using System.Xml;
using CK.Core;
using CK.Storage;
using System.IO;

namespace Storage
{

    public class XmlObjectViaIXmlSerializable : IXmlSerializable, IEquatable<XmlObjectViaIXmlSerializable>
    {
        public int Power;
        public string Name = String.Empty;

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml( XmlReader r )
        {
            // This is the specification: we are on the opening of the wrapper element.
            r.ReadStartElement();

            Assert.That( r.IsStartElement( "AnXmlObject" ) );
            Power = r.GetAttributeInt( "Power", 0 );
            r.Read();
            r.ReadStartElement( "Name" );
            Name = r.ReadString();
            r.ReadEndElement();
            r.ReadEndElement(); // AnXmlObject

            // This is the specification: ReadXml MUST read the end element of the wrapper.
            r.ReadEndElement();
        }

        void IXmlSerializable.WriteXml( XmlWriter w )
        {
            w.WriteStartElement( "AnXmlObject" );
            w.WriteAttributeString( "Power", Power.ToString() );
            w.WriteStartElement( "Name" );
            w.WriteString( Name );
            w.WriteEndElement(); // Name
            w.WriteEndElement(); // AnXmlObject
        }

        public bool Equals( XmlObjectViaIXmlSerializable other )
        {
            return other != null && Name == other.Name && Power == other.Power;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as XmlObjectViaIXmlSerializable );
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Power;
        }
    }

    public class XmlObjectViaAttributes : IEquatable<XmlObjectViaAttributes>
    {
        [XmlAttribute( AttributeName="PowerAttr" )]
        public int Power;

        [XmlElement]
        public string Name = String.Empty;

        public bool Equals( XmlObjectViaAttributes other )
        {
            return other != null && Name == other.Name && Power == other.Power;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as XmlObjectViaAttributes );
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Power;
        }
    }

    [TestFixture]
    public class XmlSerializableObjects
    {
        [SetUp]
        [TearDown]
        public void Setup()
        {
            TestBase.CleanupTestDir();
        }

        [Test]
        public void TestXmlSerializer()
        {
            XmlSerializer serIXml = new XmlSerializer( typeof( XmlObjectViaIXmlSerializable ) );
            XmlSerializer serAttr = new XmlSerializer( typeof( XmlObjectViaAttributes ) );

            object oIxml = new XmlObjectViaIXmlSerializable() { Name = "York", Power = 126 };
            object oAttr = new XmlObjectViaAttributes() { Name = "York n°2", Power = 47 };

            string xmlPath = TestBase.GetTestFilePath( "Storage", "TestXmlSerializer" );
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.Xml.WriteStartElement( "TestIXml" );
                    serIXml.Serialize( writer.Xml, oIxml );
                    writer.Xml.WriteEndElement(); // TestIXml

                    writer.Xml.WriteStartElement( "TestAttr" );
                    serAttr.Serialize( writer.Xml, oAttr );
                    writer.Xml.WriteEndElement(); // TestAttr

                    writer.WriteObjectElement( "Before", 3712 * 2 );

                    writer.WriteObjectElement( "data", oIxml );

                    writer.WriteObjectElement( "After", 3712 * 3 );
                }
            }
            TestBase.DumpFileToConsole( xmlPath );
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add<ISimpleTypeFinder>( SimpleTypeFinder.Default );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    reader.Xml.ReadStartElement( "TestIXml" );
                    object oIXml2 = serIXml.Deserialize( reader.Xml );
                    Assert.That( oIXml2, Is.EqualTo( oIxml ) );
                    reader.Xml.ReadEndElement(); // TestIXml
                    
                    reader.Xml.ReadStartElement( "TestAttr" );
                    object oAttr2 = serAttr.Deserialize( reader.Xml );
                    Assert.That( oAttr2, Is.EqualTo( oAttr ) );
                    reader.Xml.ReadEndElement(); // TestAttr
                    
                    Assert.That( reader.ReadObjectElement( "Before" ), Is.EqualTo( 3712 * 2 ) );

                    object oIXml2bis = reader.ReadObjectElement( "data" );
                    Assert.That( oIXml2bis, Is.EqualTo( oIxml ) );

                    // Since we can not (yet) inject XmlSerializer, the XmlObjectViaAttributes
                    // can not be serialized as object.
                    // This must be done for the moment with an external Serializer.

                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 3 ) );
                }
            }


        }
    }

}

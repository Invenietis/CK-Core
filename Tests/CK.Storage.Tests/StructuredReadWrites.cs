#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Storage.Tests\StructuredReadWrites.cs) is part of CiviKey. 
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
using System.IO;
using CK.Core;
using System.Collections;
using System.Drawing;
using CK.Storage;
using System.Diagnostics;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Storage
{
    [TestFixture]
    public class StructuredReadWrites
    {
        [SetUp]
        [TearDown]
        public void Setup()
        {
            TestBase.CleanupTestDir();
        }

        [Test]
        public void EmptyFile()
        {
            string test = TestBase.GetTestFilePath( "Storage", "EmptyFile" );
            using( Stream wrt = new FileStream( test, FileMode.Create ) )
            {
                IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() );
                writer.Dispose();
            }

            Assert.That( File.Exists( test ) );

            using( Stream str = new FileStream( test, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    Assert.That( reader.StorageVersion, Is.GreaterThanOrEqualTo( new Version( 2, 5, 0 ) ) );
                }
            }
        }

        [Test]
        public void FirstTest()
        {
            string test = TestBase.GetTestFilePath( "Storage", "FirstTest" );
            using( Stream wrt = new FileStream( test, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", (int)10 );
                    writer.Xml.WriteStartElement( "test.done" );
                    writer.Xml.WriteEndElement();
                }
            }
            using( Stream str = new FileStream( test, FileMode.Open ) )
            {
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, null ) )
                {
                    CheckExactTypeAndValue( typeof( int ), 10, reader.ReadObjectElement( "data" ) );
                    Assert.That( reader.Xml.IsStartElement( "test.done" ) );
                    Assert.That( reader.Xml.Read() );
                }
            }
        }

        [Test]
        [ExpectedException( typeof( FileNotFoundException ) )]
        public void ReadUnexistingFileWithException()
        {
            string path = TestBase.GetTestFilePath( "Storage", "UnexistingFileWithException" );

            Assert.That( !File.Exists( path ) );
            using( Stream str = new FileStream( path, FileMode.Open ) )
            {
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, null ) )
                {
                    // ExpectedException( typeof( FileNotFoundException ) )
                }
            }
        }

        [Test]
        public void ReadUnexistingFileWithoutException()
        {
            string path = TestBase.GetTestFilePath( "Storage", "UnexistingFileWithoutException" );
            Assert.That( !File.Exists( path ) );
            Assert.That( SimpleStructuredReader.CreateReader( null, new SimpleServiceContainer(), false ), Is.Null );
        }

        [Test]
        public void TestIntegers()
        {
            string test = TestBase.GetTestFilePath( "Storage", "TestIntegers" );
            using( Stream wrt = new FileStream( test, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", (int)10 );
                    writer.WriteObjectElement( "data", (SByte)(-8) );
                    writer.WriteObjectElement( "data", (Int16)(-16) );
                    writer.WriteObjectElement( "data", (Int32)(-32) );
                    writer.WriteObjectElement( "data", (Int64)(-64) );
                    writer.WriteObjectElement( "data", (Byte)8 );
                    writer.WriteObjectElement( "data", (UInt16)16 );
                    writer.WriteObjectElement( "data", (UInt32)32 );
                    writer.WriteObjectElement( "data", (UInt64)64 );
                }
            }
            using( Stream str = new FileStream( test, FileMode.Open ) )
            {
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, null ) )
                {
                    CheckExactTypeAndValue( typeof( int ), 10, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( SByte ), -8, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( Int16 ), -16, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( Int32 ), -32, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( Int64 ), -64, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( Byte ), 8, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( UInt16 ), 16, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( UInt32 ), 32, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( UInt64 ), 64, reader.ReadObjectElement( "data" ) );
                }
            }
        }

        [Test]
        public void XmlObjectCorrect()
        {
            string path;
            
            path = TestBase.GetTestFilePath( "Storage", "XmlObjectCorrect.Structured" );
            TestXmlSerializableObject( path, new XmlRawObjectStructured() { Name = "Normal Structured", Power = 23 } );

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectCorrect.XmlSerializable" );
            TestXmlSerializableObject( path, new XmlRawObjectXmlSerialzable() { Name = "Normal IXmlSerializable", Power = 230 } );
        }

        [Test]
        public void XmlObjectSkiptTag()
        {
            string path;

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectSkiptTag.Structured" );
            TestXmlSerializableObject( path, new XmlRawObjectStructured() { Name = "Buggy Structured", Power = 23, BugWhileReading = BugRead.SkipTag } );

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectSkiptTag.XmlSerializable" );
            TestXmlSerializableObject( path, new XmlRawObjectXmlSerialzable() { Name = "Buggy IXmlSerializable", Power = 230, BugWhileReading = BugRead.SkipTag } );
        }

        [Test]
        public void XmlObjectMoveToEndTag()
        {
            string path;

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectMoveToEndTag.Structured" );
            TestXmlSerializableObject( path, new XmlRawObjectStructured() { Name = "Buggy Structured", Power = 23, BugWhileReading = BugRead.MoveToEndTag } );

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectMoveToEndTag.XmlSerializable" );
            TestXmlSerializableObject( path, new XmlRawObjectXmlSerialzable() { Name = "Buggy IXmlSerializable", Power = 230, BugWhileReading = BugRead.MoveToEndTag } );
        }

        [Test]
        public void XmlObjectThrows()
        {
            string path;

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectThrows.Structured" );
            TestXmlSerializableObject( path, new XmlRawObjectStructured() { Name = "Buggy Structured", Power = 23, BugWhileReading = BugRead.ThrowApplicationException } );

            path = TestBase.GetTestFilePath( "Storage", "XmlObjectThrows.XmlSerializable" );
            TestXmlSerializableObject( path, new XmlRawObjectXmlSerialzable() { Name = "Buggy IXmlSerializable", Power = 230, BugWhileReading = BugRead.ThrowApplicationException } );
        }

        private void TestXmlSerializableObject( string path, XmlRawObjectBase original )
        {
            using( Stream wrt = new FileStream( path, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "Before", 3712 );
                    writer.WriteObjectElement( "data", original );
                    writer.WriteObjectElement( "After", 3712*2 );
                }
            }
            using( Stream str = new FileStream( path, FileMode.Open ) )
            {
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, null ) )
                {
                    Assert.That( reader.ReadObjectElement( "Before" ), Is.EqualTo( 3712 ) );
                    if( original.BugWhileReading == BugRead.ThrowApplicationException )
                    {
                        Assert.Throws<ApplicationException>( () => reader.ReadObjectElement( "data" ) );
                        // Even if an exception is thrown, we can continue to read the data.
                    }
                    else if( original.BugWhileReading == BugRead.None )
                    {
                        CheckExactTypeAndValue( original.GetType(), original, reader.ReadObjectElement( "data" ) );
                    }
                    else
                    {
                        XmlRawObjectBase read = (XmlRawObjectBase)reader.ReadObjectElement( "data" );
                        Assert.That( read.BugWhileReading == original.BugWhileReading );
                    }
                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ), "Whatever happens above, one can continue to read." );
                }
            }
        }

        [Test]
        public void BinarySerializableObject()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "TestBinarySerializableObject" );
            SerializableObject o = new SerializableObject();
            o.Name = "TestName";
            o.Power = 20;

            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "Before", 3712 );
                    writer.WriteObjectElement( "data", o );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                }
            }
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    Assert.That( reader.ReadObjectElement( "Before" ), Is.EqualTo( 3712 ) );
                    
                    SerializableObject o2 = (SerializableObject)reader.ReadObjectElement( "data" );
                    Assert.AreEqual( o.Name, o2.Name );
                    Assert.AreEqual( o.Power, o2.Power );

                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
        }

        [Test]
        public void GenericListOfString()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "TestGenericListOfString" );
            List<string> list = new List<string>();
            list.Add( "content1" );
            list.Add( "content2" );
            list.Add( "content3" );

            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "Before", 3712 );
                    writer.WriteObjectElement( "data", list );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                }
            }
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add<ISimpleTypeFinder>( SimpleTypeFinder.WeakDefault );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    Assert.That( reader.ReadObjectElement( "Before" ), Is.EqualTo( 3712 ) );
                    CheckExactTypeAndValue( typeof( List<string> ), list, reader.ReadObjectElement( "data" ) );
                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
        }

        [Test]
        public void ColorStruct()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "TestColor" );
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", Color.Red );
                    writer.WriteObjectElement( "data", Color.Blue );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                }
            }
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                //s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    CheckExactTypeAndValue( typeof( Color ), Color.Red, reader.ReadObjectElement( "data" ) );
                    CheckExactTypeAndValue( typeof( Color ), Color.Blue, reader.ReadObjectElement( "data" ) );
                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
        }

        [Test]
        public void ArrayListWithSerializableObjects()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "TestGenericListOfString" );
            ArrayList list = new ArrayList();
            SerializableObject firstObject = new SerializableObject() { Name = "Albert", Power = 34 };
            list.Add( firstObject );
            list.Add( new DateTime( 2009, 01, 11 ) );
            list.Add( "Franchement, les mecs, vous trouvez que c'est normal que ce soit Spi qui se cogne tous les tests unitaires ?" );

            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", list );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                }
            }
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    ArrayList list2 = (ArrayList)reader.ReadObjectElement( "data" );
                    Assert.AreEqual( ((SerializableObject)list2[0]).Name, ((SerializableObject)list[0]).Name );
                    Assert.AreEqual( ((SerializableObject)list2[0]).Power, ((SerializableObject)list[0]).Power );
                    CheckExactTypeAndValue( typeof( DateTime ), list[1], list2[1] );
                    CheckExactTypeAndValue( typeof( string ), list[2], list2[2] );

                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
        }

        [Test]
        public void TestEnum()
        {
            DoTestEnum( null );
        }

        private void DoTestEnum( Action<XDocument> docModifier )
        {
            string test = TestBase.GetTestFilePath( "Storage", "TestEnum" );
            using( Stream wrt = new FileStream( test, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", TestEnumValues.First );
                    writer.WriteObjectElement( "data", TestEnumValues.Second );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                }
            }
            LoadAndModifyXml( test, docModifier );
            using( Stream str = new FileStream( test, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    TestEnumValues value1 = (TestEnumValues)reader.ReadObjectElement( "data" );
                    TestEnumValues value2 = (TestEnumValues)reader.ReadObjectElement( "data" );
                    Assert.That( value1, Is.EqualTo( TestEnumValues.First ) );
                    Assert.That( value2, Is.EqualTo( TestEnumValues.Second ) );
                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
        }

        [Test]
        [ExpectedException( typeof( CKException ) )]
        public void BugBinaryBadContent()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "BugBinaryBadContent" );
            SerializableObject original = new SerializableObject() { Name = "coucou", Power = 20 };
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, s ) )
                {
                    writer.WriteObjectElement( "data", original );
                }
            }
            LoadAndModifyXml( xmlPath, d =>
            {
                var e = d.Root.Element( "data" );
                e.SetValue( e.Value.Insert( e.Value.Length / 2, "*bug*" ) );
            } );
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, new SimpleServiceContainer() ) )
                {
                    object obj = reader.ReadObjectElement( "data" );
                }
            }
        }

        [Test]
        [ExpectedException( typeof( CKException ) )]
        public void BugBinaryTooBigContent()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "BugBinaryTooBigContent" );
            SerializableObject original = new SerializableObject() { Name = "coucou", Power = 20 };
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", original );
                }
            }
            LoadAndModifyXml( xmlPath, d =>
            {
                var e = d.Root.Element( "data" );
                e.SetValue( e.Value.Insert( e.Value.Length / 2, "00FF00FF" ) );
            } );
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    object obj = reader.ReadObjectElement( "data" );
                }
            }
        }

        [Test]
        public void BugBinarySizeDiffer()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "BugBinarySizeDiffer" );
            SerializableObject original = new SerializableObject() { Name = "coucou", Power = 20 };
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", original );
                }
            }
            LoadAndModifyXml( xmlPath, d => d.Root.Element( "data" ).Attribute( "size" ).SetValue( "1" ) );

            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add( typeof( ISimpleTypeFinder ), SimpleTypeFinder.WeakDefault, null );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    Assert.Throws<CKException>( () => reader.ReadObjectElement( "data" ) );
                }
            }
        }

        [Test]
        public void StructuredSerializedObjectTest()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "FakeStructuredSerializedObject" );
            StructuredSerializableObject original = new StructuredSerializableObject() { OneInteger = 43, OneString = "Let's go..." };
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.ObjectWriteExData += ( s, e ) =>
                        {
                            if( e.Obj == original )
                            {
                                e.Writer.Xml.WriteStartElement( "ExtraData" );
                                e.Writer.Xml.WriteAttributeString( "OneAtrribute", "23" );
                                e.Writer.Xml.WriteElementString( "SubValue", "string in element..." );
                                e.Writer.Xml.WriteEndElement();
                            }
                        };
                    writer.WriteObjectElement( "data", original );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                }
            }
            TestBase.DumpFileToConsole( xmlPath );
            // Reads without reading ExtraData element.
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add<ISimpleTypeFinder>( SimpleTypeFinder.WeakDefault );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    object read = reader.ReadObjectElement( "data" );
                    Assert.That( read, Is.TypeOf( typeof( StructuredSerializableObject ) ) );
                    StructuredSerializableObject newOne = read as StructuredSerializableObject;
                    Assert.That( newOne.OneString, Is.EqualTo( original.OneString ) );
                    Assert.That( newOne.OneInteger, Is.EqualTo( original.OneInteger ) );

                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
            // Reads ExtraData element.
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add<ISimpleTypeFinder>( SimpleTypeFinder.WeakDefault );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    reader.ObjectReadExData += ( source, e ) =>
                        {
                            Assert.That( e.Reader.Xml.IsStartElement( "ExtraData" ) );
                            Assert.That( e.Reader.Xml.GetAttributeInt( "OneAtrribute", -12 ), Is.EqualTo( 23 ) );
                            e.Reader.Xml.Read();
                            Assert.That( e.Reader.Xml.ReadElementContentAsString(), Is.EqualTo( "string in element..." ) );
                            // Forget to read the end element.
                            Assert.That( e.Reader.Xml.NodeType == XmlNodeType.EndElement );
                        };

                    object read = reader.ReadObjectElement( "data" );
                    Assert.That( read, Is.TypeOf( typeof( StructuredSerializableObject ) ) );
                    StructuredSerializableObject newOne = read as StructuredSerializableObject;
                    Assert.That( newOne.OneString, Is.EqualTo( original.OneString ) );
                    Assert.That( newOne.OneInteger, Is.EqualTo( original.OneInteger ) );

                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                }
            }
        }

        public class UnexistingTypeFinder : SimpleTypeFinder
        {

            public override Type ResolveType( string assemblyQualifiedName, bool throwOnError )
            {
                if( assemblyQualifiedName.Contains( "Unexisting" ) )
                {
                    if( throwOnError ) throw new TypeLoadException( "Unexisting Type." );
                    else return null;
                }
                return base.ResolveType( assemblyQualifiedName, throwOnError );
            }
        }

        public enum UnexistingTestEnumValues
        {
            First = 1
        }

        public class MayBeUnexistingButValidXmlObject : IXmlSerializable
        {
            public System.Xml.Schema.XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml( XmlReader reader )
            {
                // This is the specification: we are on the opening of the wrapper element.
                reader.ReadStartElement();
                
                reader.ReadStartElement( "UnexistingObject" );
                reader.ReadStartElement( "Content" );
                Assert.That( reader.ReadString(), Is.EqualTo( "content - value" ) );
                reader.ReadEndElement();
                reader.ReadEndElement();

                // This is the specification: ReadXml MUST read the end element of the wrapper.
                reader.ReadEndElement();
            }

            public void WriteXml( XmlWriter writer )
            {
                writer.WriteStartElement( "UnexistingObject" );
                writer.WriteAttributeString( "OneAttr", "attr-Value" );
                writer.WriteStartElement( "Content" );
                writer.WriteString( "content - value" );
                writer.WriteEndElement();
                writer.WriteEndElement(); // UnexistingObject
            }

        }


        /// <summary>
        /// The UnexistingTypeFinder set will return a null type when trying to read UnexistingXXX type.
        /// </summary>
        [Test]
        public void BugUnexisting()
        {
            string xmlPath = TestBase.GetTestFilePath( "Storage", "BugUnexistingEnum" );
            using( Stream wrt = new FileStream( xmlPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( wrt, new SimpleServiceContainer() ) )
                {
                    writer.WriteObjectElement( "data", UnexistingTestEnumValues.First );
                    writer.WriteObjectElement( "After", 3712 * 2 );
                    writer.WriteObjectElement( "data", new MayBeUnexistingButValidXmlObject() );
                    writer.WriteObjectElement( "After2", 3712 * 3 );
                }
            }
            TestBase.DumpFileToConsole( xmlPath );
            using( Stream str = new FileStream( xmlPath, FileMode.Open ) )
            {
                SimpleServiceContainer s = new SimpleServiceContainer();
                s.Add<ISimpleTypeFinder>( new UnexistingTypeFinder() );
                using( IStructuredReader reader = SimpleStructuredReader.CreateReader( str, s ) )
                {
                    Assert.Throws<TypeLoadException>( () => reader.ReadObjectElement( "data" ) );
                    // An exception does not break the reader.
                    Assert.That( reader.ReadObjectElement( "After" ), Is.EqualTo( 3712 * 2 ) );
                    Assert.Throws<TypeLoadException>( () => reader.ReadObjectElement( "data" ) );
                    // An exception does not break the reader.
                    Assert.That( reader.ReadObjectElement( "After2" ), Is.EqualTo( 3712 * 3 ) );
                }
            }
        }

        [Test]
        public void VersionInAssemblyQualifiedTypeName()
        {
            DoTestEnum( delegate( XDocument d )
            {
                var types = d.Descendants( "data" ).Select( e => e.Attribute( "typeName" ) );
                foreach( var t in types ) ChangeVersionInfo( t );
            } );
        }

        [Test]
        public void VersionInAssemblyQualifiedTypeNameExternal()
        {
            // A syntax error in version, culture or token is silently ignored...
            Type tSyntaxError = Type.GetType( "ExternalDll.ExternalClass, CK.Storage.Tests.ExternalDll, VersionSYNTAX=1.0.0.0, CultureSYNTAX=neutral, PublicKeyTokenSYNTAX=b77a5c561934e089", true );
            Assert.That( tSyntaxError, Is.Not.Null );
            //As dlls are signed, version, culture & PublicTokenKey must match. Testing the WeakTypeFinder, that truncates these information, to load the type regardeless of them
            ISimpleTypeFinder wtf = SimpleTypeFinder.WeakDefault;
            Type t2 = SimpleTypeFinder.WeakDefault.ResolveType("ExternalDll.ExternalClass, CK.Storage.Tests.ExternalDll, Version=5.0.4.0, Culture=neutral, PublicKeyToken=null", true );
            Type t1 = SimpleTypeFinder.WeakDefault.ResolveType("ExternalDll.ExternalClass, CK.Storage.Tests.ExternalDll, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true );
            //Provinding only namespace.classname, assembly works properly
            Type t0 = Type.GetType( "ExternalDll.ExternalClass, CK.Storage.Tests.ExternalDll", true );

            // When the full name or the assembly can not be found, GetType( ..., false ) gently returns null.
            //           
            Type tClassNotFound = Type.GetType( "ExternalDll.Unknown.ExternalClass, CK.Storage.Tests.ExternalDll", false );
            Assert.That( tClassNotFound, Is.Null );

            Type tAssemblyNotFound = Type.GetType( "ExternalDll.ExternalClass, CK.Storage.Tests.Unknown.ExternalDll", false );
            Assert.That( tAssemblyNotFound, Is.Null );


            // BUT: an invalid PublicKeyToken throws a FileLoadException regardless of the throwOnError parameter...
            //
            // Type tInvalidSignature = Type.GetType( "ExternalDll.ExternalClass, CK.Storage.Tests.ExternalDll, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false );
            Assert.Throws<FileLoadException>( () => Type.GetType( "ExternalDll.ExternalClass, CK.Storage.Tests.ExternalDll, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false ) );
        }

        private static void ChangeVersionInfo( XAttribute t )
        {
            Group version = Regex.Match( t.Value, "Version=(?<1>[\\d\\.]+)" ).Groups[1];
            var current = new Version( version.Value );
            var next = new Version( current.Major + 1, current.Minor + 1, current.Build, current.Revision );
            string newTypeName = t.Value.Remove( version.Index, version.Length ).Insert( version.Index, next.ToString() );
            t.SetValue( newTypeName );
        }

        #region Utility methods

        private void CheckExactTypeAndValue( Type type, object value, object o )
        {
            Assert.That( o, Is.InstanceOf( type ) );
            Assert.That( o, Is.EqualTo( value ) );
        }

        private void LoadAndModifyXml( string path, Action<XDocument> hook )
        {
            if( hook != null )
            {
                XDocument d = XDocument.Load( path );
                hook( d );
                d.Save( path );
            }
        }
        #endregion
    }
}

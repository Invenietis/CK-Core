#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Storage.Tests\StructuredSerializer.cs) is part of CiviKey. 
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
using System.Xml;
using System.IO;
using CK.Storage;
using CK.Core;
using System.Globalization;

namespace Storage
{
    public class Paw
    {
        public int FingerCount { get; set; }

        public Paw( int fingerCount )
        {
            FingerCount = fingerCount;
        }
    }

    public class Dog
    {
        public string Name { get; set; }
        public List<Paw> Paws { get; set; }

        public Dog()
        {
        }

        public Dog( string name, List<Paw> paws )
        {
            Name = name;
            Paws = paws;
        }
    }

    /// <summary>
    /// This class is used to ensure that no clash happens when registering
    /// services from a scope that the developper consider independant.
    /// </summary>
    class UniqueService
    {
    }

    public class SimpleStructuredPawSerializer : IStructuredSerializer<Paw>
    {
        public object ReadInlineContent( IStructuredReader sr, Paw p )
        {
            // This is an independant scope...
            sr.ServiceContainer.Add<UniqueService>( new UniqueService() );
            
            XmlReader r = sr.Xml;
            int pawCount = Int32.Parse( r.GetAttribute( "fingerCount" ) );
            if( (pawCount % 1) == 0 )
            {
                r.Read();
                string extra = r.ReadElementContentAsString( "ExtraInfo", String.Empty );
                Assert.That( extra, Is.EqualTo( "Content of the ExtraInfo" ) );
            }
            return new Paw( pawCount );
        }

        public void WriteInlineContent( IStructuredWriter sw, Paw o )
        {
            // This is an independant scope...
            sw.ServiceContainer.Add<UniqueService>( new UniqueService() );
            
            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "fingerCount", o.FingerCount.ToString( CultureInfo.InvariantCulture ) );
            if( (o.FingerCount % 1) == 0 )
            {
                w.WriteStartElement( "ExtraInfo" );
                w.WriteString( "Content of the ExtraInfo" );
                w.WriteEndElement();
            }
        }
    }

    public class SimpleStructuredDogSerializer : IStructuredSerializer<Dog>
    {
        public object ReadInlineContent( IStructuredReader sr, Dog d )
        {
            // This is an independant scope...
            sr.ServiceContainer.Add<UniqueService>( new UniqueService() );

            XmlReader r = sr.Xml;
            if( d == null ) d = new Dog();

            d.Name = r.GetAttribute( "Name" );
            // Leaves the current open element with its attributes.
            r.Read();
            d.Paws = new List<Paw>();
            while( r.IsStartElement( "Paw" ) )
            {
                d.Paws.Add( (Paw)sr.ReadInlineObjectStructured( typeof(Paw) ) );
            }
            return d;
        }

        public void WriteInlineContent( IStructuredWriter sw, Dog o )
        {
            // This is an independant scope...
            sw.ServiceContainer.Add<UniqueService>( new UniqueService() );

            XmlWriter w = sw.Xml;
            w.WriteAttributeString( "Name", o.Name );
            foreach( Paw paw in o.Paws )
            {
                w.WriteStartElement( "Paw" );
                sw.WriteInlineObjectStructured( paw );
            }
        }
    }

    [TestFixture]
    public class StructuredSerializer
    {
        [SetUp]
        [TearDown]
        public void Setup()
        {
            TestBase.CleanupTestDir();
        }

        [Test]
        public void SimpleStructuredSerializerTest()
        {
            string testPath = TestBase.GetTestFilePath( "CKTests.Storage", "StructuredSerializer" );
            IStructuredSerializer<Dog> serializer = new SimpleStructuredDogSerializer();
            Dog dog = CreateDog();

            using( Stream str = new FileStream( testPath, FileMode.Create ) )
            {
                using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( str, new SimpleServiceContainer() ) )
                {
                    // This is an independant scope: we just created the writer...
                    writer.ServiceContainer.Add<UniqueService>( new UniqueService() );
                    writer.ServiceContainer.Add<IStructuredSerializer<Dog>>( new SimpleStructuredDogSerializer() );
                    writer.ServiceContainer.Add<IStructuredSerializer<Paw>>( new SimpleStructuredPawSerializer() );
                    
                    writer.WriteObjectElement( "Dog", dog );
                }
            }

            Dog readDog;

            // 1 - Use ReadInlineObject
            using( Stream str = new FileStream( testPath, FileMode.Open ) )
            using( IStructuredReader reader = CreateConfiguredReader( str ) )
            {
                // This is an independant scope: we just created the reader...
                reader.ServiceContainer.Add<UniqueService>( new UniqueService() );
                
                StandardReadStatus status;
                object o = reader.ReadInlineObject( out status );
                readDog = o as Dog;
            }
            CheckReadDog( readDog );

            // 2 - Use ReadInlineObjectStructured( Type )
            using( Stream str = new FileStream( testPath, FileMode.Open ) )
            using( IStructuredReader reader = CreateConfiguredReader( str ) )
            {
                // This is an independant scope: we just created the reader...
                reader.ServiceContainer.Add<UniqueService>( new UniqueService() );
                
                Assert.That( reader.Xml.IsStartElement( "Dog" ) );
                // We ignore attributes added by WriteObjectElement: we directly call ReadInlineObjectStructured for the Dog type.
                object o = reader.ReadInlineObjectStructured( typeof( Dog ), null );
                readDog = o as Dog;
            }
            CheckReadDog( readDog );
            
            // 3 - Use ReadInlineObjectStructured( object )
            using( Stream str = new FileStream( testPath, FileMode.Open ) )
            using( IStructuredReader reader = CreateConfiguredReader( str ) )
            {
                // This is an independant scope: we just created the reader...
                reader.ServiceContainer.Add<UniqueService>( new UniqueService() );
                
                Assert.That( reader.Xml.IsStartElement( "Dog" ) );
                readDog = new Dog();
                // We ignore attributes added by WriteObjectElement: we directly call ReadInlineObjectStructured for an empty Dog object.
                object o = reader.ReadInlineObjectStructured( readDog );
            }
            CheckReadDog( readDog );
        }

        public Dog CreateDog()
        {
            List<Paw> limbs = new List<Paw>();
            limbs.Add( new Paw( 3 ) );
            limbs.Add( new Paw( 6 ) );
            limbs.Add( new Paw( 5 ) );
            limbs.Add( new Paw( 4 ) );

            return new Dog( "Edgar", limbs );
        }

        private static void CheckReadDog( Dog readDog )
        {
            Assert.That( readDog != null );
            Assert.That( readDog.Name, Is.EqualTo( "Edgar" ) );
            Assert.That( readDog.Paws.Count, Is.EqualTo( 4 ) );
            Assert.That( readDog.Paws[0].FingerCount, Is.EqualTo( 3 ) );
            Assert.That( readDog.Paws[1].FingerCount, Is.EqualTo( 6 ) );
            Assert.That( readDog.Paws[2].FingerCount, Is.EqualTo( 5 ) );
            Assert.That( readDog.Paws[3].FingerCount, Is.EqualTo( 4 ) );
        }

        IStructuredReader CreateConfiguredReader( Stream str )
        {
            IStructuredReader reader = SimpleStructuredReader.CreateReader( str, new SimpleServiceContainer() );
            reader.ServiceContainer.Add<ISimpleTypeFinder>( new SimpleTypeFinder() );
            reader.ServiceContainer.Add<IStructuredSerializer<Dog>>( new SimpleStructuredDogSerializer() );
            reader.ServiceContainer.Add<IStructuredSerializer<Paw>>( new SimpleStructuredPawSerializer() );
            return reader;
        }
    }
}

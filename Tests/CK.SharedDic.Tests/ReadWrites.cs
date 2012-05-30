#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\ReadWrites.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using CK.Core;
using CK.Plugin.Config;
using CK.SharedDic;
using CK.Storage;
using NUnit.Framework;
using System.Xml;
using System.Xml.Serialization;

namespace SharedDic
{
    [TestFixture]
    public class ReadWrites
    {
        [Test]
        public void SimpleConfig()
        {
            INamedVersionedUniqueId id1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId id2 = SharedDicTestContext.Plugins[1];

            SharedDictionaryImpl dic1 = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );

            // 1 - Writes the data to the file without any skipped fragments since we do not have any yet.
            dic1[this, id1, "AnIntParam"] = 1;
            dic1[this, id1, "ABoolParam"] = true;
            dic1[this, id1, "AStringParam"] = "This marvellous API works!";
            dic1[this, id1, "ANullObject"] = null;

            dic1[this, id2, "AnIntParam"] = 1;
            dic1[this, id2, "ABoolParam"] = true;
            dic1[this, id2, "AStringParam"] = "This must not be reloaded since the plugin is not runnable for dic2.";

            Assert.That( dic1.Contains( id1 ) && dic1.Contains( id2 ) );

            string path1 = TestBase.GetTestFilePath( "SharedDic", "SimpleConfig" );
            SharedDicTestContext.Write( "Test", path1, dic1, this );
            Assert.IsTrue( new FileInfo( path1 ).Length > 0, "File must exist and be not empty." );

            // 2 - Reloads the file in another dictionary: fragments are now updated with data from the not runnable plugin.
            IList<ReadElementObjectInfo> errors;
            ISharedDictionary dic2 = SharedDicTestContext.Read( "Test", path1, this, d => d.Ensure( id1 ), out errors );
            Assert.AreEqual( 0, errors.Count, "This file has no errors in it." );

            Assert.That( dic1[this, id1, "AnIntParam"], Is.EqualTo( dic2[this, id1, "AnIntParam"] ) );
            Assert.That( dic1[this, id1, "ABoolParam"], Is.EqualTo( dic2[this, id1, "ABoolParam"] ) );
            Assert.That( dic1[this, id1, "AStringParam"], Is.EqualTo( dic2[this, id1, "AStringParam"] ) );
            Assert.That( dic1[this, id1, "ANullObject"], Is.EqualTo( dic2[this, id1, "ANullObject"] ) );
            Assert.That( dic1.Contains( this, id1, "ANullObject" ) && dic2.Contains( this, id1, "ANullObject" ), "The null value has a key." );
            Assert.That( dic2.Contains( this, id2, "AnUnexistingKey" ), Is.False, "Any other key is not defined." );

            // _notRunnables data are not loaded...
            Assert.That( dic2[this, id2, "AnIntParam"], Is.Null );
            Assert.That( dic2.Contains( this, id2, "AnIntParam" ), Is.False );
            Assert.That( dic2[this, id2, "ABoolParam"], Is.Null );
            Assert.That( dic2.Contains( this, id2, "ABoolParam" ), Is.False );
            Assert.That( dic2[this, id2, "AStringParam"], Is.Null );
            Assert.That( dic2.Contains( this, id2, "AStringParam" ), Is.False );
            Assert.That( dic2.Contains( this, id2, "ANullObject" ), Is.False );

            // 3 - Rewrites the file n°2 from dic2 with the skipped fragments.
            string path2 = TestBase.GetTestFilePath( "SharedDic", "SimpleConfig2" );
            SharedDicTestContext.Write( "Test", path2, dic2, this );
            Assert.IsTrue( new FileInfo( path2 ).Length > 0, "File must exist and be not empty." );

            // Files are not equal due to whitespace handling and node reordering.
            // To compare them we should reload the 2 documents and to be independant of the element ordering
            // we need to ensure that every element of d1 exists with the same attributes in d2 and vice-versa.

            // 4 - In fragments, we have the NotRunnables[0] plugin fragment.
            SharedDictionaryImpl implDic2 = (SharedDictionaryImpl)dic2;
            Assert.That( implDic2.GetSkippedFragments( this ).Count == 1 );
            Assert.That( implDic2.GetSkippedFragments( this )[0].PluginId == id2.UniqueId );

            // Activate the previously not runnable plugin.
            dic2.Ensure( id2 );

            Assert.That( implDic2.GetSkippedFragments( this ), Is.Null, "Fragment has been read (no more fragments)." );
            Assert.AreEqual( dic1[this, id2, "AnIntParam"], dic2[this, id1, "AnIntParam"] );
            Assert.AreEqual( dic1[this, id2, "ABoolParam"], dic2[this, id1, "ABoolParam"] );
            Assert.AreEqual( dic1[this, id2, "AStringParam"], "This must not be reloaded since the plugin is not runnable for dic2." );
            Assert.AreEqual( dic1[this, id1, "AStringParam"], "This marvellous API works!" );
        }

        [Test]
        public void BugBinarySizeDiffer()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];
            string path = TestBase.GetTestFilePath( "SharedDic", "BugBinarySizeDiffer" );

            SharedDictionaryTester checker = new SharedDictionaryTester( "BugBinarySizeDiffer", path, this, uid1, uid2, (d,o,id) => d[o, id, "Obj"] = new SerializableObject() );

            checker.WriteAndReadWithTests(
                d => d.SelectSingleNode( @"CK-Structured/BugBinarySizeDiffer/p/data[@key=""Obj""]" ).Attributes["size"].Value = "1",
                dic => { dic.Ensure( uid1 );  dic.Ensure( uid2 ); } );

            Assert.That( checker.Errors.Count, Is.EqualTo( 1 ) );
            Assert.That( checker.Errors[0].HasError, Is.True );
        }

        /// <summary>
        /// When an error occur while reading binary content, XmlReader.ReadState = Error
        /// and this is definitive: this kind of error can not be kindly handled like the other ones.
        /// </summary>
        [Test]        
        public void BugBinaryBadContent()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];
            string path = TestBase.GetTestFilePath( "SharedDic", "BugBinaryBadContent" );

            SharedDictionaryTester checker = new SharedDictionaryTester( "BugBinaryBadContent", path, this, uid1, uid2, (d,o,id) => d[o, id, "Obj"] = new SerializableObject() );
            Assert.Throws<XmlException>( () =>
            {
                checker.WriteAndReadWithTests
                    (
                        d =>
                        {
                            XmlNode n = d.SelectSingleNode( @"CK-Structured/BugBinaryBadContent/p/data[@key=""Obj""]" );
                            n.InnerText = n.InnerText.Insert( n.InnerText.Length / 2, "*bug*" );
                        },
                        dic => { dic.Ensure( uid1 ); dic.Ensure( uid2 ); }
                    );
            });
        }


        /// <summary>
        /// If an object moves the read head anyhow, this is correctly handled.
        /// Only if an exception is thrown by a read is the object let to null.
        /// </summary>
        [Test]
        public void BuggyReaderStructured()
        {
            BuggyReader<BuggyObjectStructured>( 1 );
        }

        /// <summary>
        /// If an object moves the read head anyhow, this is correctly handled.
        /// Only if an exception is thrown by a read is the object let to null.
        /// </summary>
        [Test]
        public void BuggyReaderXml()
        {
            BuggyReader<BuggyObjectXml>( 1 );
        }

        private ISharedDictionary BuggyReader<T>( int expectedErrorCount ) where T : BuggyObjectBase, new()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];

            string path = TestBase.GetTestFilePath( "SharedDic", typeof(T).Name );

            SharedDictionaryTester checker = new SharedDictionaryTester( typeof( T ).Name, path, this, uid1, uid2, ( d, o, id ) =>
            {
                Assert.That( id == uid1 );
                d[o, id, "AnotherValue"] = 13;
                d[o, id, "Obj"] = new T() { Name = "No Bug for this one!" };
                d[o, id, "Obj2"] = new T() { BugWhileReading = BugRead.SkipTag, Name = "This name will not be read back. (But the object exists.)" };
                d[o, id, "Obj3"] = new T() { BugWhileReading = BugRead.MoveToEndTag, Name = "This name will not be read back. (But the object exists.)" };
                d[o, id, "Obj4"] = new T() { BugWhileReading = BugRead.Throw, Name = "This object should not exist at all..." };
                d[o, id, "YetAnotherValue"] = 87;
            } );
            ISharedDictionary d2 = checker.WriteAndReadWithTests( null, dic => { dic.Ensure( uid1 ); dic.Ensure( uid2 ); } );

            TestBase.DumpFileToConsole( path );

            Assert.That( checker.Errors.Count, Is.EqualTo( expectedErrorCount ) );
            Assert.That( checker.Errors.All( e => e.HasError ) );

            Assert.That( d2[this, uid1, "AnotherValue"], Is.EqualTo( 13 ) );
            Assert.That( ((BuggyObjectBase)d2[this, uid1, "Obj"]).Name, Is.EqualTo( "No Bug for this one!" ) );
            Assert.That( ((BuggyObjectBase)d2[this, uid1, "Obj2"]).Name, Is.EqualTo( "Default Name" ) );
            Assert.That( ((BuggyObjectBase)d2[this, uid1, "Obj3"]).Name, Is.EqualTo( "Default Name" ) );
            Assert.That( d2[this, uid1, "Obj4"], Is.Null );

            return d2;

        }

    }
}

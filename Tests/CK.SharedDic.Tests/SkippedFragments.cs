#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\SkippedFragments.cs) is part of CiviKey. 
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
using System.Xml.Serialization;

namespace SharedDic
{
    [TestFixture]
    public class SkippedFragments
    {
        [Test]
        public void CreateSkippedFragments()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];

            // Creates a dummy dictionnary and writes it.
            SharedDictionaryImpl dic = CreateDummySharedDic( this, uid1, uid2 );

            string path = TestBase.GetTestFilePath( "SharedDic", "SkippedFragments" );
            SharedDicTestContext.Write( "Test", path, dic, this );
            Assert.IsTrue( new FileInfo( path ).Length > 0, "File must exist and be not empty." );
            Assert.That( dic.Fragments.Count, Is.EqualTo( 0 ), "There is no skipped fragments for this dic." );

            // Creates a second dictionnary to load previous data (with skippedFragments)
            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dic2 = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
            dic2.Ensure( uid1 );

            Assert.That( errors.Count, Is.EqualTo( 0 ) );
            Assert.That( dic2.Fragments.Count, Is.EqualTo( 1 ) );
            Assert.That( dic2[this, uid2, "key1"], Is.Null );
            Assert.That( dic2[this, uid2, "key2"], Is.Null );
        }

        [Test]
        public void ReloadSkippedFragments()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];

            // Creates a dummy dictionnary and writes it.
            SharedDictionaryImpl dic = CreateDummySharedDic( this, uid1, uid2 );

            // Creates a second dictionnary to load previous data (with skippedFragments)
            string path = TestBase.GetTestFilePath( "SharedDic", "ReloadSkippedFragments" );
            SharedDicTestContext.Write( "Test", path, dic, this );

            IList<ReadElementObjectInfo> errors;

            ISharedDictionary dic2 = SharedDicTestContext.Read( "Test", path, this, d => d.Ensure( uid1 ), out errors );
            SharedDictionaryImpl implDic2 = (SharedDictionaryImpl)dic2;

            Assert.IsTrue( new FileInfo( path ).Length > 0, "File must exist and be not empty." );

            Assert.That( errors.Count, Is.EqualTo( 0 ) );
            Assert.That( implDic2.Fragments.Count, Is.EqualTo( 1 ) );
            Assert.That( dic2[this, uid2, "key1"], Is.Null );
            Assert.That( dic2[this, uid2, "key2"], Is.Null );

            // Now we have skippedFragments. Let's try to reload it.
            dic2.Ensure( uid2 );

            Assert.That( dic2[this, uid2, "key1"], Is.EqualTo( "value1" ) );
            Assert.That( dic2[this, uid2, "key2"], Is.EqualTo( "value2" ) );

            Assert.That( implDic2.Fragments.Count, Is.EqualTo( 0 ) );
        }

        [Test]
        public void ImportSkippedFragments()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];

            string path = TestBase.GetTestFilePath( "SharedDic", "ImportSkippedFragments" );
            #region Creates actual fragments

            // Creates a dummy dictionnary and writes it.
            SharedDictionaryImpl dic = CreateDummySharedDic( this, uid1, uid2 );
            SharedDicTestContext.Write( "Test", path, dic, this );

            // Creates a second dictionnary to load previous data (with skippedFragments)           
            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dicFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, d => d.Ensure( uid1 ), out errors );
            Assert.IsTrue( new FileInfo( path ).Length > 0, "File must exist and be not empty." );

            Assert.That( errors.Count, Is.EqualTo( 0 ) );
            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 1 );
            Assert.That( dicFrag[this, uid2, "key1"], Is.Null );
            Assert.That( dicFrag[this, uid2, "key2"], Is.Null );

            #endregion

            ISharedDictionary dic2 = SharedDictionary.Create( SharedDicTestContext.ServiceProvider );
            dic2[this, uid1, "key1"] = "value1";
            dic2[this, uid1, "key2"] = "value2";
            Assert.That( dic2[this, uid2, "key1"], Is.Null );
            Assert.That( dic2[this, uid2, "key2"], Is.Null );

            SharedDictionaryImpl implDic2 = (SharedDictionaryImpl)dic2;
            implDic2.ImportFragments( dicFrag.Fragments, MergeMode.None );
            Assert.That( implDic2.GetSkippedFragments( this ) != null );

            dic2.Ensure( uid2 );

            Assert.That( dic2[this, uid2, "key1"], Is.EqualTo( "value1" ) );
            Assert.That( dic2[this, uid2, "key2"], Is.EqualTo( "value2" ) );
        }

        [Test]
        public void ImportReloadedSkippedFragments()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];

            string path = TestBase.GetTestFilePath( "SharedDic", "ImportReloadedSkippedFragments" );
            #region Creates actual fragments

            // Creates a dummy dictionnary and writes it.
            SharedDictionaryImpl dic = CreateDummySharedDic( this, uid1, uid2 );
            SharedDicTestContext.Write( "Test", path, dic, this );

            // Creates a second dictionnary to load previous data (with skippedFragments).
            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dicFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
            Assert.IsTrue( new FileInfo( path ).Length > 0, "File must exist and be not empty." );

            Assert.That( errors.Count, Is.EqualTo( 0 ) );
            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 2 );
            Assert.That( dicFrag[this, uid1, "key1"], Is.Null );
            Assert.That( dicFrag[this, uid2, "key2"], Is.Null );

            #endregion

            ISharedDictionary dic2 = SharedDictionary.Create( SharedDicTestContext.ServiceProvider );
            dic2[this, uid1, "key1"] = "value1";
            dic2[this, uid1, "key2"] = "value2";

            Assert.That( dic2[this, uid2, "key1"], Is.Null );
            Assert.That( dic2[this, uid2, "key2"], Is.Null );

            dic2.Ensure( uid2 );

            SharedDictionaryImpl implDic2 = (SharedDictionaryImpl)dic2;
            implDic2.ImportFragments( dicFrag.Fragments, MergeMode.None );
            Assert.That( implDic2.GetSkippedFragments( this ) == null );

            Assert.That( dic2[this, uid2, "key1"], Is.EqualTo( "value1" ) );
            Assert.That( dic2[this, uid2, "key2"], Is.EqualTo( "value2" ) );
        }

        [Test]
        public void TryImportBadSkippedFragments()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];

            string path = TestBase.GetTestFilePath( "SharedDic", "TryImportBadSkippedFragments" );
            #region Creates actual fragments

            object key = new object();

            // Creates a dummy dictionnary and writes it.
            SharedDictionaryImpl dic = CreateDummySharedDic( key, uid1, uid2 );
            SharedDicTestContext.Write( "Test", path, dic, key );

            // Creates a second dictionnary to load previous data (with skippedFragments).
            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dicFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, key, d => d.Ensure( uid1 ), out errors );
            Assert.IsTrue( new FileInfo( path ).Length > 0, "File must exist and be not empty." );

            Assert.That( errors.Count, Is.EqualTo( 0 ) );
            Assert.That( dicFrag.GetSkippedFragments( key ) != null );
            Assert.That( dicFrag[key, uid2, "key1"], Is.Null );
            Assert.That( dicFrag[key, uid2, "key2"], Is.Null );

            #endregion

            SharedDictionaryImpl dic2 = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            dic2.Ensure( uid1 );

            dic2[this, uid1, "key1"] = "value1";
            dic2[this, uid1, "key2"] = "value2";
            Assert.That( dic2[this, uid2, "key1"], Is.Null );
            Assert.That( dic2[this, uid2, "key2"], Is.Null );

            dic2.ImportFragments( dicFrag.Fragments, MergeMode.None );

            Assert.That( dic2.GetSkippedFragments( this ) == null );
        }

        [Test]
        public void MergeFragments_ErrorDuplicate()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];
            INamedVersionedUniqueId uid3 = SharedDicTestContext.Plugins[2];

            SharedDictionaryImpl dic = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            dic[this, uid1, "key1"] = "value1";
            dic[this, uid1, "key2"] = "value2";
            dic[this, uid2, "key1"] = "value1";
            dic[this, uid2, "key2"] = "value2";
            dic[this, uid3, "key1"] = "value1";
            dic[this, uid3, "key2"] = "value2";
            Assert.That( dic.Contains( uid1 ) && dic.Contains( uid2 ) && dic.Contains( uid3 ) );

            string path = TestBase.GetTestFilePath( "SharedDic", "ErrorDuplicate" );
            SharedDicTestContext.Write( "Test", path, dic, this );

            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
            Assert.That( errors.Count == 0 );

            SharedDictionaryImpl dicFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, d => { d.Ensure( uid1 ); d.Ensure( uid2 ); }, out errors );
            Assert.That( errors.Count == 0 );

            Assert.That( dic.GetSkippedFragments( this ) == null );
            Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );
            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 1 );

            Assert.Throws<CKException>( () => dicFrag.ImportFragments( dicFullFrag.Fragments, MergeMode.ErrorOnDuplicate ) );

            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 1 );
        }

        [Test]
        public void MergeFragments_ReplaceExisting()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];
            INamedVersionedUniqueId uid3 = SharedDicTestContext.Plugins[2];

            SharedDictionaryImpl dic = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            dic[this, uid1, "key1"] = "value1";
            dic[this, uid1, "key2"] = "value2";
            dic[this, uid2, "key1"] = "value1";
            dic[this, uid2, "key2"] = "value2";
            dic[this, uid3, "key1"] = "value1";
            dic[this, uid3, "key2"] = "value2";
            Assert.That( dic.Contains( uid1 ) && dic.Contains( uid2 ) && dic.Contains( uid3 ) );
            Assert.That( dic.GetSkippedFragments( this ) == null );

            string path = TestBase.GetTestFilePath( "SharedDic", "MergeFragments" );
            SharedDicTestContext.Write( "Test", path, dic, this );

            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
            Assert.That( errors.Count == 0 );
            Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );

            SharedDictionaryImpl dicFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, d => { d.Ensure( uid1 ); d.Ensure( uid2 ); }, out errors );
            Assert.That( errors.Count == 0 );
            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 1 );

            int hashCode = dicFrag.GetSkippedFragments( this )[0].GetHashCode();

            dicFrag.ImportFragments( dicFullFrag.Fragments, MergeMode.ReplaceExisting );

            Assert.That( dicFrag.GetSkippedFragments( this )[0].GetHashCode() != hashCode );
        }

        [Test]
        public void MergeFragments_PreserveExisting()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];
            INamedVersionedUniqueId uid3 = SharedDicTestContext.Plugins[2];

            SharedDictionaryImpl dic = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            dic[this, uid1, "key1"] = "value1";
            dic[this, uid1, "key2"] = "value2";
            dic[this, uid2, "key1"] = "value1";
            dic[this, uid2, "key2"] = "value2";
            dic[this, uid3, "key1"] = "value1";
            dic[this, uid3, "key2"] = "value2";

            string path = TestBase.GetTestFilePath( "SharedDic", "MergeFragments" );
            SharedDicTestContext.Write( "Test", path, dic, this );

            IList<ReadElementObjectInfo> errors;
            SharedDictionaryImpl dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
            Assert.That( errors.Count == 0 );
            Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );

            SharedDictionaryImpl dicFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, d => { d.Ensure( uid1 ); d.Ensure( uid2 ); }, out errors );
            Assert.That( errors.Count == 0 );
            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 1 );

            int hashCode = dicFrag.GetSkippedFragments( this )[0].GetHashCode();

            dicFrag.ImportFragments( dicFullFrag.Fragments, MergeMode.PreserveExisting );

            Assert.That( dicFrag.GetSkippedFragments( this ) != null );
            Assert.That( dicFrag.GetSkippedFragments( this ).Count == 1 );
            Assert.That( dicFrag.GetSkippedFragments( this )[0].GetHashCode() == hashCode );
        }

        [Test]
        public void SyncDestroyObject()
        {
            INamedVersionedUniqueId uid1 = SharedDicTestContext.Plugins[0];
            INamedVersionedUniqueId uid2 = SharedDicTestContext.Plugins[1];
            INamedVersionedUniqueId uid3 = SharedDicTestContext.Plugins[2];

            string path = TestBase.GetTestFilePath( "SharedDic", "MergeFragments" );
            SharedDictionaryImpl dicFullFrag;

            // Create fragments file.
            {
                SharedDictionaryImpl dic = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
                dic[this, uid1, "key1"] = "value1";
                dic[this, uid1, "key2"] = "value2";
                dic[this, uid2, "key1"] = "value1";
                dic[this, uid2, "key2"] = "value2";
                dic[this, uid3, "key1"] = "value1";
                dic[this, uid3, "key2"] = "value2";

                SharedDicTestContext.Write( "Test", path, dic, this );
            }
            {
                IList<ReadElementObjectInfo> errors;
                dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
                Assert.That( errors.Count == 0 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );

                dicFullFrag.Destroy( this );
                Assert.That( dicFullFrag.GetSkippedFragments( this ) == null, "Destroying the object destroyed the fragments." );
            }

            {
                IList<ReadElementObjectInfo> errors;
                dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
                Assert.That( errors.Count == 0 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );

                // If we Ensure() the id, then the fragment will be restored.
                // We test here the ClearFragments internal.
                dicFullFrag.ClearFragments( uid1 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 2, "Destroying the fragment explicitely." );

                dicFullFrag.Destroy( uid2 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 1, "Destroying the fragment explicitely." );

                dicFullFrag.Destroy( uid3 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ) == null, "Destroying the fragment explicitely." );
            }

            {
                IList<ReadElementObjectInfo> errors;
                dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
                Assert.That( errors.Count == 0 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );

                // This destroy an unknown plugin: 
                // we must destroy the fragments even if the id is not known.
                dicFullFrag.Destroy( uid2 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 2, "Destroying the fragment via an unknown Id." );

                dicFullFrag.Destroy( uid3 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 1, "Destroying the fragment via an unknown Id." );

                // No op.
                dicFullFrag.Destroy( uid3 );

                dicFullFrag.Destroy( uid1 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ) == null );
            }

            {
                IList<ReadElementObjectInfo> errors;
                dicFullFrag = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, this, out errors );
                Assert.That( errors.Count == 0 );
                Assert.That( dicFullFrag.GetSkippedFragments( this ).Count == 3 );

                dicFullFrag.ClearAll();

                Assert.That( dicFullFrag.GetSkippedFragments( this ) == null );
            }
        }

        [Test]
        public void TryReloadNestedSkippedFragments()
        {
            TryReloadNestedSkippedFragments( new StructuredSerializableObject() { Value = 10 } );
        }

        [Test]
        public void TryReloadNestedSkippedFragmentsOnObjectTypes()
        {
            TryReloadNestedSkippedFragments( new BinarySerializableObject() { Value = 10 } );
        }

        [Test]
        public void TryReloadNestedSkippedFragmentsOnXmlSerializableTypes()
        {
            TryReloadNestedSkippedFragments( new XmlSerializableObject() { Value = 10 } );
        }

        void TryReloadNestedSkippedFragments( ITestObject complexObject )
        {
            object rootObject = new object();

            SimpleNamedVersionedUniqueId p1 = new SimpleNamedVersionedUniqueId( Guid.NewGuid(), new Version( 1, 0, 0 ), "plugin1" );
            SimpleNamedVersionedUniqueId p2 = new SimpleNamedVersionedUniqueId( Guid.NewGuid(), new Version( 1, 0, 0 ), "plugin2" );

            string path = TestBase.GetTestFilePath( "SharedDic", "NestedSkippedFragments" );

            // Write !
            {
                SharedDictionaryImpl dic = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
                dic[rootObject, p1, "complexObject"] = complexObject;
                dic[complexObject, p2, "subKey"] = "subValue";

                SharedDicTestContext.Write( "Test", path, dic, rootObject );

                TestBase.DumpFileToConsole( path );
            }
            // Read nothing then p1 and p2
            {
                IList<ReadElementObjectInfo> errors = new List<ReadElementObjectInfo>();
                SharedDictionaryImpl dic = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, rootObject, out errors );

                dic.Ensure( p1 );
                dic.Ensure( p2 );

                CheckAllDataLoaded( rootObject, complexObject, p1, p2, dic );
            }
            // Read nothing then p2 and p1
            {
                IList<ReadElementObjectInfo> errors = new List<ReadElementObjectInfo>();
                SharedDictionaryImpl dic = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, rootObject, out errors );

                dic.Ensure( p2 );
                dic.Ensure( p1 );

                CheckAllDataLoaded( rootObject, complexObject, p1, p2, dic );
            }
            // Ensure p1 then read p2
            {
                IList<ReadElementObjectInfo> errors = new List<ReadElementObjectInfo>();
                SharedDictionaryImpl dic = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, rootObject, d => d.Ensure( p1 ), out errors );

                dic.Ensure( p2 );

                CheckAllDataLoaded( rootObject, complexObject, p1, p2, dic );
            }
            // Ensure p2 then read p1
            {
                IList<ReadElementObjectInfo> errors = new List<ReadElementObjectInfo>();
                SharedDictionaryImpl dic = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, rootObject, d => d.Ensure( p2 ), out errors );

                dic.Ensure( p1 );

                CheckAllDataLoaded( rootObject, complexObject, p1, p2, dic );
            }
            // Ensure p1 and p2 then read nothing
            {
                IList<ReadElementObjectInfo> errors = new List<ReadElementObjectInfo>();
                SharedDictionaryImpl dic = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, rootObject, d => { d.Ensure( p1 ); d.Ensure( p2 ); }, out errors );

                CheckAllDataLoaded( rootObject, complexObject, p1, p2, dic );
            }
            // Ensure p2 and p1 then read nothing
            {
                IList<ReadElementObjectInfo> errors = new List<ReadElementObjectInfo>();
                SharedDictionaryImpl dic = (SharedDictionaryImpl)SharedDicTestContext.Read( "Test", path, rootObject, d => { d.Ensure( p2 ); d.Ensure( p1 ); }, out errors );

                CheckAllDataLoaded( rootObject, complexObject, p1, p2, dic );
            }
        }

        private static void CheckAllDataLoaded( object rootObject, ITestObject complexObject, SimpleNamedVersionedUniqueId p1, SimpleNamedVersionedUniqueId p2, SharedDictionaryImpl dic )
        {
            ITestObject obj = (ITestObject)dic[rootObject, p1, "complexObject"];
            Assert.That( obj != null );
            Assert.That( obj.Value == complexObject.Value );
            string value = (string)dic[obj, p2, "subKey"];
            Assert.That( value == "subValue" );
        }

        SharedDictionaryImpl CreateDummySharedDic( object dataHolder, params INamedVersionedUniqueId[] identifiers )
        {
            SharedDictionaryImpl dic = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            foreach( INamedVersionedUniqueId id in identifiers )
            {
                dic[dataHolder, id, "key1"] = "value1";
                dic[dataHolder, id, "key2"] = "value2";
            }
            return dic;
        }
    }

    #region Classes and interfaces used for the "TryReloadNestedSkippedFragments" tests

    public interface ITestObject
    {
        int Value { get; set; }
    }

    public class StructuredSerializableObject : IStructuredSerializable, ITestObject
    {
        public int Value { get; set; }

        public void ReadContent( IStructuredReader sr )
        {
            sr.Xml.Read();
            sr.Xml.ReadStartElement( "Value" );
            Value = sr.Xml.ReadContentAsInt();
            sr.Xml.ReadEndElement();
        }

        public void WriteContent( IStructuredWriter sw )
        {
            sw.Xml.WriteStartElement( "Value" );
            sw.Xml.WriteValue( Value );
            sw.Xml.WriteEndElement();
        }
    }

    [Serializable]
    public class BinarySerializableObject : ITestObject
    {
        public int Value { get; set; }
    }

    public class XmlSerializableObject : IXmlSerializable, ITestObject
    {
        public int Value { get; set; }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml( System.Xml.XmlReader reader )
        {
            reader.Read();
            reader.ReadStartElement( "Value" );
            Value = reader.ReadContentAsInt();
            reader.ReadEndElement();
        }

        public void WriteXml( System.Xml.XmlWriter writer )
        {
            writer.WriteStartElement( "Value" );
            writer.WriteValue( Value );
            writer.WriteEndElement();
        }
    }

    #endregion

}

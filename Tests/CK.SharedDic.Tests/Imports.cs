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

namespace SharedDic
{
    public class MergeableObject : IMergeable
    {
        public int Property { get; set; }

        public bool Merge( object source, IServiceProvider services )
        {
            MergeableObject mSource = source as MergeableObject;
            if( mSource != null )
            {
                Property += mSource.Property;
                return true;
            }
            return false;
        }
    }

    [TestFixture]
    public class Imports
    {

        [Test]
        public void None()
        {
            SharedDictionaryImpl source = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            source[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            source[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            source[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";

            object key = new object();
            SharedDictionaryImpl target = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            target[key, SharedDicTestContext.Plugins[1], "previousKey"] = "previousValue";

            target.Import( source, MergeMode.None );

            Assert.IsNull( target[key, SharedDicTestContext.Plugins[1], "previousKey"] );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key1"] == "value1" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key2"] == "value2" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key3"] == "value3" );
        }

        [Test]
        public void PreserveExisting()
        {
            object key = new object();

            SharedDictionaryImpl source = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            source[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            source[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            source[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            source[key, SharedDicTestContext.Plugins[1], "previousKey"] = "value";

            SharedDictionaryImpl target = new SharedDictionaryImpl( null );
            target[key, SharedDicTestContext.Plugins[1], "previousKey"] = "previousValue";

            target.Import( source, MergeMode.PreserveExisting );

            Assert.That( (string)target[key, SharedDicTestContext.Plugins[1], "previousKey"] == "previousValue" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key1"] == "value1" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key2"] == "value2" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key3"] == "value3" );
        }

        [Test]
        public void ReplaceExisting()
        {
            object key = new object();

            SharedDictionaryImpl source = new SharedDictionaryImpl( null );
            source[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            source[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            source[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            source[key, SharedDicTestContext.Plugins[1], "previousKey"] = "value";

            SharedDictionaryImpl target = new SharedDictionaryImpl( null );
            target[key, SharedDicTestContext.Plugins[1], "previousKey"] = "previousValue";

            target.Import( source, MergeMode.ReplaceExisting );

            Assert.That( (string)target[key, SharedDicTestContext.Plugins[1], "previousKey"] == "value" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key1"] == "value1" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key2"] == "value2" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key3"] == "value3" );
        }

        [Test]
        public void ReplaceExistingTryMerge()
        {
            object key = new object();

            SharedDictionaryImpl source = new SharedDictionaryImpl( SharedDicTestContext.ServiceProvider );
            source[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            source[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            source[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            source[this, SharedDicTestContext.Plugins[0], "key4"] = "newValue";

            MergeableObject mergeableObj = new MergeableObject() { Property = 10 };
            source[key, SharedDicTestContext.Plugins[1], "mergeableObject"] = mergeableObj;

            SharedDictionaryImpl target = new SharedDictionaryImpl( null );
            MergeableObject newMergeableObj = new MergeableObject() { Property = 20 };
            target[key, SharedDicTestContext.Plugins[1], "mergeableObject"] = newMergeableObj;
            target[this, SharedDicTestContext.Plugins[0], "key4"] = "value4";

            target.Import( source, MergeMode.ReplaceExistingTryMerge );

            Assert.That( ((MergeableObject)target[key, SharedDicTestContext.Plugins[1], "mergeableObject"]).GetHashCode() == newMergeableObj.GetHashCode() );
            Assert.That( ((MergeableObject)target[key, SharedDicTestContext.Plugins[1], "mergeableObject"]).Property == 30 );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key1"] == "value1" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key2"] == "value2" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key3"] == "value3" );
            Assert.That( (string)target[this, SharedDicTestContext.Plugins[0], "key4"] == "newValue" );
        }
    }
}

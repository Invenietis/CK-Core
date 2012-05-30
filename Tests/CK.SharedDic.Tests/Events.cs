#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\Events.cs) is part of CiviKey. 
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

namespace SharedDic
{
    [TestFixture]
    public class Events
    {
        SharedDictionaryImpl _dic;
        ConfigChangedEventArgs _lastConfigChangedEventArgs = null;
        object _lastSender = null;

        [SetUp]
        public void Setup()
        {
            _lastConfigChangedEventArgs = null;
            _lastSender = null;

            _dic = new SharedDictionaryImpl( null );
            _dic.Changed += ( o, e ) => { _lastSender = o; _lastConfigChangedEventArgs = e; };
        }

        [Test]
        public void DirectAdd()
        {
            _dic.Add( this, SharedDicTestContext.Plugins[0], "key1", "value1" );
            Assert.IsNotNull( _lastConfigChangedEventArgs );
            Assert.IsNotNull( _lastSender );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key1" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Add );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "value1" );

            _dic[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key2" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Add );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "value2" );

            _dic.GetOrSet( this, SharedDicTestContext.Plugins[0], "key3", "value3" );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key3" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Add );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "value3" );
        }

        [Test]
        public void FinalDicAdd()
        {
            IObjectPluginConfig finalDic = GetFinalDic( this, SharedDicTestContext.Plugins[0] );

            finalDic.Add( "key1", "value1" );
            Assert.IsNotNull( _lastConfigChangedEventArgs );
            Assert.IsNotNull( _lastSender );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key1" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Add );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "value1" );

            finalDic["key2"] = "value2";
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key2" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Add );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "value2" );

            finalDic.GetOrSet( "key3", "value3" );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key3" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Add );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "value3" );
        }

        [Test]
        public void DirectUpdate()
        {
            _dic.Add( this, SharedDicTestContext.Plugins[0], "key2", "value2" );

            _dic[this, SharedDicTestContext.Plugins[0], "key2"] = "NewValue2";
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key2" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Update );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "NewValue2" );

            _lastConfigChangedEventArgs = null;

            _dic.GetOrSet( this, SharedDicTestContext.Plugins[0], "key2", "NewNewValue2" );
            Assert.IsNull( _lastConfigChangedEventArgs );

            Assert.Throws<ArgumentNullException>( () => _dic.GetOrSet<bool>( this, SharedDicTestContext.Plugins[0], "key2", true, null ) );
            _dic.GetOrSet<bool>( this, SharedDicTestContext.Plugins[0], "key2", true, ( o ) => { return true; } );
            Assert.IsNotNull( _lastConfigChangedEventArgs );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key2" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Update );
            Assert.That( (bool)_lastConfigChangedEventArgs.Value == true );
        }

        [Test]
        public void FinalDicUpdate()
        {
            IObjectPluginConfig finalDic = GetFinalDic( this, SharedDicTestContext.Plugins[0] );
            _dic.Add( this, SharedDicTestContext.Plugins[0], "key2", "value2" );

            finalDic["key2"] = "NewValue2";
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key2" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Update );
            Assert.That( (string)_lastConfigChangedEventArgs.Value == "NewValue2" );

            _lastConfigChangedEventArgs = null;

            finalDic.GetOrSet( "key2", "NewNewValue2" );
            Assert.IsNull( _lastConfigChangedEventArgs );

            Assert.Throws<ArgumentNullException>( () => finalDic.GetOrSet<bool>( "key2", true, null ) );
            finalDic.GetOrSet<bool>( "key2", true, ( o ) => { return true; } );
            Assert.IsNotNull( _lastConfigChangedEventArgs );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key2" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Contains( this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Update );
            Assert.That( (bool)_lastConfigChangedEventArgs.Value == true );
        }

        [Test]
        public void DirectRemove()
        {
            _dic.Add( this, SharedDicTestContext.Plugins[0], "key1", "value1" );

            _dic.Remove( this, SharedDicTestContext.Plugins[0], "key1" );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key1" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Delete );
            Assert.IsNull( _lastConfigChangedEventArgs.Value );
        }

        [Test]
        public void FinalDicRemove()
        {
            IObjectPluginConfig finalDic = GetFinalDic( this, SharedDicTestContext.Plugins[0] );
            _dic.Add( this, SharedDicTestContext.Plugins[0], "key1", "value1" );

            finalDic.Remove( "key1" );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.That( _lastConfigChangedEventArgs.Key == "key1" );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );            
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.Delete );
            Assert.IsNull( _lastConfigChangedEventArgs.Value );
        }

        [Test]
        public void DestroyKeyObject()
        {
            _dic[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            _dic[this, SharedDicTestContext.Plugins[1], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[1], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[1], "key3"] = "value3";

            _dic.Destroy( this );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.IsNull( _lastConfigChangedEventArgs.Key );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 2 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[1]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.ContainerDestroy );
            Assert.IsNull( _lastConfigChangedEventArgs.Value );
        }

        [Test]
        public void DestroyPluginId()
        {
            object otherKey = new object();

            _dic[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key3"] = "value3";

            _dic.Destroy( SharedDicTestContext.Plugins[0] );
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.IsNull( _lastConfigChangedEventArgs.Key );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 2 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Any( ( o ) => { return o == this; } ) );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Any( ( o ) => { return o == otherKey; } ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.IsNull( _lastConfigChangedEventArgs.Obj );
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.ContainerDestroy );
            Assert.IsNull( _lastConfigChangedEventArgs.Value );
        }

        [Test]
        public void ClearAll()
        {
            object otherKey = new object();

            _dic[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            _dic[this, SharedDicTestContext.Plugins[1], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[1], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[1], "key3"] = "value3";
            _dic[otherKey, SharedDicTestContext.Plugins[1], "key1"] = "value1";
            _dic[otherKey, SharedDicTestContext.Plugins[1], "key2"] = "value2";
            _dic[otherKey, SharedDicTestContext.Plugins[1], "key3"] = "value3";

            _dic.DestroyAll();
            Assert.That( _lastConfigChangedEventArgs.IsAllConcerned );
            Assert.IsNull( _lastConfigChangedEventArgs.Key );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 2 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Any( o => o == this ) );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Any( o => o == otherKey ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 2 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( p => p == SharedDicTestContext.Plugins[0] ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( p => p == SharedDicTestContext.Plugins[1] ) );
            Assert.IsNull( _lastConfigChangedEventArgs.Obj );
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.ContainerDestroy );
            Assert.IsNull( _lastConfigChangedEventArgs.Value );
        }

        [Test]
        public void FinalDicClear()
        {
            object otherKey = new object();

            _dic[this, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key1"] = "value1";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key2"] = "value2";
            _dic[otherKey, SharedDicTestContext.Plugins[0], "key3"] = "value3";
            _dic[this, SharedDicTestContext.Plugins[1], "key1"] = "value1";
            _dic[this, SharedDicTestContext.Plugins[1], "key2"] = "value2";
            _dic[this, SharedDicTestContext.Plugins[1], "key3"] = "value3";
            _dic[otherKey, SharedDicTestContext.Plugins[1], "key1"] = "value1";
            _dic[otherKey, SharedDicTestContext.Plugins[1], "key2"] = "value2";
            _dic[otherKey, SharedDicTestContext.Plugins[1], "key3"] = "value3";

            IObjectPluginConfig finalDicThis1 = GetFinalDic( this, SharedDicTestContext.Plugins[0] );
            IObjectPluginConfig finalDicThis2 = GetFinalDic( this, SharedDicTestContext.Plugins[1] );
            IObjectPluginConfig finalDicOther1 = GetFinalDic( this, SharedDicTestContext.Plugins[0] );
            IObjectPluginConfig finalDicOther2 = GetFinalDic( this, SharedDicTestContext.Plugins[1] );

            finalDicThis1.Clear();
            Assert.That( !_lastConfigChangedEventArgs.IsAllConcerned );
            Assert.IsNull( _lastConfigChangedEventArgs.Key );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiObj.Any( ( o ) => { return o == this; } ) );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Count == 1 );
            Assert.That( _lastConfigChangedEventArgs.MultiPluginId.Any( ( p ) => { return p == SharedDicTestContext.Plugins[0]; } ) );
            Assert.That( _lastConfigChangedEventArgs.Obj == this );
            Assert.That( _lastConfigChangedEventArgs.Status == ChangeStatus.ContainerClear );
            Assert.IsNull( _lastConfigChangedEventArgs.Value );
        }

        IObjectPluginConfig GetFinalDic( object key, IUniqueId id )
        {
            IObjectPluginConfig finalDic = _dic.GetObjectPluginConfig( this, SharedDicTestContext.Plugins[0] );
            Assert.IsNotNull( finalDic );
            return finalDic;
        }
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\Helpers\SharedDictionaryTester.cs) is part of CiviKey. 
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
using System.IO;
using CK.Storage;
using System.Xml;
using NUnit.Framework;
using System.Collections.ObjectModel;
using CK.SharedDic;
using CK.Core;
using CK.Plugin.Config;

namespace SharedDic
{
    /// <summary>
    /// This class encapsulates the creation of a dictionary and write/read operations on it.
    /// It automatically adds and checks random integer properties.
    /// When data element are written in the Xml stream, they follow their creation order: we use this
    /// "unwanted feature" to insert the tested elements (built by the delegate in the constructor)
    /// inside the random properties.
    /// This enables testing the behavior of the Reader/Writer when errors occur in the stream: random properties 
    /// must be correclty (if possible) read even if an error occurs for one of the element.
    /// </summary>
    public class SharedDictionaryTester
    {
        ISharedDictionary _dic;
        int _seed;
        int _seed2;
        int _seed3;
        object _o;
        INamedVersionedUniqueId _uid1;
        INamedVersionedUniqueId _uid2;
        IList<ReadElementObjectInfo> _errors;
        ISharedDictionary _dicRead;
        string _testName;
        string _path;

        public SharedDictionaryTester( string testName, string path, object o, INamedVersionedUniqueId uid1, INamedVersionedUniqueId uid2, Action<ISharedDictionary, object, INamedVersionedUniqueId> f )
        {
            _errors = new List<ReadElementObjectInfo>();
            _testName = testName;
            _path = path;
            _seed = Environment.TickCount;
            _o = o;
            _uid1 = uid1;
            _uid2 = uid2;
            _dic = SharedDictionary.Create( null );
            _seed2 = GenerateRandomProperties( _dic, _o, _uid1, 10, _seed );
            f( _dic, _o, _uid1 );
            _seed3 = GenerateRandomProperties( _dic, _o, _uid1, 10, _seed2 );
            GenerateRandomProperties( _dic, _o, _uid2, 20, _seed3 );
        }

        public IList<ReadElementObjectInfo> Errors
        {
            get { return _errors; }
        }

        public ISharedDictionary WriteAndReadWithTests( Action<XmlDocument> afterWrite, Action<ISharedDictionary> beforeRead )
        {
            SharedDicTestContext.Write( _testName, _path, _dic, _o, afterWrite );
            _dicRead = SharedDicTestContext.Read( _testName, _path, _o, beforeRead, out _errors );
            if( _dicRead.Contains( _uid1 ) )
            {
                CheckRandomProperties( _dicRead, _o, _uid1, 10, _seed );
                CheckRandomProperties( _dicRead, _o, _uid1, 10, _seed2 );
            }
            if( _dicRead.Contains( _uid2 ) ) CheckRandomProperties( _dicRead, _o, _uid2, 20, _seed3 );
            return _dicRead;
        }

        private int GenerateRandomProperties( ISharedDictionary dic, object o, INamedVersionedUniqueId g, int count, int seed )
        {
            Random r = new Random( seed );
            StringBuilder b = new StringBuilder( 10 );
            while( --count > 0 )
            {
                b.Length = 0;
                for( int j = 0; j < 10; ++j ) b.Append( (char)(r.Next( 26 ) + 'A') );
                dic[o, g, b.ToString()] = count;
            }
            return r.Next();
        }

        private static int CheckRandomProperties( ISharedDictionary dic, object o, INamedVersionedUniqueId g, int count, int seed )
        {
            Random r = new Random( seed );
            StringBuilder b = new StringBuilder( 10 );
            while( --count > 0 )
            {
                b.Length = 0;
                for( int j = 0; j < 10; ++j ) b.Append( (char)(r.Next( 26 ) + 'A') );
                Assert.AreEqual( count, dic[o, g, b.ToString()] );
            }
            return r.Next();
        }
    }


    [Serializable]
    public class SerializableObject : IEquatable<SerializableObject>
    {
        public int Power;
        public string Name = String.Empty;

        public bool Equals(SerializableObject other)
        {
            return other != null && Name == other.Name && Power == other.Power;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SerializableObject);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Power;
        }

    }
}

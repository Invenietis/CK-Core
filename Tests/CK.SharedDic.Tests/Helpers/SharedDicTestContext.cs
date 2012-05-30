#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\Helpers\SharedDicTestContext.cs) is part of CiviKey. 
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
using CK.Core;
using CK.SharedDic;
using System.Diagnostics;
using CK.Plugin.Config;
using System.IO;
using CK.Storage;
using System.Xml;

namespace SharedDic
{
    public static class SharedDicTestContext
    {
        static List<INamedVersionedUniqueId> _allPlugins;
        static IReadOnlyList<INamedVersionedUniqueId> _allPluginsEx;

        static IServiceProvider _serviceProvider;

        static SharedDicTestContext()
        {
            _allPlugins = new List<INamedVersionedUniqueId>();
            _allPluginsEx = new ReadOnlyListOnIList<INamedVersionedUniqueId>( _allPlugins );
            EnsurePlugins( 10 );

            var c = new SimpleServiceContainer();
            c.Add<ISimpleTypeFinder>( SimpleTypeFinder.Default );
            _serviceProvider = c;
        }

        // Initialized with the ISimpleTypeFinder.Default.
        public static IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        public static IReadOnlyList<INamedVersionedUniqueId> Plugins { get { return _allPluginsEx; } }

        public static void ClearPlugins()
        {
            _allPlugins.Clear();
        }

        public static void EnsurePlugins( int nbPlugin )
        {
            int missing = nbPlugin - _allPlugins.Count;
            while( --missing >= 0 )
            {
                _allPlugins.Add( new SimpleNamedVersionedUniqueId( Guid.NewGuid(), new Version( 1, 0, 0, 0 ), String.Format( "Plugin n°{0}", _allPlugins.Count ) ) );
            }
        }

        public static ISharedDictionary Read( string testName, string path, object o, out IList<ReadElementObjectInfo> errors )
        {
            return Read( testName, path, o, null, out errors );
        }

        public static ISharedDictionary Read( string testName, string path, object o, Action<ISharedDictionary> beforeRead, out IList<ReadElementObjectInfo> errors )
        {
            ISharedDictionary dicRead = SharedDictionary.Create( ServiceProvider );
            if( beforeRead != null ) beforeRead( dicRead );
            using( Stream str = new FileStream( path, FileMode.Open ) )
            {
                using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, ServiceProvider ) )
                {
                    using( ISharedDictionaryReader r = dicRead.RegisterReader( sr, MergeMode.None ) )
                    {
                        r.ReadPluginsDataElement( testName, o );
                        errors = r.ErrorCollector;
                    }
                }
            }
            return dicRead;
        }

        public static void Write( string testName, string path, ISharedDictionary dic, object o )
        {
            Write( testName, path, dic, o, null );
        }

        public static void Write( string testName, string path, ISharedDictionary dic, object o, Action<XmlDocument> afterWrite )
        {
            using( Stream wrt = new FileStream( path, FileMode.Create ) )
            {
                using( IStructuredWriter sw = SimpleStructuredWriter.CreateWriter( wrt, ServiceProvider ) )
                {
                    using( ISharedDictionaryWriter w = dic.RegisterWriter( sw ) )
                    {
                        w.WritePluginsDataElement( testName, o );
                    }
                }
            }
            if( afterWrite != null )
            {
                XmlDocument d = new XmlDocument();
                d.Load( path );
                afterWrite( d );
                d.Save( path );
            }
        }
        
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Discoverer.cs) is part of CiviKey. 
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
using NUnit.Framework;
using CK.Core;
using CK.Plugin;
using System.Linq;
using CK.Plugin.Discoverer;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Discoverer
{
	[TestFixture]
	public partial class Discoverer
	{
        [SetUp]
        [TearDown]
        public void CleanupTestFolderDir()
        {
            TestBase.CleanupTestDir();
        }

		[Test]
		public void TestDiscoverNothing()
		{
            PluginDiscoverer discoverer = new PluginDiscoverer();

            bool discoverBegin = false;
            bool discoverDone = false;

            discoverer.DiscoverBegin += ( object sender, EventArgs e ) => discoverBegin = true;
            discoverer.DiscoverDone += ( object sender, DiscoverDoneEventArgs e ) => discoverDone = true;

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.IsTrue( discoverDone && discoverBegin );

            Assert.That( discoverer.AllAssemblies.Count == 0 );
            Assert.That( discoverer.PluginOrServiceAssemblies.Count == 0 );
		}

        [Test]
        public void TestDiscoverOnePlugin()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.AllAssemblies.Count == 1 );
            Assert.That( discoverer.PluginOrServiceAssemblies.Count == 1 );
            Assert.That( discoverer.Plugins.Count == 1 );
            Assert.That( discoverer.Services.Count == 1 );

            //Test methods, events and properties info :
            IReadOnlyCollection<ISimpleEventInfo> events = discoverer.Services.First().EventsInfoCollection;
            IReadOnlyCollection<ISimpleMethodInfo> methods = discoverer.Services.First().MethodsInfoCollection;
            IReadOnlyCollection<ISimplePropertyInfo> properties = discoverer.Services.First().PropertiesInfoCollection;

            Assert.That( events.Count, Is.EqualTo( 1 ) );
            Assert.That( methods.Count, Is.EqualTo( 2 ) );
            Assert.That( properties.Count, Is.EqualTo( 1 ) );

            Assert.That( events.First().Name, Is.EqualTo( "HasStarted" ) );

            Assert.That( methods.First().Name, Is.EqualTo( "Add" ) );
            Assert.That( methods.First().Parameters.Count, Is.EqualTo( 2 ) );
            Assert.That( methods.First().Parameters.First().ParameterName, Is.EqualTo( "a" ) );
            Assert.That( methods.First().Parameters.Last().ParameterName, Is.EqualTo( "b" ) );
            Assert.That( methods.First().ReturnType, Is.EqualTo( "System.Int32" ) );

            Assert.That( properties.First().Name, Is.EqualTo( "HasBeenStarted" ) );
            Assert.That( properties.First().PropertyType, Is.EqualTo( "System.Boolean" ) );
        }

        [Test]
        public void TestDiscoverTwoPlugins()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();
            
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.AllAssemblies.Count == 2 );
            Assert.That( discoverer.PluginOrServiceAssemblies.Count == 2 );
            Assert.That( discoverer.Plugins.Count == 2 );
            Assert.That( discoverer.Services.Count == 2 );

            foreach( IServiceInfo s in discoverer.Services )
            {
                if( s.IsDynamicService )
                {
                    Assert.That( s.AssemblyInfo != null );
                    Assert.That( s.AssemblyQualifiedName != "" );
                }
            }

            //Test methods, events and properties info of service A
            IReadOnlyCollection<ISimpleEventInfo> eventsA = discoverer.Services.First().EventsInfoCollection;
            IReadOnlyCollection<ISimpleMethodInfo> methodsA = discoverer.Services.First().MethodsInfoCollection;
            IReadOnlyCollection<ISimplePropertyInfo> propertiesA = discoverer.Services.First().PropertiesInfoCollection;

            Assert.That( eventsA.Count, Is.EqualTo( 1 ) );
            Assert.That( methodsA.Count, Is.EqualTo( 2 ) );
            Assert.That( propertiesA.Count, Is.EqualTo( 1 ) );

            Assert.That( eventsA.First().Name, Is.EqualTo( "HasStarted" ) );

            Assert.That( methodsA.First().Name, Is.EqualTo( "Add" ) );
            Assert.That( methodsA.First().Parameters.Count, Is.EqualTo( 2 ) );
            Assert.That( methodsA.First().Parameters.First().ParameterName, Is.EqualTo( "a" ) );
            Assert.That( methodsA.First().Parameters.Last().ParameterName, Is.EqualTo( "b" ) );
            Assert.That( methodsA.First().ReturnType, Is.EqualTo( "System.Int32" ) );

            Assert.That( propertiesA.First().Name, Is.EqualTo( "HasBeenStarted" ) );
            Assert.That( propertiesA.First().PropertyType, Is.EqualTo( "System.Boolean" ) );


            //Test methods, events and properties info of service B
            IReadOnlyCollection<ISimpleEventInfo> eventsB = discoverer.Services.ElementAt( 1 ).EventsInfoCollection;
            IReadOnlyCollection<ISimpleMethodInfo> methodsB = discoverer.Services.ElementAt( 1 ).MethodsInfoCollection;
            IReadOnlyCollection<ISimplePropertyInfo> propertiesB = discoverer.Services.ElementAt( 1 ).PropertiesInfoCollection;

            Assert.That( eventsB.Count, Is.EqualTo( 0 ) );
            Assert.That( methodsB.Count, Is.EqualTo( 2 ) );
            Assert.That( propertiesA.Count, Is.EqualTo( 1 ) );

            Assert.That( methodsB.First().Name, Is.EqualTo( "Mult" ) );
            Assert.That( methodsB.First().Parameters.Count, Is.EqualTo( 2 ) );
            Assert.That( methodsB.First().Parameters.First().ParameterName, Is.EqualTo( "a" ) );
            Assert.That( methodsB.First().Parameters.Last().ParameterName, Is.EqualTo( "b" ) );
            Assert.That( methodsB.First().ReturnType, Is.EqualTo( "System.Int32" ) );

            Assert.That( methodsB.ElementAt( 1 ).Name, Is.EqualTo( "Substract" ) );
            Assert.That( methodsB.ElementAt( 1 ).Parameters.Count, Is.EqualTo( 3 ) );
            Assert.That( methodsB.ElementAt( 1 ).Parameters.First().ParameterName, Is.EqualTo( "a" ) );
            Assert.That( methodsB.ElementAt( 1 ).Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "b" ) );
            Assert.That( methodsB.ElementAt( 1 ).Parameters.Last().ParameterName, Is.EqualTo( "isAboveZero" ) );
            Assert.That( methodsB.ElementAt( 1 ).ReturnType, Is.EqualTo( "System.Int32" ) );

            Assert.That( propertiesB.First().Name, Is.EqualTo( "HasBeenStarted" ) );
            Assert.That( propertiesB.First().PropertyType, Is.EqualTo( "System.Boolean" ) );
        }

        [Test]
        public void TestDiscoverEditors()
        {
            int nbEditors = 0;

            PluginDiscoverer discoverer = new PluginDiscoverer();
            
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            TestBase.CopyPluginToTestDir( "EditorsOfPlugins.dll" );
            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.AllAssemblies.Count == 3 );
            Assert.That( discoverer.PluginOrServiceAssemblies.Count == 3 );
            Assert.That( discoverer.Plugins.Count, Is.EqualTo( 6 ) );

            foreach( IPluginInfo plugin in discoverer.Plugins )
            {
                nbEditors += plugin.EditorsInfo.Count;
            }

            Assert.That( nbEditors, Is.EqualTo( 6 ) ); // 6 because ServiceA get an accessor to its configuration.

            IPluginInfo editorOfTwoPlugins = discoverer.FindPlugin( new Guid( "{BC7E641B-E4EE-47f5-833B-0AFFFAA2A683}" ) );
            Assert.That( editorOfTwoPlugins.EditorsInfo.Any( ( a ) => { return (a.ConfigurationPropertyName == "ConfigPlugin01") && (a.IsConfigurationPropertyValid); } ) );
            Assert.That( editorOfTwoPlugins.EditorsInfo.Any( ( a ) => { return (a.ConfigurationPropertyName == "ConfigPlugin02") && (a.IsConfigurationPropertyValid); } ) );
        }

        [Test]
        public void TestCollectionEditableBy()
        {
            int passed = 0;
            PluginDiscoverer discoverer = new PluginDiscoverer();
            
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            TestBase.CopyPluginToTestDir( "EditorsOfPlugins.dll" );
            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.Plugins.Count, Is.EqualTo( 6 ) );
            foreach( IPluginInfo plugin in discoverer.Plugins )
            {
                if( plugin.PluginId == new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" ) ) //Plugin01
                {
                    Assert.That( plugin.EditableBy.Count, Is.EqualTo( 3 ) );
                    // Edited by EditorOfPlugin01
                    Assert.That( plugin.EditableBy.Any(
                        ( editor ) => { return editor.Plugin.PluginId == new Guid( "{8BE8C487-4052-46b0-9225-708BC2A1E033}" ); } ) );
                    // Edited by EditorOfPlugin01And02
                    Assert.That( plugin.EditableBy.Any(
                        ( editor ) => { return editor.Plugin.PluginId == new Guid( "{BC7E641B-E4EE-47f5-833B-0AFFFAA2A683}" ); } ) );
                    // Edited by itself
                    Assert.That( plugin.EditableBy.Any(
                        ( editor ) => { return editor.Plugin.PluginId == new Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" ); } ) );
                    
                    passed++;
                }
                else if( plugin.PluginId == new Guid( "{E64F17D5-DCAB-4a07-8CEE-901A87D8295E}" ) ) //Plugin02
                {
                    Assert.That( plugin.EditableBy.Count, Is.EqualTo( 2 ) );
                    // Edited by EditorOfPlugin02
                    Assert.That( plugin.EditableBy.Any(
                        ( editor ) => { return editor.Plugin.PluginId == new Guid( "{EDA76E35-30C2-449e-817C-91CB24D38763}" ); } ) );
                    // Edited by EditorOfPlugin01And02
                    Assert.That( plugin.EditableBy.Any(
                        ( editor ) => { return editor.Plugin.PluginId == new Guid( "{BC7E641B-E4EE-47f5-833B-0AFFFAA2A683}" ); } ) );
                    passed++;
                }
            }
            Assert.That( passed, Is.EqualTo( 2 ) );
        }

        [Test]
        public void TestDiscoverOldVersionnedPlugins()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();
            
            TestBase.CopyPluginToTestDir( "VersionedPlugins.dll" );
            TestBase.CopyPluginToTestDir( "VersionedPluginWithService.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.AllAssemblies.Count, Is.EqualTo( 2 ) );
            Assert.That( discoverer.AllPlugins.Count, Is.EqualTo( 4 ) );
            Assert.That( discoverer.AllServices.Count, Is.EqualTo( 1 ) );
            Assert.That( discoverer.Services.Count, Is.EqualTo( 1 ) );
            Assert.That( discoverer.OldVersionnedPlugins.Count, Is.EqualTo( 2 ) );
            Assert.That( discoverer.Plugins.Count == 2 );

            Assert.That( discoverer.Services.ElementAt( 0 ).Implementations.Count, Is.EqualTo( 1 ) ); // the best version of the implementation
            Assert.That( !discoverer.Services.ElementAt( 0 ).Implementations.Any( ( p ) => discoverer.OldVersionnedPlugins.Contains( p ) ) );
        }

        [Test]
        public void TestRequireConfiguration()
        {
            int passed = 0;

            PluginDiscoverer discoverer = new PluginDiscoverer();
            
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            TestBase.CopyPluginToTestDir( "EditorsOfPlugins.dll" );
            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.Plugins.Count, Is.EqualTo( 6 ) );
            foreach( IPluginInfo plugin in discoverer.Plugins )
            {
                if( plugin.PluginId == new Guid( "{8BE8C487-4052-46b0-9225-708BC2A1E033}" ) ) //EditorOfPlugin01
                {
                    Assert.That( plugin.EditorsInfo.Count, Is.EqualTo( 1 ) );
                    Assert.That( plugin.EditorsInfo[0].ConfigurationPropertyName == "EditedPluginConfiguration" );
                    Assert.IsTrue( plugin.EditorsInfo[0].IsConfigurationPropertyValid );
                    passed++;
                }
                else if( plugin.PluginId == new Guid( "{EDA76E35-30C2-449e-817C-91CB24D38763}" ) ) //EditorOfPlugin02
                {
                    Assert.That( plugin.EditorsInfo.Count, Is.EqualTo( 1 ) );
                    Assert.IsTrue( plugin.EditorsInfo[0].IsConfigurationPropertyValid );
                    passed++;
                }
            }

            Assert.That( passed, Is.EqualTo( 2 ) );
        }

        [Test]
        public void DiscoverMultiServicesPlugin()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();            

            TestBase.CopyPluginToTestDir( "ServiceD.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.AllPlugins.Count == 1 );
            Assert.That( discoverer.AllPlugins.ElementAt( 0 ).HasError );
        }

        [Test]
        public void HomonymServices()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();

            TestBase.CopyPluginToTestDir( "HomonymServiceZ.dll" );
            TestBase.CopyPluginToTestDir( "HomonymServiceZbis.dll" );
            TestBase.CopyPluginToTestDir( "HomonymClassZ.dll" );
            TestBase.CopyPluginToTestDir( "HomonymClassZbis.dll" );

            List<object> objects = new List<object>();

            foreach ( FileInfo f in TestBase.TestFolderDir.GetFiles( "*.dll" ) )
            {
                objects.Add( Load( f.FullName ) );
            }

            //object[0] is a HomonymClassZ object
            //object[1] is a HomonymClassZbis object            

            //object[2] is a HomonymImplZ object
            //object[3] is a HomonymImplZbis object

            object objectZ = objects[0];
            object objectZbis = objects[1];
            object implZ = objects[2];
            object implZbis = objects[3];

            //Assert that a "Zbis" impl can't be set in the "Z" object's property
            Assert.Throws<System.Reflection.TargetException>( () => objectZ.GetType().GetProperty( "Service" ).SetValue( objectZbis, implZ, null ) );
            //Assert that a "Z" impl can't be set in the "Zbis" object's property
            Assert.Throws<System.Reflection.TargetException>( () => objectZbis.GetType().GetProperty( "Service" ).SetValue( objectZ, implZ, null ) );

            objectZ.GetType().GetProperty( "Service" ).SetValue( objectZ, implZ, null );
            objectZbis.GetType().GetProperty( "Service" ).SetValue( objectZbis, implZbis, null );

            object propertyZ = objectZ.GetType().GetProperty( "Service" ).GetValue( objectZ, null );
            object propertyZbis = objectZbis.GetType().GetProperty( "Service" ).GetValue( objectZbis, null );

            MethodInfo m = propertyZ.GetType().GetMethod( "GetNumber" );
            object[] argsZ = new object[2] { 0, 0 };
           
            Assert.That( m.Invoke( propertyZ, argsZ), Is.EqualTo( 2 ) );

            MethodInfo mbis = propertyZbis.GetType().GetMethod( "GetNumber" );
            object[] argsZbis = new object[2] { 0, 0 };

            Assert.That( mbis.Invoke( propertyZbis, argsZbis ), Is.EqualTo( 1 ) );
        }

        [Test]
        public void RefInternalNonDynamicService()
        {
            PluginDiscoverer discoverer = new PluginDiscoverer();

            TestBase.CopyPluginToTestDir( "RefInternalNonDynamicService.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( discoverer.Plugins.Count, Is.EqualTo( 2 ), "There are 2 runnable plugins." );
            Assert.That( discoverer.Plugins.Select( x => x.PublicName ).OrderBy( Util.FuncIdentity ).SequenceEqual( new[] { "PluginSuccess", "PluginSuccessAlso" } ) );

            Assert.That( discoverer.AllPlugins.Count, Is.EqualTo( 3 ), "There are 2 runnable plugins and 1 in error." );
            Assert.That( discoverer.AllPlugins.Any( p => p.PublicName == "PluginFailed" ) );
        }

        [Test]
        public void RefExternalNonDynamicService()
        {
            {
                // Missing ServiceC.Model.dll assembly: the interface has no definition.
                PluginDiscoverer discoverer = new PluginDiscoverer();
                TestBase.CopyPluginToTestDir( "RefExternalNonDynamicService.dll" );

                discoverer.Discover( TestBase.TestFolderDir, true );

                Assert.That( discoverer.AllAssemblies.Count, Is.EqualTo( 1 ) );
                Assert.That( discoverer.AllAssemblies.First().HasError, Is.True );

                Assert.That( discoverer.AllPlugins.Count, Is.EqualTo( 0 ) );
                Assert.That( discoverer.AllServices.Count, Is.EqualTo( 0 ) );
            }
            {
                PluginDiscoverer discoverer = new PluginDiscoverer();
                TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );

                discoverer.Discover( TestBase.TestFolderDir, true );

                Assert.That( discoverer.Plugins.Count, Is.EqualTo( 2 ), "There are 2 runnable plugins." );
                Assert.That( discoverer.Plugins.Select( x => x.PublicName ).OrderBy( Util.FuncIdentity ).SequenceEqual( new[] { "PluginSuccess", "PluginSuccessAlso" } ) );

                Assert.That( discoverer.AllPlugins.Count, Is.EqualTo( 3 ), "There are 2 runnable plugins and 1 in error." );
                Assert.That( discoverer.AllPlugins.Any( p => p.PublicName == "PluginFailed" ) );
            }

        }

        object Load( string assemblyName )
        {
            object o = null;
            try
            {
                Assembly asm = Assembly.LoadFrom( assemblyName );

                foreach ( Type t in asm.GetTypes() )
                {
                    if ( !t.IsInterface )
                    {
                        o = asm.CreateInstance( t.FullName );
                    }
                }
            }
            catch
            {
                Console.Out.WriteLine( "erreur au chargement de : " + assemblyName );
                return null;
            }

            return o;
        }
	}
}

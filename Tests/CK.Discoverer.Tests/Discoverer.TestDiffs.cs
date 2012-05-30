#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Discoverer.TestDiffs.cs) is part of CiviKey. 
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
using System.Reflection;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Discoverer;
using NUnit.Framework;

namespace Discoverer
{
    public partial class Discoverer
    {
        [Test]
        public void TestRediscoverOneAssembly()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer Discoverer = new CK.Plugin.Discoverer.PluginDiscoverer();

            Discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            Discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( Discoverer.AllAssemblies.Count, Is.EqualTo( 1 ) );
            Assert.That( Discoverer.AllPlugins.Count, Is.EqualTo( 1 ) );

            // Test methods, events and properties info (ServiceA.dll):
            {
                var events = Discoverer.Services.First().EventsInfoCollection;
                var methods = Discoverer.Services.First().MethodsInfoCollection;
                var properties = Discoverer.Services.First().PropertiesInfoCollection;

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

            // We add the new one: it has the same assembly name but not the same content (Plugin01 is kept as-is 
            // but the ServiceA.2 contains another plugin.
            // ==> Two "identical" assemblies are now available. 
            TestBase.CopyPluginToTestDir( @"ServiceA.2\ServiceA.dll" );
            Discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( Discoverer.AllAssemblies.Count, Is.EqualTo( 2 ) );
            Assert.That( Discoverer.AllAssemblies.Single( a => a.AssemblyFileName.Contains( "ServiceA.2" ) ).HasError, Is.True, "The 2nd assembly is on error: the other one has been discovered first." );

            Discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( lastDiscoverEventArgs.ChangeCount, Is.EqualTo( 0 ), "Discovering twice an assembly error (no bug, no change)." );

            // We remove the first one, and we discover again.
            TestBase.RemovePluginFromTestDir( @"ServiceA.dll" );
            Discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( lastDiscoverEventArgs.ChangeCount, Is.GreaterThan( 0 ), "There are changes." );

            Assert.That( Discoverer.AllAssemblies.Count, Is.EqualTo( 1 ), "The first assembly has been deleted." );
            Assert.That( lastDiscoverEventArgs.DeletedAssemblies.Count, Is.EqualTo( 1 ), "...that's what I said." );
            Assert.That( Discoverer.AllPlugins.Count, Is.EqualTo( 2 ), "ServiceA.2/ServiceA.dll contains 2 plugins." );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 1 ), "Plugin01 (same id) exists in both assembly." );

            //Test methods, events and properties info :
            {
                var events = Discoverer.Services.First().EventsInfoCollection;
                var methods = Discoverer.Services.First().MethodsInfoCollection;
                var properties = Discoverer.Services.First().PropertiesInfoCollection;

                Assert.That( events.Count, Is.EqualTo( 1 ) );
                Assert.That( methods.Count, Is.EqualTo( 1 ) );
                Assert.That( properties.Count, Is.EqualTo( 1 ) );

                Assert.That( events.First().Name, Is.EqualTo( "DifferentHasStarted" ) );

                Assert.That( methods.First().Name, Is.EqualTo( "Add" ) );
                Assert.That( methods.First().Parameters.Count, Is.EqualTo( 2 ) );
                Assert.That( methods.First().Parameters.First().ParameterName, Is.EqualTo( "a" ) );
                Assert.That( methods.First().Parameters.Last().ParameterName, Is.EqualTo( "b" ) );
                Assert.That( methods.First().Parameters.First().ParameterType, Is.EqualTo( "System.Double" ) );
                Assert.That( methods.First().Parameters.Last().ParameterType, Is.EqualTo( "System.Int32" ) );
                Assert.That( methods.First().ReturnType, Is.EqualTo( "System.Int32" ) );

                Assert.That( properties.First().Name, Is.EqualTo( "HasBeenStarted" ) );
                Assert.That( properties.First().PropertyType, Is.EqualTo( "System.Boolean" ) );
            }
        }

        [Test]
        public void AddRemoveAssemblies()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer discoverer = new PluginDiscoverer();

            discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            // Add 3 assemblies
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewAssemblies.Count, Is.EqualTo( 3 ) );


            // Remove all assmeblies, add the first two then add a new one.
            TestBase.CleanupTestDir();
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            TestBase.CopyPluginToTestDir( "EditorsOfPlugins.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewAssemblies.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.DeletedAssemblies.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        public void AddRemovePluginsAndService()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer discoverer = new PluginDiscoverer();

            discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            // Add 2 assemblies : 2 plugins & 2 services
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 2 ) );
            Assert.That( lastDiscoverEventArgs.NewServices.Count, Is.EqualTo( 2 ) );

            // Clear and add previous plugins, and add a new service
            TestBase.CleanupTestDir();
            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.NewServices.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.DeletedServices.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.DeletedPlugins.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        public void AddRemoveEditors()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer discoverer = new PluginDiscoverer();

            discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );
            TestBase.CopyPluginToTestDir( "ServiceB.dll" );
            TestBase.CopyPluginToTestDir( "EditorsOfPlugins.dll" );
            discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( lastDiscoverEventArgs.NewEditors.Count, Is.EqualTo( 6 ) );

            TestBase.CleanupTestDir();
            discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( lastDiscoverEventArgs.NewEditors.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.DeletedEditors.Count, Is.EqualTo( 6 ) );
        }

        [Test]
        public void AddBetterPlugins()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer discoverer = new PluginDiscoverer();

            discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            TestBase.CopyPluginToTestDir( "ServiceA.Old.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.NewOldPlugins.Count, Is.EqualTo( 0 ) );

            IPluginInfo bestVersion = discoverer.FindPlugin( new System.Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" ) );
            Assert.That( bestVersion.Version == new Version( "1.0.0" ) );

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.NewOldPlugins.Count, Is.EqualTo( 1 ) );

            IPluginInfo newBestVersion = discoverer.FindPlugin( new System.Guid( "{12A9FCC0-ECDC-4049-8DBF-8961E49A9EDE}" ) );
            Assert.That( newBestVersion.Version == new Version( "1.1.0" ) );

            Assert.That( bestVersion.GetHashCode() == newBestVersion.GetHashCode() );
        }

        [Test]
        public void AddWorstPlugins()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer discoverer = new PluginDiscoverer();

            discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            TestBase.CopyPluginToTestDir( "ServiceA.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.NewServices.Count, Is.EqualTo( 1 ) );

            TestBase.CopyPluginToTestDir( "ServiceA.Old.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );

            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.NewServices.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.NewOldPlugins.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        public void AddRemoveMissingReferences()
        {
            DiscoverDoneEventArgs lastDiscoverEventArgs = null;

            PluginDiscoverer discoverer = new PluginDiscoverer();

            discoverer.DiscoverDone += ( sender, e ) => lastDiscoverEventArgs = e;

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( lastDiscoverEventArgs.NewAssemblies.Count, Is.EqualTo( 1 ) );
            IAssemblyInfo pAss = lastDiscoverEventArgs.NewAssemblies[0];
            Assert.That( pAss.HasError, Is.True );
            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.NewServices.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.NewDisappearedAssemblies.Count, Is.EqualTo( 0 ) );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );

            discoverer.Discover( TestBase.TestFolderDir, true );
            Assert.That( lastDiscoverEventArgs.NewPlugins.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.NewServices.Count, Is.EqualTo( 1 ) );
            Assert.That( pAss.HasError, Is.False, "The assembly info reference is updated." );

            Assert.That( lastDiscoverEventArgs.ChangedAssemblies.Count, Is.EqualTo( 1 ) );
            Assert.That( lastDiscoverEventArgs.ChangedAssemblies[0], Is.SameAs( pAss ) );

            Assert.That( lastDiscoverEventArgs.NewDisappearedAssemblies.Count, Is.EqualTo( 0 ) );
            Assert.That( lastDiscoverEventArgs.DeletedDisappearedAssemblies.Count, Is.EqualTo( 0 ) );
        }
    
    }
}

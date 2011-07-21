using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Context;
using CK.Storage;
using System.IO;

namespace PluginConfig
{
    [TestFixture]
    public class ConfigCollection : TestBase
    {
        [Test]
        public void PluginStatusCollectionEvents()
        {
            IContext ctx = CreateContext();

            PluginStatusCollectionChangingEventArgs lastChanging = null;
            PluginStatusCollectionChangedEventArgs lastChanged = null;
            int changingCount = 0;
            int changedCount = 0;

            PluginStatusCollection collection = new PluginStatusCollection( ctx.ConfigManager.SystemConfiguration as SystemConfiguration );
            collection.Changing += ( o, e ) => { lastChanging = e; changingCount++; };
            collection.Changed += ( o, e ) => { lastChanged = e; changedCount++; };

            // check add
            collection.SetStatus( Guid.Empty, ConfigPluginStatus.Manual );

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginID == Guid.Empty );
            Assert.That( lastChanged.PluginID == Guid.Empty );
            Assert.That( lastChanging.Status == ConfigPluginStatus.Manual );
            Assert.That( lastChanged.Status == ConfigPluginStatus.Manual );

            changedCount = 0; changingCount = 0;

            // check update
            collection.SetStatus( Guid.Empty, ConfigPluginStatus.Disabled );

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Update );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Update );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginID == Guid.Empty );
            Assert.That( lastChanged.PluginID == Guid.Empty );
            Assert.That( lastChanging.Status == ConfigPluginStatus.Disabled );
            Assert.That( lastChanged.Status == ConfigPluginStatus.Disabled );

            changedCount = 0; changingCount = 0;

            // check delete
            IPluginStatus status = collection.GetPluginStatus( Guid.Empty );
            status.Destroy();

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginID == Guid.Empty );
            Assert.That( lastChanged.PluginID == Guid.Empty );
            Assert.That( lastChanging.Status == ConfigPluginStatus.Disabled );
            Assert.That( lastChanged.Status == ConfigPluginStatus.Disabled );

            changedCount = 0; changingCount = 0;

            // check clear
            collection.Clear();

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.ContainerClear );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.ContainerClear );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginID == Guid.Empty );
            Assert.That( lastChanged.PluginID == Guid.Empty );
            Assert.That( lastChanging.Status == 0 );
            Assert.That( lastChanged.Status == 0 );
        }

        [Test]
        public void PluginStatusCollectionMerge()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            IContext ctx = CreateContext();

            PluginStatusCollection collection = new PluginStatusCollection( ctx.ConfigManager.SystemConfiguration as SystemConfiguration );

            collection.SetStatus( id1, ConfigPluginStatus.Manual );

            string path = TestBase.GetTestFilePath( "PluginStatusCollectionMerge" );
            using( Stream s = new FileStream( path, FileMode.Create ) )
            {
                using( var w = SimpleStructuredWriter.CreateWriter( s, null ) )
                {
                    PluginStatusCollection collection2 = new PluginStatusCollection( ctx.ConfigManager.SystemConfiguration as SystemConfiguration );
                    collection2.SetStatus( id1, ConfigPluginStatus.Disabled );
                    collection2.SetStatus( id2, ConfigPluginStatus.Disabled );
                    collection2.SetStatus( id3, ConfigPluginStatus.AutomaticStart );
                    w.WriteInlineObjectStructuredElement( "PC", collection2 );
                }
            }

            int changingCount = 0, changedCount = 0;
            PluginStatusCollectionChangingEventArgs lastChanging = null;
            PluginStatusCollectionChangedEventArgs lastChanged = null;
            collection.Changing += ( o, e ) => { lastChanging = e; changingCount++; };
            collection.Changed += ( o, e ) => { lastChanged = e; changedCount++; };

            using( Stream s = new FileStream( path, FileMode.Open ) )
            {
                using( var r = SimpleStructuredReader.CreateReader( s, null ) )
                {
                    r.ReadInlineObjectStructuredElement( "PC", collection );
                }
            }

            // Check event count & args 
            Assert.That( changingCount == 0 && changedCount == 1 );
            Assert.That( lastChanging == null );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.ContainerUpdate );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanged.PluginID == Guid.Empty );
            Assert.That( lastChanged.Status == 0 );

            // Check content
            Assert.That( collection.Count == 3 );
            Assert.That( collection.GetStatus( id1, ConfigPluginStatus.Manual ) == ConfigPluginStatus.Disabled );
            Assert.That( collection.GetStatus( id2, ConfigPluginStatus.Manual ) == ConfigPluginStatus.Disabled );
            Assert.That( collection.GetStatus( id3, ConfigPluginStatus.Manual ) == ConfigPluginStatus.AutomaticStart );
        }

        [Test]
        public void UserProfileCollectionEvents()
        {
            IContext ctx = CreateContext();

            UserProfileCollectionChangingEventArgs lastChanging = null;
            UserProfileCollectionChangedEventArgs lastChanged = null;
            int changingCount = 0;
            int changedCount = 0;

            UserProfileCollection collection = new UserProfileCollection( ctx.ConfigManager.SystemConfiguration as SystemConfiguration );
            collection.Changing += ( o, e ) => { lastChanging = e; changingCount++; };
            collection.Changed += ( o, e ) => { lastChanged = e; changedCount++; };

            // check add
            collection.AddOrSet( "profile1", "address", ConfigSupportType.File, false );

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.Address == "address" );
            Assert.That( lastChanged.Address == "address" );
            Assert.That( lastChanging.Name == "profile1" );
            Assert.That( lastChanged.Name == "profile1" );
            Assert.That( lastChanging.Type == ConfigSupportType.File );
            Assert.That( lastChanged.Type == ConfigSupportType.File );

            changedCount = 0; changingCount = 0;

            // check update : set as LastProfile
            IUserProfile profile1 = collection.AddOrSet( "profile1", "address", ConfigSupportType.File, true );

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.ContainerUpdate );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.ContainerUpdate );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.Address == "address" );
            Assert.That( lastChanged.Address == "address" );
            Assert.That( lastChanging.Name == "profile1" );
            Assert.That( lastChanged.Name == "profile1" );
            Assert.That( lastChanging.Type == ConfigSupportType.File );
            Assert.That( lastChanged.Type == ConfigSupportType.File );

            changedCount = 0; changingCount = 0;

            // check update : via Rename
            profile1.Rename( "newProfile1" );

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Update );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Update );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.Address == "address" );
            Assert.That( lastChanged.Address == "address" );
            Assert.That( lastChanging.Name == "newProfile1" );
            Assert.That( lastChanged.Name == "newProfile1" );
            Assert.That( lastChanging.Type == ConfigSupportType.File );
            Assert.That( lastChanged.Type == ConfigSupportType.File );

            changedCount = 0; changingCount = 0;

            // check delete
            profile1.Destroy();

            Assert.That( changingCount == 1 && changedCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.Address == "address" );
            Assert.That( lastChanged.Address == "address" );
            Assert.That( lastChanging.Name == "newProfile1" );
            Assert.That( lastChanged.Name == "newProfile1" );
            Assert.That( lastChanging.Type == ConfigSupportType.File );
            Assert.That( lastChanged.Type == ConfigSupportType.File );
        }

        [Test]
        public void UserProfileCollectionMerge()
        {
            IContext ctx = CreateContext();

            UserProfileCollectionChangingEventArgs lastChanging = null;
            UserProfileCollectionChangedEventArgs lastChanged = null;
            int changingCount = 0;
            int changedCount = 0;

            UserProfileCollection collection = new UserProfileCollection( ctx.ConfigManager.SystemConfiguration as SystemConfiguration );
            collection.Changing += ( o, e ) => { lastChanging = e; changingCount++; };
            collection.Changed += ( o, e ) => { lastChanged = e; changedCount++; };

            collection.AddOrSet( "profile1", "address", ConfigSupportType.Other, true );

            string path = TestBase.GetTestFilePath( "UserProfileCollectionMerge" );
            using( Stream s = new FileStream( path, FileMode.Create ) )
            {
                using( var w = SimpleStructuredWriter.CreateWriter( s, null ) )
                {
                    UserProfileCollection collection2 = new UserProfileCollection( ctx.ConfigManager.SystemConfiguration as SystemConfiguration );
                    collection2.AddOrSet( "profile1", "address", ConfigSupportType.Other, false );
                    collection2.AddOrSet( "profile2", "address2", ConfigSupportType.File, true );
                    collection2.AddOrSet( "profile3", "address3", ConfigSupportType.None, false );

                    w.WriteInlineObjectStructuredElement( "UserProfiles", collection2 );
                }
            }

            changedCount = 0; changingCount = 0;
            lastChanged = null; lastChanging = null;

            using( Stream s = new FileStream( path, FileMode.Open ) )
            {
                using( var r = SimpleStructuredReader.CreateReader( s, null ) )
                {
                    r.ReadInlineObjectStructuredElement( "UserProfiles", collection );
                }
            }

            // Check event count & args.
            Assert.That( changingCount == 0 && changedCount == 1 );
            Assert.That( lastChanging == null );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.ContainerUpdate );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanged.Address == string.Empty );
            Assert.That( lastChanged.Name == string.Empty );
            Assert.That( lastChanged.Type == ConfigSupportType.None );

            // Check content
            Assert.That( collection.Count == 3 );
            Assert.That( collection.Find( "address" ).Name == "profile1" );
            Assert.That( collection.Find( "address" ).Type == ConfigSupportType.Other );
            Assert.That( !collection.Find( "address" ).IsLastProfile );

            Assert.That( collection.Find( "address2" ).Name == "profile2" );
            Assert.That( collection.Find( "address2" ).Type == ConfigSupportType.File );
            Assert.That( collection.Find( "address2" ).IsLastProfile );

            Assert.That( collection.Find( "address3" ).Name == "profile3" );
            Assert.That( collection.Find( "address3" ).Type == ConfigSupportType.None );
            Assert.That( !collection.Find( "address3" ).IsLastProfile );

            foreach( UserProfile profile in collection )
            {
                Assert.That( profile.Holder == collection );
            }
        }

        [Test]
        public void PluginRequirementCollectionEvents()
        {
            PluginRequirementCollectionEvents( new PluginRequirementCollection() );
        }
        void PluginRequirementCollectionEvents( PluginRequirementCollection collection )
        {
            Guid id = Guid.NewGuid();

            PluginRequirementCollectionChangingEventArgs lastChanging = null;
            PluginRequirementCollectionChangedEventArgs lastChanged = null;
            int changingCount = 0;
            int changedCount = 0;

            collection.Changing += ( o, e ) => { lastChanging = e; changingCount++; };
            collection.Changed += ( o, e ) => { lastChanged = e; changedCount++; };

            // Check add
            PluginRequirement req = collection.AddOrSet( id, RunningRequirement.MustExistAndRun );

            Assert.That( changedCount == 1 && changingCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginId == id );
            Assert.That( lastChanged.PluginId == id );
            Assert.That( lastChanging.Requirement == RunningRequirement.MustExistAndRun );
            Assert.That( lastChanged.Requirement == RunningRequirement.MustExistAndRun );

            changedCount = 0; changingCount = 0;

            // Check delete : from the collection
            collection.Remove( id );

            Assert.That( changedCount == 1 && changingCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginId == id );
            Assert.That( lastChanged.PluginId == id );
            Assert.That( lastChanging.Requirement == RunningRequirement.MustExistAndRun );
            Assert.That( lastChanged.Requirement == RunningRequirement.MustExistAndRun );

            changedCount = 0; changingCount = 0;

            // Check clear
            collection.Clear();

            Assert.That( changedCount == 1 && changingCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.ContainerClear );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.ContainerClear );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.PluginId == Guid.Empty );
            Assert.That( lastChanged.PluginId == Guid.Empty );
            Assert.That( lastChanging.Requirement == 0 );
            Assert.That( lastChanged.Requirement == 0 );
        }

        [Test]
        public void ServiceRequirementCollectionEvents()
        {
            ServiceRequirementCollectionEvents( new ServiceRequirementCollection() );
        }
        void ServiceRequirementCollectionEvents( ServiceRequirementCollection collection )
        {
            string id = "service.full.name";

            ServiceRequirementCollectionChangingEventArgs lastChanging = null;
            ServiceRequirementCollectionChangedEventArgs lastChanged = null;
            int changingCount = 0;
            int changedCount = 0;

            collection.Changing += ( o, e ) => { lastChanging = e; changingCount++; };
            collection.Changed += ( o, e ) => { lastChanged = e; changedCount++; };

            // Check add
            ServiceRequirement req = collection.AddOrSet( id, RunningRequirement.MustExistAndRun );

            Assert.That( changedCount == 1 && changingCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Add );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.AssemblyQualifiedName == id );
            Assert.That( lastChanged.AssemblyQualifiedName == id );
            Assert.That( lastChanging.Requirement == RunningRequirement.MustExistAndRun );
            Assert.That( lastChanged.Requirement == RunningRequirement.MustExistAndRun );

            changedCount = 0; changingCount = 0;

            // Check delete : from the collection
            collection.Remove( id );

            Assert.That( changedCount == 1 && changingCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.Delete );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.AssemblyQualifiedName == id );
            Assert.That( lastChanged.AssemblyQualifiedName == id );
            Assert.That( lastChanging.Requirement == RunningRequirement.MustExistAndRun );
            Assert.That( lastChanged.Requirement == RunningRequirement.MustExistAndRun );

            changedCount = 0; changingCount = 0;

            // Check clear
            collection.Clear();

            Assert.That( changedCount == 1 && changingCount == 1 );
            Assert.That( lastChanging.Action == CK.Core.ChangeStatus.ContainerClear );
            Assert.That( lastChanged.Action == CK.Core.ChangeStatus.ContainerClear );
            Assert.That( lastChanging.Collection == collection );
            Assert.That( lastChanged.Collection == collection );
            Assert.That( lastChanging.AssemblyQualifiedName == string.Empty );
            Assert.That( lastChanged.AssemblyQualifiedName == string.Empty );
            Assert.That( lastChanging.Requirement == 0 );
            Assert.That( lastChanged.Requirement == 0 );
        }

        [Test]
        public void RequirementLayersEvents()
        {
            RequirementLayer layer = new RequirementLayer( "Layer" );
            PluginRequirementCollectionEvents( layer.PluginRequirements as PluginRequirementCollection );
            ServiceRequirementCollectionEvents( layer.ServiceRequirements as ServiceRequirementCollection );
        }
    }
}

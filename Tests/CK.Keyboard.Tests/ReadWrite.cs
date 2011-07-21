using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using CK.Storage;
using CK.Keyboard.Model;
using CK.Keyboard;
using CK.SharedDic;
using CK.Core;
using CK.Plugin;
using System.Xml;
using System.ComponentModel.Design;
using CK.Context;
using CK.Plugin.Config;

namespace Keyboard
{
    [TestFixture]
    public class ReadWrite
    {
        [Test]
        public void WritesReadKeyboard()
        {
            string testPath = TestBase.GetTestFilePath( "Keyboard" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                KeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
                ctx.Keyboards.Create( "kiborde" );
                WriteKeyboardContext( testPath, dic, ctx );
            }
            TestBase.DumpFileToConsole( testPath );
            {
                IKeyboardContext ctx = ReadKeyboardContext( testPath, container );
                Assert.That( ctx.Keyboards.Count == 1 );
                Assert.That( ctx.Keyboards.Current.Name == "kiborde" );
            }
        }

        [Test]
        public void WritesReadZones()
        {
            string testPath = TestBase.GetTestFilePath( "Zones" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                KeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
                ctx.Keyboards.Create( "kiborde" );
                ctx.Keyboards["kiborde"].Zones.Create( "zone1" );
                ctx.Keyboards["kiborde"].Zones.Create( "zone2" );

                WriteKeyboardContext( testPath, dic, ctx );
            }

            {
                IKeyboardContext ctx = ReadKeyboardContext( testPath, container );

                Assert.That( ctx.Keyboards["kiborde"].Zones.Count == 3 );
                Assert.That( ctx.Keyboards["kiborde"].Zones[string.Empty] != null );
                Assert.That( ctx.Keyboards["kiborde"].Zones["zone1"] != null );
                Assert.That( ctx.Keyboards["kiborde"].Zones["zone2"] != null );
            }
        }

        [Test]
        public void WritesReadKey()
        {
            string testPath = TestBase.GetTestFilePath( "Key" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                IKeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
                ctx.Keyboards.Create( "kiborde" );
                
                IKey k = ctx.Keyboards["kiborde"].Zones.Default.Keys.Create();
                k.Current.OnKeyDownCommands.Commands.Add( "Current key Fire, Fire!" );
                k.Current.OnKeyDownCommands.Commands.Add( "Current key Fire, Fire again!" );
                k.Current.UpLabel = "Current key down label.";
                k.Current.DownLabel = "Current key down label.";
                k.Current.Description = "Description of the current key.";
                
                WriteKeyboardContext( testPath, dic, ctx );
            }
            TestBase.DumpFileToConsole( testPath );
            {
                IKeyboardContext ctx = ReadKeyboardContext( testPath, container );
                Assert.That( ctx.Keyboards["kiborde"].Zones.Default.Keys.Count == 1 );

                IKey k = ctx.Keyboards["kiborde"].Zones.Default.Keys[0];
                Assert.That( k.Current.OnKeyDownCommands.Commands[0] == "Current key Fire, Fire!" );
                Assert.That( k.Current.OnKeyDownCommands.Commands[1] == "Current key Fire, Fire again!" );
                Assert.That( k.Current.UpLabel == "Current key down label." );
                Assert.That( k.Current.DownLabel == "Current key down label." );
                Assert.That( k.Current.Description == "Description of the current key." );
            }
        }

        [Test]
        public void WritesReadServiceRequirements()
        {
            string testPath = TestBase.GetTestFilePath( "ServiceRequirements" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            IEnumerable<string> original;
            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                IKeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
                ctx.Keyboards.Create( "kiborde" );

                ctx.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.AddOrSet( "service.fullname.service1", RunningRequirement.MustExistAndRun );
                ctx.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.AddOrSet( "service.fullname.service2", RunningRequirement.MustExistTryStart );
                ctx.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.AddOrSet( "service.fullname.service3", RunningRequirement.OptionalTryStart );
                ctx.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.AddOrSet( "service.fullname.service4", RunningRequirement.MustExist );
                ctx.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.AddOrSet( "service.fullname.service5", RunningRequirement.Optional );

                original = ctx.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.Select( p => String.Format( "{0}-{1}|", p.AssemblyQualifiedName, p.Requirement ) );

                WriteKeyboardContext( testPath, dic, ctx );
            }
            {
                IKeyboardContext ctx2 = ReadKeyboardContext( testPath, container );
                var read = ctx2.Keyboards["kiborde"].RequirementLayer.ServiceRequirements.Select( p => String.Format( "{0}-{1}|", p.AssemblyQualifiedName, p.Requirement ) );

                Assert.That( original.OrderBy( Util.FuncIdentity ).SequenceEqual( read.OrderBy( Util.FuncIdentity ) ) );

            }
        }

        [Test]
        public void WritesReadPluginRequirements()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();
            Guid guid4 = Guid.NewGuid();
            Guid guid5 = Guid.NewGuid();

            string testPath = TestBase.GetTestFilePath( "PluginRequirements" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                IKeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
                ctx.Keyboards.Create( "kiborde" );

                ctx.Keyboards["kiborde"].RequirementLayer.PluginRequirements.AddOrSet( guid1, RunningRequirement.MustExistAndRun );
                ctx.Keyboards["kiborde"].RequirementLayer.PluginRequirements.AddOrSet( guid2, RunningRequirement.MustExistTryStart );
                ctx.Keyboards["kiborde"].RequirementLayer.PluginRequirements.AddOrSet( guid3, RunningRequirement.OptionalTryStart );
                ctx.Keyboards["kiborde"].RequirementLayer.PluginRequirements.AddOrSet( guid4, RunningRequirement.MustExist );
                ctx.Keyboards["kiborde"].RequirementLayer.PluginRequirements.AddOrSet( guid5, RunningRequirement.Optional );

                WriteKeyboardContext( testPath, dic, ctx );
            }

            {
                IKeyboardContext ctx2 = ReadKeyboardContext( testPath, container );

                Assert.That( ctx2.Keyboards["kiborde"].RequirementLayer.PluginRequirements.Count == 5 );

                var good = (new[] { String.Format( "{0}-{1}|", guid1, RunningRequirement.MustExistAndRun ),
                                    String.Format( "{0}-{1}|", guid2, RunningRequirement.MustExistTryStart ),
                                    String.Format( "{0}-{1}|", guid3, RunningRequirement.OptionalTryStart ),
                                    String.Format( "{0}-{1}|", guid4, RunningRequirement.MustExist ),
                                    String.Format( "{0}-{1}|", guid5, RunningRequirement.Optional )
                                  }).OrderBy( Util.FuncIdentity );
                var reqs = ctx2.Keyboards["kiborde"].RequirementLayer.PluginRequirements.Select( p => String.Format( "{0}-{1}|", p.PluginId, p.Requirement ) );

                Assert.That( good.OrderBy( Util.FuncIdentity ).SequenceEqual( reqs.OrderBy( Util.FuncIdentity ) ) );
            }
        }

        [Test]
        public void ReadKeysAndModes()
        {
            string testPath = TestBase.GetTestFilePath( "KeyModeAndModes" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                IKeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };

                var kb = ctx.Keyboards.Create( "kiborde" );
                kb.AvailableMode = kb.AvailableMode.Add( ctx.ObtainMode( "frigo" ) );
                kb.AvailableMode = kb.AvailableMode.Add( ctx.ObtainMode( "congelo" ) );

                IKey k0 = kb.Zones.Default.Keys.Create();
                k0.Current.OnKeyDownCommands.Commands.Add( "command" );
                k0.Current.UpLabel = "a";
                k0.Current.DownLabel = "a";
                k0.CurrentLayout.Current.X = 111;
                k0.CurrentLayout.Current.Y = 222;
                k0.CurrentLayout.Current.Width = 333;
                k0.CurrentLayout.Current.Height = 444;

                IKey k1 = kb.Zones.Default.Keys.Create();
                k1.Current.OnKeyDownCommands.Commands.Add( "command2" );
                k1.Current.UpLabel = "b";
                k1.Current.DownLabel = "b";
                k1.CurrentLayout.Current.X = 1;
                k1.CurrentLayout.Current.Y = 2;
                k1.CurrentLayout.Current.Width = 3;
                k1.CurrentLayout.Current.Height = 4;


                IKeyMode kak0 = k0.KeyModes.Create( ctx.ObtainMode( "frigo" ) );
                kak0.UpLabel = "A";
                kak0.DownLabel = "A";

                IKeyMode kak1 = k0.KeyModes.Create( ctx.ObtainMode( "congelo" ) );
                kak1.UpLabel = "B";
                kak1.DownLabel = "B";

                WriteKeyboardContext( testPath, dic, ctx );
            }
            // Reads context and checks it.
            {
                IKeyboardContext ctx = ReadKeyboardContext( testPath, container );

                Assert.That( ctx.Keyboards["kiborde"].AvailableMode.AtomicModes.Count, Is.EqualTo( 2 ) );
                Assert.That( ctx.Keyboards["kiborde"].Zones.Default.Keys.Count, Is.EqualTo( 2 ) );

                IKey k0 = ctx.Keyboards["kiborde"].Zones.Default.Keys[0];

                Assert.That( k0.KeyModes.Count == 3 );
                Assert.That( k0.KeyModes[ctx.EmptyMode].UpLabel == "a" );
                Assert.That( k0.KeyModes[ctx.EmptyMode].DownLabel == "a" );
                Assert.That( k0.KeyModes[ctx.ObtainMode( "frigo" )].UpLabel, Is.EqualTo( "A" ) );
                Assert.That( k0.KeyModes[ctx.ObtainMode( "frigo" )].DownLabel, Is.EqualTo( "A" ) );
                Assert.That( k0.KeyModes[ctx.ObtainMode( "congelo" )].UpLabel, Is.EqualTo( "B" ) );
                Assert.That( k0.KeyModes[ctx.ObtainMode( "congelo" )].DownLabel, Is.EqualTo( "B" ) );

                Assert.That( k0.CurrentLayout.Current.X == 111 );
                Assert.That( k0.CurrentLayout.Current.Y == 222 );
                Assert.That( k0.CurrentLayout.Current.Width == 333 );
                Assert.That( k0.CurrentLayout.Current.Height == 444 );

                IKey k1 = ctx.Keyboards["kiborde"].Zones.Default.Keys[1];

                Assert.That( k1.KeyModes.Count == 1 );
                Assert.That( k1.KeyModes[ctx.EmptyMode].UpLabel == "b" );
                Assert.That( k1.KeyModes[ctx.EmptyMode].DownLabel == "b" );

                Assert.That( k1.CurrentLayout.Current.X == 1 );
                Assert.That( k1.CurrentLayout.Current.Y == 2 );
                Assert.That( k1.CurrentLayout.Current.Width == 3 );
                Assert.That( k1.CurrentLayout.Current.Height == 4 );
            }
        }
        
        [Test]
        public void ReadKeysAndModesBis()
        {
            // Writes a second context.
            string testPath = TestBase.GetTestFilePath( "KeyModeAndModes_bis" );
            
            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );
            {
                // Now, let's write another context.
                ISharedDictionary dic = SharedDictionary.Create( container );
                IKeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };

                var kb = ctx.Keyboards.Create( "Kouborde" );
                kb.AvailableMode = kb.AvailableMode.Add( ctx.ObtainMode( "zanimo" ) );
                kb.AvailableMode = kb.AvailableMode.Add( ctx.ObtainMode( "congelo" ) );
                kb.AvailableMode = kb.AvailableMode.Add( ctx.ObtainMode( "bolero" ) );

                IKey k = kb.Zones.Default.Keys.Create();
                k.Current.OnKeyDownCommands.Commands.Add( "command_bis" );
                k.Current.UpLabel = "a_bis";
                k.Current.DownLabel = "a_bis";

                IKey k2 = kb.Zones.Default.Keys.Create();
                k2.Current.OnKeyDownCommands.Commands.Add( "command2_bis" );
                k2.Current.UpLabel = "b_bis";
                k2.Current.DownLabel = "b_bis";

                IKey k3 = kb.Zones.Default.Keys.Create();
                k3.Current.OnKeyDownCommands.Commands.Add( "command3_bis" );
                k3.Current.UpLabel = "c_bis";
                k3.Current.DownLabel = "c_bis";

                IKeyMode kak = k.KeyModes.Create( ctx.ObtainMode( "zanimo" ) );
                kak.UpLabel = "A_bis";
                kak.DownLabel = "A_bis";

                IKeyMode kak2 = k.KeyModes.Create( ctx.ObtainMode( "congelo" ) );
                kak2.UpLabel = "B_bis";
                kak2.DownLabel = "B_bis";

                IKeyMode kak3 = k.KeyModes.Create( ctx.ObtainMode( "bolero" ) );
                kak3.UpLabel = "C_bis";
                kak3.DownLabel = "C_bis";

                WriteKeyboardContext( testPath, dic, ctx );
            }

            
            // Reads the second context.
            {
                IKeyboardContext ctx = ReadKeyboardContext( testPath, container );

                Assert.That( ctx.Keyboards["Kouborde"].AvailableMode.AtomicModes.Count == 3 );
                Assert.That( ctx.Keyboards["Kouborde"].Zones.Default.Keys.Count == 3 );

                IKey k0_bis = ctx.Keyboards["Kouborde"].Zones.Default.Keys[0];

                Assert.That( k0_bis.KeyModes.Count == 4 );
                Assert.That( k0_bis.KeyModes[ctx.EmptyMode].UpLabel == "a_bis" );
                Assert.That( k0_bis.KeyModes[ctx.EmptyMode].DownLabel == "a_bis" );
                Assert.That( k0_bis.KeyModes[ctx.ObtainMode( "zanimo" )].UpLabel == "A_bis" );
                Assert.That( k0_bis.KeyModes[ctx.ObtainMode( "zanimo" )].DownLabel == "A_bis" );
                Assert.That( k0_bis.KeyModes[ctx.ObtainMode( "congelo" )].UpLabel == "B_bis" );
                Assert.That( k0_bis.KeyModes[ctx.ObtainMode( "congelo" )].DownLabel == "B_bis" );
                Assert.That( k0_bis.KeyModes[ctx.ObtainMode( "bolero" )].UpLabel == "C_bis" );
                Assert.That( k0_bis.KeyModes[ctx.ObtainMode( "bolero" )].DownLabel == "C_bis" );

                IKey k1_bis = ctx.Keyboards["Kouborde"].Zones.Default.Keys[1];

                Assert.That( k1_bis.KeyModes.Count == 1 );
                Assert.That( k1_bis.KeyModes[ctx.EmptyMode].UpLabel == "b_bis" );
                Assert.That( k1_bis.KeyModes[ctx.EmptyMode].DownLabel == "b_bis" );

                IKey k4_bis = ctx.Keyboards["Kouborde"].Zones.Default.Keys[2];

                Assert.That( k4_bis.KeyModes.Count == 1 );
                Assert.That( k4_bis.KeyModes[ctx.EmptyMode].UpLabel == "c_bis" );
                Assert.That( k4_bis.KeyModes[ctx.EmptyMode].DownLabel == "c_bis" );
            }

        }

        [Test]
        public void ReadPluginData()
        {
            string testPath = TestBase.GetTestFilePath( "KeyModeAndModesWithPluginData" );

            ISimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( RequirementLayerSerializer.Instance );
            container.Add( SimpleTypeFinder.Default );

            INamedVersionedUniqueId pluginId = new SimpleNamedVersionedUniqueId( "{073F8B43-F088-49f1-BC95-6960A15D463F}", "1.0.0", "JustForTest" );

            {
                ISharedDictionary dic = SharedDictionary.Create( container );
                IKeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
                var kb = ctx.Keyboards.Create( "kiborde" );

                dic.Add( ctx, pluginId, "testCell", "Awesome value associated to the keyboard context." );
                dic.Add( kb, pluginId, "KbProperty", "Value associated to the kiborde object." );
                dic.Add( kb.Zones.Default, pluginId, "ZoneProperty", "Value associated to the keyborde's default zone object." );

                WriteKeyboardContext( testPath, dic, ctx );
            }

            TestBase.DumpFileToConsole( testPath );

            {
                KeyboardContext ctx = new KeyboardContext();
                ctx = ReadKeyboardContext( testPath, container );
                IConfigContainer config = ctx.ConfigContainer;                
                config.Ensure( pluginId );                               

                Assert.That( ctx.CurrentKeyboard.Name, Is.EqualTo( "kiborde" ) );

                Assert.That( config[ctx, pluginId, "testCell"], Is.EqualTo( "Awesome value associated to the keyboard context." ) );
                Assert.That( config[ctx.CurrentKeyboard, pluginId, "KbProperty"], Is.EqualTo( "Value associated to the kiborde object." ) );
                Assert.That( config[ctx.CurrentKeyboard.Zones.Default, pluginId, "ZoneProperty"], Is.EqualTo( "Value associated to the keyborde's default zone object." ) );
            }
        }

        private static KeyboardContext ReadKeyboardContext( string testPath, ISimpleServiceContainer container )
        {
            MissingDisposeCallSentinel.DebugCheckMissing( Assert.Fail );
            ISharedDictionary dic = SharedDictionary.Create( container );
            KeyboardContext ctx = new KeyboardContext() { ConfigContainer = dic };
            using( Stream str = new FileStream( testPath, FileMode.Open ) )
            {
                using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, dic.ServiceProvider ) )
                {
                    using( ISharedDictionaryReader r = dic.RegisterReader( sr, MergeMode.None ) )
                    {
                        Assert.That( sr.GetService<ISharedDictionaryReader>() == r, "Creating a SharedDictionaryReader also registers it into the StructuredReader's services." );
                        Assert.That( container == null || container.GetService<ISharedDictionaryReader>() == null, "...but (of course) the service is not available above." );

                        sr.ReadInlineObjectStructuredElement( "KeyboardContext", ctx );

                    }
                    Assert.That( sr.GetService<ISharedDictionaryReader>() == null, "If we dispose the SharedDictionaryReader... The service has been removed." );
                }
            }
            MissingDisposeCallSentinel.DebugCheckMissing( Assert.Fail );
            return ctx;
        }

        private static void WriteKeyboardContext( string testPath, ISharedDictionary dic, IKeyboardContext ctx )
        {
            MissingDisposeCallSentinel.DebugCheckMissing( Assert.Fail );
            using( Stream wrt = new FileStream( testPath, FileMode.Create ) )
            {
                using( IStructuredWriter sw = SimpleStructuredWriter.CreateWriter( wrt, dic.ServiceProvider ) )
                {
                    using( ISharedDictionaryWriter w = dic.RegisterWriter( sw ) )
                    {
                        Assert.That( sw.GetService<ISharedDictionaryWriter>() == w, "Creating a SharedDictionaryWriter also registers it into the StructuredWriter's services." );
                        Assert.That( dic.ServiceProvider == null || dic.ServiceProvider.GetService<ISharedDictionaryWriter>() == null, "...but the service is NOT available above." );

                        sw.WriteInlineObjectStructuredElement( "KeyboardContext", ctx );

                    }
                    Assert.That( sw.GetService<ISharedDictionaryWriter>() == null, "If we dispose the SharedDictionaryWriter... The service has been removed." );
                }
            }
            MissingDisposeCallSentinel.DebugCheckMissing( Assert.Fail );
        }

    }
}

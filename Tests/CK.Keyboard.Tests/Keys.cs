#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Tests\Kernel\Contexts\Keys.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using NUnit.Framework;
using CK.Keyboard.Model;
using CK.Keyboard;

namespace Keyboard
{
    [TestFixture]
    public class Keys
    {
        KeyboardContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new KeyboardContext( null );
        }

        [Test]
        public void CreateMoveAndDestroy()
        {
            IKeyboard keyboard = Context.Keyboards.Create( "Everything in Its Right Place" );

            IKey k0 = null, k1 = null, k2 = null, k3 = null;

            int nbKeyCreated = 0;
            keyboard.KeyCreated += delegate( object sender, KeyEventArgs e )
            {
                Assert.That( e.Keyboard == keyboard, "From our keyboard" );
                Assert.That( e.Key != null, "Key created." );
                ++nbKeyCreated;
            };

            int nbKeyDestroyed = 0;
            keyboard.KeyDestroyed += delegate( object sender, KeyEventArgs e )
            {
                Assert.That( e.Keyboard == keyboard, "From our keyboard" );
                Assert.That( e.Key == k0 || e.Key == k1 || e.Key == k2 || e.Key == k3, "One of our key." );
                ++nbKeyDestroyed;
            };
            IZone z = keyboard.Zones.Default;
            Assert.That( z.Keys.Count == 0, "No keys yet." );
            
            k1 = z.Keys.Create( 92387 );
            Assert.That( k1.Index == 0, "Add to the end." );

            k0 = z.Keys.Create( -1 );
            Assert.That( k0.Index == 0 && k1.Index == 1, "Add to the beginning." );

            k2 = z.Keys.Create();
            Assert.That( k0.Index == 0 && k1.Index == 1 && k2.Index == 2, "No position: Add to the end." );

            k3 = z.Keys.Create( 1 );
            Assert.That( k0.Index == 0 && k3.Index == 1 && k1.Index == 2 && k2.Index == 3, "k3 at 1." );

            Assert.That( nbKeyCreated == 4 && z.Keys.Count == 4, "4 keys in the zone." );

            // Moving indices
            int nbKeyMoved = 0;
            keyboard.KeyMoved += delegate( object sender, KeyMovedEventArgs e )
            {
                Assert.That( e.Keyboard == keyboard, "From our keyboard" );
                Assert.That(
                    (nbKeyMoved == 0 && e.Key == k2 && e.PreviousIndex == 3 && k2.Index == 0)
                    || (nbKeyMoved == 1 && e.Key == k0 && e.PreviousIndex == 1 && k0.Index == 3), "k2 moved from 2 to 0, and then, k0 moved from 1 to 3." );
                ++nbKeyMoved;
            };

            k2.Index = 0;
            Assert.That( k2.Index == 0 && k0.Index == 1 && k3.Index == 2 && k1.Index == 3, "Moved k2 at the beginning." );
            k0.Index = 1312;
            Assert.That( k2.Index == 0 && k3.Index == 1 && k1.Index == 2 && k0.Index == 3, "Moved k0 at the end." );

            k1.Destroy();
            Assert.That( k2.Index == 0 && k3.Index == 1 && k0.Index == 2, "Moved down." );
            k2.Destroy();
            Assert.That( k3.Index == 0 && k0.Index == 1, "Moved down." );
            k0.Destroy();
            Assert.That( k3.Index == 0, "At rhe end." );
            k3.Destroy();
            
            Assert.That( nbKeyDestroyed == 4 && z.Keys.Count == 0, "No more keys in the zone." );
        }

        [Test]
        public void KeyModeChanged()
        {
            IKeyboard kb = Context.Keyboards.Create( "Kiborde" );

            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "mode1" ) );
            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "mode2" ) );

            Assert.That( kb.AvailableMode.AtomicModes.Count, Is.EqualTo( 2 ) );

            IKey k = kb.Zones.Default.Keys.Create();
            IKeyMode km0 = k.KeyModes.Create( Context.EmptyMode );
            IKeyMode km1 = k.KeyModes.Create( Context.ObtainMode( "mode1" ) );
            IKeyMode km2 = k.KeyModes.Create( Context.ObtainMode( "mode2" ) );

            ILayoutKeyMode lkm0 = k.CurrentLayout.LayoutKeyModes.Create( Context.EmptyMode );
            ILayoutKeyMode lkm1 = k.CurrentLayout.LayoutKeyModes.Create( Context.ObtainMode( "mode1" ) );
            ILayoutKeyMode lkm2 = k.CurrentLayout.LayoutKeyModes.Create( Context.ObtainMode( "mode2" ) );

            Assert.That( k.Current == km0 );
            Assert.That( k.CurrentLayout.Current == lkm0 );

            kb.CurrentMode = Context.ObtainMode( "mode1" );
            Assert.That( k.Current == km1 );
            Assert.That( k.CurrentLayout.Current == lkm1 );

            bool fired = false;
            bool layoutFired = false;
            k.KeyPropertyChanged += ( o, e ) =>
                {
                    if( e.PropertyName == "Current" ) fired = true;
                    else if( e.PropertyName == "CurrentLayout" ) layoutFired = true;
                };

            kb.CurrentMode = Context.ObtainMode( "mode2" );
            Assert.That( k.Current == km2 );
            Assert.That( k.CurrentLayout.Current == lkm2 );
            Assert.That( fired && layoutFired);
        }

        [Test]
        public void KeyModeModeChangedForGreaterMode()
        {
            IKeyboard kb = Context.Keyboards.Create( "Kiborde" );

            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "mode1" ) );
            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "mode2" ) );

            Assert.That( kb.AvailableMode.AtomicModes.Count, Is.EqualTo( 2 ) );

            IKey k = kb.Zones.Default.Keys.Create();
            IKeyMode km1 = k.KeyModes.Create( Context.ObtainMode( "mode1" ) );

            kb.CurrentMode = Context.ObtainMode( "mode1" );
            Assert.That( k.Current == km1 );

            bool fired = false;
            k.KeyModeModeChanged += ( o, e ) => fired = true;

            km1.ChangeMode( Context.ObtainMode( "mode2" ) );

            Assert.That( fired );
            Assert.That( km1.Mode, Is.EqualTo( Context.ObtainMode( "mode2" ) ) );
        }

        [Test]
        public void KeyModeModeChangedForSmallerMode()
        {
            IKeyboard kb = Context.Keyboards.Create( "Kiborde" );

            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "mode1" ) );
            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "mode2" ) );

            Assert.That( kb.AvailableMode.AtomicModes.Count, Is.EqualTo( 2 ) );

            IKey k = kb.Zones.Default.Keys.Create();
            IKeyMode km1 = k.KeyModes.Create( Context.ObtainMode( "mode2" ) );

            kb.CurrentMode = Context.ObtainMode( "mode2" );
            Assert.That( k.Current == km1 );

            bool fired = false;
            k.KeyModeModeChanged += ( o, e ) => fired = true;

            km1.ChangeMode( Context.ObtainMode( "mode1" ) );

            Assert.That( fired );
            Assert.That( km1.Mode, Is.EqualTo( Context.ObtainMode( "mode1" ) ) );

            fired = false;

            km1.ChangeMode( Context.ObtainMode( "mode2" ) );

            Assert.That( fired );
            Assert.That( km1.Mode, Is.EqualTo( Context.ObtainMode( "mode2" ) ) );

            fired = false;

            km1.ChangeMode( Context.ObtainMode( "mode2" ) );

            Assert.That( !fired );
            Assert.That( km1.Mode, Is.EqualTo( Context.ObtainMode( "mode2" ) ) );
        }

        [Test]
        public void DefaultLayouts()
        {
            IKeyboard keyboard = Context.Keyboards.Create( "Anyone Can Play the Guitar" );
            
            Assert.That( keyboard.Layouts.Default == keyboard.Layouts.Current, "Default layout is the current one" );
            for( int i = 0; i < 10; ++i ) keyboard.Zones.Default.Keys.Create();
            for( int i = 0; i < 5; ++i )
            {
                ILayoutKey l = keyboard.Zones.Default.Keys[i].CurrentLayout;
                Assert.That( l != null, "Layout automatically created by accessing layouts collection." );
                Assert.That( l.LayoutZone == keyboard.Zones.Default.CurrentLayout, "Default keyboard zone layout" );
                Assert.That( l.LayoutKeyModes.Count == 1, "Only one Actual key ==> one layout." );
                Assert.That( l == keyboard.Layouts.Default.LayoutZones.Default.LayoutKeys[i], "From layouts to keys." );
            }
            for( int i = 5; i < 10; ++i )
            {
                ILayoutKey l = keyboard.Layouts.Default.LayoutZones.Default.LayoutKeys[i];
                Assert.That( l != null, "Layout automatically created." );
                Assert.That( l.Layout == keyboard.Layouts.Default, "Default keyboard layout." );
                Assert.That( l.LayoutZone == keyboard.Zones.Default.CurrentLayout, "Default keyboard zone layout" );
                Assert.That( l.LayoutKeyModes.Count == 1, "Only one Actual key ==> one layout." );
                Assert.That( l == keyboard.Zones.Default.Keys[i].CurrentLayout, "From key to its layout." );
            }
        }

        [Test]
        public void KeyEvents()
        {
            IKeyboard kb = Context.Keyboards.Create("test keyboard");
            IKey k = kb.Zones.Default.Keys.Create();

            int nbKeyDown = 0;
            int nbKeyPressed = 0;
            int nbKeyUp = 0;

            bool isUp = true;

            kb.KeyDown += delegate( object sender, KeyInteractionEventArgs e )
            {
                Assert.That( isUp, Is.True );
                Assert.That( k.IsDown, Is.True );
                nbKeyDown++;
                isUp = false;
            };
            kb.KeyPressed += delegate( object sender, KeyPressedEventArgs e )
            {
                Assert.That( isUp, Is.False );
                Assert.That( k.IsDown, Is.True );
                nbKeyPressed++;
            };
            kb.KeyUp += delegate( object sender, KeyInteractionEventArgs e )
            {
                Assert.That( isUp, Is.False );
                Assert.That( k.IsDown, Is.False );
                isUp = true;
                nbKeyUp++;
            };

            Assert.That( k.IsDown, Is.False );
            k.Push();
            Assert.AreEqual( 1, nbKeyDown );
            Assert.That( k.IsDown, Is.True );
            k.RepeatPressed();
            Assert.AreEqual( 1, nbKeyPressed );
            k.RepeatPressed();
            Assert.AreEqual( 2, nbKeyPressed );
            k.Release();
            Assert.AreEqual( 3, nbKeyPressed );
            Assert.AreEqual( 1, nbKeyUp );

            Assert.Throws( typeof( InvalidOperationException ), delegate() { k.Release(); } );
            Assert.Throws( typeof( InvalidOperationException ), () => k.RepeatPressed() );
        }

        [Test]
        public void KeyPrograms()
        {
            IKeyProgram kp = new KeyProgram( Context );

            //Indeces can not be initialized at 0. 
            //Since 0 is a valid index, we initialize the indices to a negative (invalid) value.
            int insertIndex = -243, insertCount = 0;
            int updateIndex = -243, updateCount = 0;
            int removeIndex = -243, removeCount = 0;
            bool isCleared = false;

            kp.CommandInserted += delegate( object sender, KeyProgramCommandsEventArgs e )
            {
                Assert.That( e.KeyProgram == kp );
                Assert.That( e.EventType == KeyProgramCommandsEventType.Inserted );
                insertIndex = e.Index;
                insertCount++;
            };
            kp.CommandUpdated += delegate( object sender, KeyProgramCommandsEventArgs e )
            {
                Assert.That( e.KeyProgram == kp );
                Assert.That( e.EventType == KeyProgramCommandsEventType.Updated );
                updateIndex = e.Index;
                updateCount++;
            };
            kp.CommandDeleted += delegate( object sender, KeyProgramCommandsEventArgs e )
            {
                Assert.That( e.KeyProgram == kp );
                Assert.That( e.EventType == KeyProgramCommandsEventType.Deleted );
                removeIndex = e.Index;
                removeCount++;
            };
            kp.CommandsCleared += delegate( object sender, KeyProgramCommandsEventArgs e )
            {
                Assert.That( e.KeyProgram == kp );
                Assert.That( e.Index == -1 );
                Assert.That( e.EventType == KeyProgramCommandsEventType.Cleared );
                isCleared = true;
            };

            kp.Commands.Add( "command1" );
            Assert.That( insertCount == 1 );
            Assert.That( insertIndex == 0 );
            Assert.That( kp.Commands.Count == 1 );

            kp.Commands.Add( "command2" );
            Assert.That( insertCount == 2 );
            Assert.That( insertIndex == 1 );
            Assert.That( kp.Commands.Count == 2 );

            kp.Commands.Add( "command3" );
            Assert.That( insertCount == 3 );
            Assert.That( insertIndex == 2 );
            Assert.That( kp.Commands.Count == 3 );
            Assert.That( kp.Commands.Contains( "command2" ) );
            Assert.That( !kp.Commands.Contains( "updated command 2" ) );

            kp.Commands[1] = "updated command 2";
            Assert.That( updateCount == 1 );
            Assert.That( updateIndex == 1 );
            Assert.That( kp.Commands.Count == 3 );
            Assert.That( kp.Commands.Contains( "updated command 2" ) );

            Assert.That( kp.Commands.IndexOf( "command3" ) == 2 );

            kp.Commands.Remove( "command3" );
            Assert.That( removeCount == 1 );
            Assert.That( removeIndex == 2 );
            Assert.That( kp.Commands.Count == 2 );

            kp.Commands.RemoveAt( 1 );
            Assert.That( removeCount == 2 );
            Assert.That( removeIndex == 1 );
            Assert.That( kp.Commands.Count == 1 );

            kp.Commands.Insert( 0, "command n°0" );
            Assert.That( insertCount == 4 );
            Assert.That( insertIndex == 0 );
            Assert.That( kp.Commands.Count == 2 );
            Assert.That( kp.Commands[1] == "command1" );

            kp.Commands.Insert( 0, "command n°-1" );
            Assert.That( insertCount == 5 );
            Assert.That( insertIndex == 0 );
            Assert.That( kp.Commands.Count == 3 );
            Assert.That( kp.Commands[2] == "command1" );

            kp.Commands.Insert( 1, "command between -1 and 0" );
            Assert.That( insertCount == 6 );
            Assert.That( insertIndex == 1 );
            Assert.That( kp.Commands.Count == 4 );
            Assert.That( kp.Commands[0] == "command n°-1" );
            Assert.That( kp.Commands[1] == "command between -1 and 0" );
            Assert.That( kp.Commands[2] == "command n°0" );
            Assert.That( kp.Commands[3] == "command1" );

            kp.Commands.Clear();
            Assert.That( isCleared == true );
            Assert.That( kp.Commands.Count == 0 );
        }

        [Test]
        public void KeyInteractionEventArgs()
        {
            IKey k = Context.Keyboards.Create( "test" ).Zones.Default.Keys.Create();

            IKeyProgram kp = k.Current.OnKeyDownCommands;
            kp.Commands.Add( "command1" );
            kp.Commands.Add( "command2" );
            kp.Commands.Add( "command3" );

            KeyInteractionEventArgs e = new KeyInteractionEventArgs( k, kp, KeyInteractionEventType.Down );

            Assert.That( e.Commands.Count == 3 );
            Assert.That( e.Commands[0] == "command1" );
            Assert.That( e.Commands[1] == "command2" );
            Assert.That( e.Commands[2] == "command3" );

            kp.Commands.Remove( "command2" );

            Assert.That( kp.Commands.Count == 2 );
            Assert.That( e.Commands.Count == 3 );
            Assert.That( e.Commands[0] == "command1" );
            Assert.That( e.Commands[1] == "command2" );
            Assert.That( e.Commands[2] == "command3" );
        }

    }
}

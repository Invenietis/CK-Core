#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Tests\Kernel\Contexts\DestroyEvents.cs) is part of CiviKey. 
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

using NUnit.Framework;
using CK.Keyboard.Model;
using CK.Keyboard;
using CK.SharedDic;
using System;
using CK.Plugin.Config;

namespace Keyboard
{
    [TestFixture]    
    //This class tests DestroyEvents, it strongly attached to the SharedDic, as a Destroy calls the SharedDictionnary to remove items linked to the detroyed object.
    public class DestroyEvents
    {
        KeyboardContext Context;

        [SetUp]
        public void Setup()
        {
            ISharedDictionary dic = SharedDictionary.Create( null );
            Context = new KeyboardContext() { ConfigContainer = dic };
        }

        [Test]
        public void DestroyKeyboard()
        {
            IKeyboard kb = Context.Keyboards.Create( "Test" );

            IZone z1 = kb.Zones.Create( "Zone1" );

            bool zoneDestroyed = false;
            bool keyboardDestroyed = false;
            kb.Zones.ZoneDestroyed += ( object sender, ZoneEventArgs e ) => zoneDestroyed = true ;
            Context.Keyboards.KeyboardDestroyed += ( object sender, KeyboardEventArgs e ) => keyboardDestroyed = true;

            kb.Destroy();

            Assert.That( keyboardDestroyed, Is.True );
            Assert.That( zoneDestroyed, Is.False );
        }

        [Test]
        public void DestroyZone()
        {
            IKeyboard kb = Context.Keyboards.Create( "Test" );

            IZone z1 = kb.Zones.Create( "Zone1" );

            z1.Keys.Create();

            bool zoneDestroyed = false;
            bool keyDestroyed = false;
            kb.Zones.ZoneDestroyed += ( object sender, ZoneEventArgs e ) => zoneDestroyed = true;
            kb.KeyDestroyed += ( object sender, KeyEventArgs e ) => keyDestroyed = true;

            z1.Destroy();

            Assert.That( zoneDestroyed, Is.True );
            Assert.That( keyDestroyed, Is.False );
        }

        [Test]
        public void DestroyKey()
        {
            IKeyboard kb = Context.Keyboards.Create( "Test" );
            IKey key = kb.Zones.Default.Keys.Create();
            IKeyMode aKey = key.KeyModes.Create( kb.CurrentMode );

            bool keyDestroyed = false;
            bool actualKeyDestroyed = false;
            kb.KeyDestroyed += ( object sender, KeyEventArgs e ) => keyDestroyed = true;
            key.KeyModes.KeyModeDestroyed += ( object sender, KeyModeEventArgs e ) => actualKeyDestroyed = true;

            key.Destroy();

            Assert.That( keyDestroyed, Is.True );
            Assert.That( actualKeyDestroyed, Is.False );
        }

        [Test]
        public void DestroyLayout()
        {
            IKeyboard kb = Context.Keyboards.Create( "Test" );
            IZone z1 = kb.Zones.Create( "Zone1" );
            ILayout lay = kb.Layouts.Create( "testLayout" );

            bool kbLayoutDestroyed = false;
            kb.Layouts.LayoutDestroyed += ( object sender, LayoutEventArgs e ) => kbLayoutDestroyed = true;

            lay.Destroy();

            Assert.That( kbLayoutDestroyed, Is.True );
        }

        [Test]
        public void DestroyingKeyboard()
        {           
            int nbKeyboardDestroyed = 0;
            int nbKeyboardChanged = 0;

            Context.Keyboards.KeyboardDestroyed += delegate( object sender, KeyboardEventArgs e )
            {
                nbKeyboardDestroyed++;
            };

            Assert.That( Context.Keyboards.Count, Is.EqualTo( 0 ) );
            Assert.That( Context.CurrentKeyboard, Is.Null );

            IKeyboard keyboard = Context.Keyboards.Create( "Kibôrde" );
            IKeyboard keyboard2 = Context.Keyboards.Create( "Kibôrde2" );

            Context.CurrentKeyboardChanged += delegate( object sender, CurrentKeyboardChangedEventArgs e )
            {
                ++nbKeyboardChanged;
            };

            Assert.That( Context.Keyboards.Count, Is.EqualTo( 2 ) );
            Assert.That( Context.Keyboards.Current, Is.EqualTo( keyboard ) );

            keyboard.Destroy();

            Assert.That( Context.Keyboards.Count, Is.EqualTo( 1 ) );
            Assert.That( Context.CurrentKeyboard, Is.Null );
            Assert.That( nbKeyboardChanged == 1 && nbKeyboardDestroyed == 1 );

            keyboard2.Destroy();

            Assert.That( Context.Keyboards.Count, Is.EqualTo( 0 ) );
            Assert.That( Context.CurrentKeyboard, Is.Null );
            Assert.That( nbKeyboardChanged == 1 && nbKeyboardDestroyed == 2 );
        }

        [Test]
        public void DestroyingLayout()
        {

            IKeyboard keyboard = Context.Keyboards.Create( "Idiotheque" );

            ILayout secondLayout = null;
            secondLayout = keyboard.Layouts.Create( "Second layout" );
            Assert.That( secondLayout == keyboard.Layouts["Second layout"] && secondLayout != keyboard.Layouts.Default, "Not the default yet." );
            keyboard.Layouts.Current = secondLayout;

            int nbLayoutDestroyed = 0;
            int nbLayoutChanged = 0;

            keyboard.Layouts.LayoutDestroyed += delegate( object sender, LayoutEventArgs e )
            {
                ++nbLayoutDestroyed;
                Assert.That( nbLayoutChanged == 1, "Destroy fires after CurrentLayoutChangedEvent." );
            };

            keyboard.Layouts.CurrentChanged += delegate( object sender, KeyboardCurrentLayoutChangedEventArgs e )
            {
                ++nbLayoutChanged;
                Assert.That( nbLayoutDestroyed == 0, "Destroy fires after CurrentLayoutChangedEvent." );
            };

            keyboard.Layouts.Current = keyboard.Layouts.Default;

            secondLayout.Destroy();
            Assert.That( nbLayoutChanged == 1 && nbLayoutDestroyed == 1, "Events (CurrentChange first and then destroyed) fired" );

            bool exception = false;
            try
            {
                keyboard.Layouts.Default.Destroy();
            }
            catch ( Exception )
            {
                exception = true;
            }
            Assert.That( exception, "Destroying Default layout triggers an error." );
        }
    }
}

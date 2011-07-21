#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Tests\Kernel\Modes\KeyboardModes.cs) is part of CiviKey. 
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
using CK.Core;
using CK.Keyboard.Model;
using CK.Keyboard;
using CK.Plugin;

namespace Keyboard
{
    [TestFixture]
    public class KeyboardModes
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void AvailableModes()
        {
            bool availableModeChangedCalled = false;
            bool availableModeChangingCalled = false;
            bool currentModeChangingCalled = false;
            bool currentModeChangedCalled = false;

            bool rejectCurrentModeChange = false;

            KeyboardContext Context = new KeyboardContext();
            IKeyboard kb = Context.Keyboards.Create( "Test" );

            kb.CurrentModeChanging += ( object sender, KeyboardModeChangingEventArgs e ) =>
            {
                currentModeChangingCalled = true;
                if( rejectCurrentModeChange ) e.Cancel = true;
            };
            kb.CurrentModeChanged += ( object sender, KeyboardModeChangedEventArgs e ) => { currentModeChangedCalled = true; };
            kb.AvailableModeChanging += ( object sender, KeyboardModeChangingEventArgs e ) => { availableModeChangingCalled = true; };
            kb.AvailableModeChanged += ( object sender, KeyboardModeChangedEventArgs e ) => { availableModeChangedCalled = true; };            

            IKeyboardMode mode1 = Context.ObtainMode( "Beta" );
            IKeyboardMode mode2 = Context.ObtainMode( "Alpha" );
            Assert.That( mode2.CompareTo( mode1 ) > 0, "Alpha is a stronger mode than Beta." );

            IZone zone = kb.Zones.Create( "Zone" );
            IKey key = zone.Keys.Create();
            Assert.Throws<CKException>( ()=> key.KeyModes.Create( mode1 ) );
            Assert.Throws<CKException>( ()=> key.KeyModes.Create( mode2 ) );

            kb.AvailableMode = kb.AvailableMode.Add( mode1 );
            Assert.That( availableModeChangingCalled ); availableModeChangingCalled = false;
            Assert.That( availableModeChangedCalled ); availableModeChangedCalled = false;

            IKeyMode keyMode1 = key.KeyModes.Create( mode1 );
            Assert.Throws<CKException>( () => key.KeyModes.Create( mode2 ) );

            kb.AvailableMode = kb.AvailableMode.Add( mode2 );
            Assert.That( availableModeChangingCalled ); availableModeChangingCalled = false;
            Assert.That( availableModeChangedCalled ); availableModeChangedCalled = false;
            IKeyMode keyMode2 = key.KeyModes.Create( mode2 );

            kb.CurrentMode = mode1;
            Assert.That( currentModeChangingCalled ); currentModeChangingCalled = false;
            Assert.That( currentModeChangedCalled ); currentModeChangedCalled = false;
            Assert.That( key.Current == keyMode1 );

            rejectCurrentModeChange = true;
            kb.CurrentMode = mode2;
            Assert.That( currentModeChangingCalled ); currentModeChangingCalled = false;
            Assert.That( currentModeChangedCalled == false );
            Assert.That( kb.CurrentMode == mode1 );

            rejectCurrentModeChange = false;
            kb.CurrentMode = mode2;
            Assert.That( currentModeChangingCalled ); currentModeChangingCalled = false;
            Assert.That( currentModeChangedCalled ); currentModeChangedCalled = false;
            Assert.That( key.Current == keyMode2 );

            kb.CurrentMode = mode1;
            Assert.That( key.Current == keyMode1 );

            // Setting Alpha+Beta : Alpha (mode2) wins.
            kb.CurrentMode = mode1.Add( mode2 );
            Assert.That( key.Current == keyMode2 );
            
            kb.CurrentMode = null;
            Assert.That( key.Current != keyMode1 && key.Current != keyMode2 );
            Assert.That( key.Current.Mode.IsEmpty );
        }


        [Test]
        public void KeyModeSwap()
        {
            KeyboardContext Context = new KeyboardContext();
            Context.Keyboards.Create( "Keyboard" );
            CK.Keyboard.Keyboard kb = Context.CurrentKeyboard;

            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "frigo" ) );
            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "bingo" ) );
            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode( "yoyo" ) );
            kb.AvailableMode = kb.AvailableMode.Add( Context.ObtainMode("flagello") );

            IZone zone = kb.Zones.Create( "Zone" );
            IKey key = zone.Keys.Create();
            IKeyMode keyMode1 = key.KeyModes.Create( Context.EmptyMode );
            keyMode1.Description = "keyMode1";
            IKeyMode keyMode2 = key.KeyModes.Create( Context.ObtainMode( "frigo" ) );
            keyMode2.Description = "keyMode2";
            IKeyMode keyMode3 = key.KeyModes.Create( Context.ObtainMode( "bingo" ) );
            keyMode3.Description = "keyMode3";
            IKeyMode keyMode4 = key.KeyModes.Create( Context.ObtainMode( "yoyo" ) );
            keyMode4.Description = "keyMode4";

            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "" ) ), Is.EqualTo( keyMode1 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "frigo" ) ), Is.EqualTo( keyMode2 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "bingo" ) ), Is.EqualTo( keyMode3 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "yoyo" ) ), Is.EqualTo( keyMode4 ) );

            Assert.That( keyMode1.ChangeMode( Context.ObtainMode( "flagello" ) ), Is.False );            

            keyMode1.SwapModes( keyMode2 );

            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "frigo" ) ), Is.EqualTo( keyMode1 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "" ) ), Is.EqualTo( keyMode2 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "bingo" ) ), Is.EqualTo( keyMode3 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "yoyo" ) ), Is.EqualTo( keyMode4 ) );

            keyMode1.SwapModes( keyMode4 );

            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "yoyo" ) ), Is.EqualTo( keyMode1 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "" ) ), Is.EqualTo( keyMode2 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "bingo" ) ), Is.EqualTo( keyMode3 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "frigo" ) ), Is.EqualTo( keyMode4 ) );

            keyMode3.SwapModes( keyMode2 );

            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "yoyo" ) ), Is.EqualTo( keyMode1 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "bingo" ) ), Is.EqualTo( keyMode2 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "" ) ), Is.EqualTo( keyMode3 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "frigo" ) ), Is.EqualTo( keyMode4 ) );

            keyMode1.SwapModes( keyMode3 );

            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "" ) ), Is.EqualTo( keyMode1 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "bingo" ) ), Is.EqualTo( keyMode2 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "yoyo" ) ), Is.EqualTo( keyMode3 ) );
            Assert.That( key.KeyModes.FindBest( Context.ObtainMode( "frigo" ) ), Is.EqualTo( keyMode4 ) );


        }        
    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Tests\Kernel\Layouts\Layouts.cs) is part of CiviKey. 
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
using CK.Core;

namespace Keyboard
{
    [TestFixture]
    public class Layouts
    {
        KeyboardContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new KeyboardContext();
            Context.Keyboards.Create( "Keyboard" );            
            CK.Keyboard.Keyboard kb = Context.CurrentKeyboard;
            IZone zone = kb.Zones.Create( "Zone" );
            IKey key = zone.Keys.Create();
            IKeyMode actualKey = key.KeyModes.Create( Context.EmptyMode );

            ILayout layout = kb.Layouts.Create( "Layout" );


        }

        IKeyboard Keyboard
        {
            get { return Context.Keyboards["Keyboard"]; }
        }

        [Test]
        public void ChangeLayouts()
        {
            ILayout keyboardLayout = Keyboard.CurrentLayout;
            ILayout keyboardLayout2 = Keyboard.Layouts.Create( "Layout2" );

            Assert.That( keyboardLayout.IsDefault );
            Assert.That( Keyboard.CurrentLayout.LayoutZones["Zone"] == keyboardLayout.LayoutZones["Zone"] );
            Assert.That( Keyboard.CurrentLayout.LayoutZones["Zone"].LayoutKeys[0] == keyboardLayout.LayoutZones["Zone"].LayoutKeys[0] );
            Assert.That( Keyboard.CurrentLayout.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes[Context.EmptyMode] == keyboardLayout.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes[Context.EmptyMode] );

            Keyboard.CurrentLayout = keyboardLayout2;
            Assert.That( Keyboard.CurrentLayout == keyboardLayout2 );
            Assert.That( Keyboard.CurrentLayout.LayoutZones["Zone"] == keyboardLayout2.LayoutZones["Zone"] );
            Assert.That( Keyboard.CurrentLayout.LayoutZones["Zone"].LayoutKeys[0] == keyboardLayout2.LayoutZones["Zone"].LayoutKeys[0] );
            Assert.That( Keyboard.CurrentLayout.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes[Context.EmptyMode] == keyboardLayout2.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes[Context.EmptyMode] );
        }

        [Test]
        public void LayoutKeyModeEvents()
        {
            int created = 0;
            int changed = 0;
            int destroyed = 0;
            
            CK.Keyboard.Keyboard Keyboard = Context.CurrentKeyboard;
            Layout layoutKeyboard = Keyboard.CurrentLayout;
            Keyboard.AvailableMode = Keyboard.AvailableMode.Add( Context.ObtainMode( "frigo" ) );
            Keyboard.AvailableMode = Keyboard.AvailableMode.Add( Context.ObtainMode( "lego" ) );
            Keyboard.AvailableMode = Keyboard.AvailableMode.Add( Context.ObtainMode( "micro" ) );

            Context.ObtainMode( "frigo" );

            layoutKeyboard.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes.LayoutKeyModeCreated += ( o, e ) => created++;
            layoutKeyboard.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes.LayoutKeyModeModeChanged += ( o, e ) => changed++;
            layoutKeyboard.LayoutZones["Zone"].LayoutKeys[0].LayoutKeyModes.LayoutKeyModeDestroyed += ( o, e ) => destroyed++;

            LayoutZone layoutZone = layoutKeyboard.FindOrCreate( Keyboard.Zones["Zone"] );
            LayoutKey layoutKey = layoutZone[0];

            LayoutKeyMode firstLayoutKeyMode = layoutKey.Find( Context.ObtainMode( "" ) );

            Assert.That( firstLayoutKeyMode.ChangeMode( Context.ObtainMode( "frigo" ) ), Is.False );

            layoutKey.FindOrCreate( Context.ObtainMode( "frigo" ) );
            Assert.That( created == 1 );
            Assert.That( layoutKey.Find( Context.ObtainMode( "frigo" ) ), Is.Not.Null );
            LayoutKeyMode secondLayoutKeyMode = layoutKey.Find( Context.ObtainMode( "frigo" ) );

            Assert.That( layoutKey.Find( Context.ObtainMode( "" ) ), Is.Not.Null );
            Assert.That( layoutKey.Find( Context.ObtainMode( "frigo" ) ), Is.Not.Null );

            firstLayoutKeyMode.SwapModes( secondLayoutKeyMode );

            Assert.That( changed == 2 );

            Assert.That( layoutKey.Find( Context.ObtainMode( "" ) ), Is.Not.Null );
            Assert.That( layoutKey.Find( Context.ObtainMode( "frigo" ) ), Is.Not.Null );
            Assert.That( firstLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "frigo" ) ) );
            Assert.That( secondLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "" ) ) );

            LayoutKeyMode currentKeyMode = layoutKey.Current;
            layoutKey.Current.Destroy();
            Assert.That( destroyed == 1 );
            Assert.That( layoutKey.Current, Is.Not.Null );
        }

        [Test]
        public void LayoutKeyModesSwap()
        {
            CK.Keyboard.Keyboard Keyboard = Context.CurrentKeyboard;
            Layout layoutKeyboard = Keyboard.CurrentLayout;

            LayoutZone layoutZone = layoutKeyboard.FindOrCreate( Keyboard.Zones["Zone"] );
            LayoutKey layoutKey = layoutZone[0];
            LayoutKeyMode firstLayoutKeyMode = layoutKey.Find( Context.ObtainMode( "" ) );

            Assert.That( firstLayoutKeyMode.ChangeMode( Context.ObtainMode( "frigo" ) ), Is.False );

            //Creating a new KeyMode, linked to the "frigo" mode
            Keyboard.AvailableMode = Keyboard.AvailableMode.Add( Context.ObtainMode( "frigo" ) );
            layoutKey.FindOrCreate( Context.ObtainMode( "frigo" ) );
            LayoutKeyMode secondLayoutKeyMode = layoutKey.Find( Context.ObtainMode( "frigo" ) );

            //Swap two KeyModes that are next to each other in the linked chain
            firstLayoutKeyMode.SwapModes( secondLayoutKeyMode );

            Assert.That( layoutKey.Find( Context.ObtainMode( "" ) ), Is.Not.Null );
            Assert.That( layoutKey.Find( Context.ObtainMode( "frigo" ) ), Is.Not.Null );

            Assert.That( firstLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "frigo" ) ) );
            Assert.That( secondLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "" ) ) );

            //Creating a new KeyMode, linked to the "frigo" mode
            Keyboard.AvailableMode = Keyboard.AvailableMode.Add( Context.ObtainMode( "bingo" ) );
            layoutKey.Create( Context.ObtainMode( "bingo" ) );
            LayoutKeyMode thirdLayoutKeyMode = layoutKey.Find( Context.ObtainMode( "bingo" ) );

            //Swap two KeyModes that are not next to each other in the linked chain.
            //One of them is the first item of the chain, and the other is the last item of the chain.
            firstLayoutKeyMode.SwapModes( thirdLayoutKeyMode );

            Assert.That( layoutKey.Find( Context.ObtainMode( "frigo" ) ), Is.Not.Null );
            Assert.That( layoutKey.Find( Context.ObtainMode( "bingo" ) ), Is.Not.Null );

            Assert.That( firstLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "bingo" ) ) );
            Assert.That( thirdLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "frigo" ) ) );

            //Same swap, but with swapped parameters
            thirdLayoutKeyMode.SwapModes( firstLayoutKeyMode );

            Assert.That( layoutKey.Find( Context.ObtainMode( "frigo" ) ), Is.Not.Null );
            Assert.That( layoutKey.Find( Context.ObtainMode( "bingo" ) ), Is.Not.Null );

            Assert.That( firstLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "frigo" ) ) );
            Assert.That( thirdLayoutKeyMode.Mode, Is.EqualTo( Context.ObtainMode( "bingo" ) ) );

        }
    }
}

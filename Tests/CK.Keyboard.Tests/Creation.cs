#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Tests\Kernel\Contexts\Creation.cs) is part of CiviKey. 
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
using System.Drawing;
using CK.Keyboard.Model;
using CK.Keyboard;
using CK.SharedDic;

namespace Keyboard
{
    [TestFixture]
    public class Creation
    {
        KeyboardContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new KeyboardContext( null ); 
        }

        int _eventOrder;

        [Test]
        public void CreateKeyboardFromScratch()
        {
            Context.Keyboards.KeyboardCreated += Keyboards_KeyboardCreated;
            Context.Keyboards.KeyboardRenamed += Keyboards_KeyboardRenamed;

            Context.CurrentKeyboardChanged += Context_CurrentKeyboardChanged;
            Context.Keyboards.CurrentChanged += Keyboards_CurrentChanged;

            Assert.That( Context.Keyboards.Count == 0, "No keyboards at the beginning." );
            Assert.That( Context.CurrentKeyboard == null, "No current keyboard." );

            IKeyboard k = Context.Keyboards.Create( "Murfn..." );
            k.Rename( "Another Murfn!" );
            //k.Destroy();

            Assert.That( _eventOrder == 4, "All expected events (and no more) have been processed." );

        }

        void Keyboards_KeyboardCreated( object sender, KeyboardEventArgs e )
        {
            Assert.That( _eventOrder++ == 0, "First event is Created." );
            Assert.That( e.Keyboard == Context.Keyboards["Murfn..."], "Murfn is Created." );
        }

        void Context_CurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            Assert.That( _eventOrder++ == 1, "Then the current is set." );
            Assert.That( e.Previous == null, "No current before." );
            Assert.That( e.Current == Context.Keyboards["Murfn..."], "Murfn is the current one." );
            // Removes this handler.
            Context.Keyboards.CurrentChanged -= Context_CurrentKeyboardChanged;
        }

        void Keyboards_CurrentChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( _eventOrder == 2 )
            {
                Assert.That( _eventOrder++ == 2, "Second subscription to the CurrentChanged event." );
                Assert.That( e.Previous == null, "No current before." );
                Assert.That( e.Current == Context.Keyboards["Murfn..."], "Murfn is the current one." );
            }
            //else
            //{
            //    Assert.That( _eventOrder++ == 4, "When destroying the last one Current becomes null." );
            //    Assert.That( e.Previous.Name == "Another Murfn!", "Bye bye the keyboard." );
            //    Assert.That( Context.Keyboards["Another Murfn!"] == e.Previous, "The keyboard is still here and functionnal." );
            //    Assert.That( e.Current == null, "No more current keyboard." );
            //}
        }

        void Keyboards_KeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            Assert.That( _eventOrder++ == 3, "Renamed..." );
            Assert.That( e.PreviousName == "Murfn...", "Was Murfn." );
            Assert.That( e.Keyboard.Name == "Another Murfn!", "New name." );
        }

        //void Keyboards_KeyboardDestroyed( object sender, KeyboardEventArgs e )
        //{
        //    Assert.That( _eventOrder++ == 5, "Renamed: after set current to null." );
        //    Assert.That( Context.Keyboards.Count == 0, "No more keyboards." );
        //}

        [Test]
        public void KeyboardNameClashes()
        {
            IKeyboard k = Context.Keyboards.Create( "First one" );
            Assert.That( k.Rename( "First one" ) == "First one",        "Same name always works." );
            Assert.That( k.Name == "First one",                         "Rename did nothing." );
            Assert.That( k.Rename( "First one (2)" ) == "First one",    "Pattern is processed (and removed)." );
            Assert.That( k.Name == "First one",                         "Rename removed the numbering pattern." );
            Assert.That( Context.CurrentKeyboard == k,                  "First keyboard became the current one." );
            
            IKeyboard k2 = Context.Keyboards.Create( "First one" );
            Assert.That( k2.Name == "First one (1)",                            "Creation use the numbering pattern." );
            Assert.That( k2.Rename( "First one" ) == "First one (1)",           "Slot is occupied." );
            Assert.That( k2.Rename( "First one (3712)" ) == "First one (1)",    "Numbering pattern is processed and slot 0 is occupied." );
            Assert.That( k2.Name == "First one (1)",                            "Rename choose the 1 slot." );
            Assert.That( Context.CurrentKeyboard == k,                          "Second keyboard does not change the current one." );
            
            //k.Destroy();
            //Assert.That( Context.CurrentKeyboard == null, "Destroying the current one sets the current to null (even if other exist)." );
            
            //k2.Destroy();

        }
        
        [Test]
        public void ZoneNameClashes()
        {
            IKeyboard k = Context.Keyboards.Create( "First one" );
            
            Assert.That( k.Zones.Count == 1, "The default zone exists." );
            Assert.That( k.Layouts.Count == 1, "The default layout exists." );
            Assert.That( k.Zones[""] != null, "The default zone's name is the empty string." );
            Assert.That( k.Layouts[""] != null, "The default layout's name is the empty string." );

            IZone zDefault = k.Zones.Default;
            IZone zAlpha = k.Zones.Create( "Alpha" );
            Assert.That( k.Zones.Count == 2 && zAlpha != zDefault, "Default & Alpha zones." );
            Assert.That( zDefault.IsDefault && !zAlpha.IsDefault, "IsDefault works." ); 
            
            Assert.That( zAlpha.Keys.Count == 0 && zDefault.Keys.Count == 0, "Default & Alpha zones are empty." );
            
            Assert.That( zAlpha.Rename( "" ) == " (1)", "default name is locked." );
            Assert.That( zAlpha.Name == " (1)", "Rename did its job." );
            Assert.That( zAlpha.Rename( "Alpha (3712)" ) == "Alpha", "Pattern is processed (and removed)." );
            Assert.That( zAlpha.Name == "Alpha", "Rename removed the numbering pattern." );

            IZone zAlpha2 = k.Zones.Create( "Alpha" );
            Assert.That( k.Zones.Count == 3 && zAlpha != zAlpha2, "Alpha & Alpha2 are different zones." );
            Assert.That( zAlpha2.Name == "Alpha (1)", "Rename did its job: Alpha2 has been auto numbered." );

            Assert.Throws( typeof( InvalidOperationException ), delegate() { zDefault.Rename( "Bing" ); }, "This triggers an exception: the default zone can not be renamed." );
        }

        [Test]
        public void SecondLayout()
        {
            IKeyboard keyboard = Context.Keyboards.Create( "Idiotheque" );

            int nbLayoutCreated = 0;
            keyboard.Layouts.LayoutCreated += delegate( object sender, LayoutEventArgs e )
            {
                Assert.That( e.Layout != keyboard.Layouts.Default, "Other than default." );
                ++nbLayoutCreated;
            };

            int nbZoneCreated = 0;
            keyboard.Zones.ZoneCreated += delegate( object sender, ZoneEventArgs e )
            {
                Assert.That( e.Zone != keyboard.Zones.Default, "Other than default." );
                ++nbZoneCreated;
            };

            IZone z0 = keyboard.Zones.Default;
            IZone z1 = keyboard.Zones.Create( "z1" );
            IZone z2 = keyboard.Zones.Create( "z2" );
            Assert.That( nbZoneCreated == 2, "Event fired" );

            for( int i = 0; i < 10; ++i )
            {
                IKey k0 = z0.Keys.Create();
                IKey k1 = z1.Keys.Create();
                IKey k2 = z2.Keys.Create();
                k0.CurrentLayout.Current.X = 100 + i;
                k1.CurrentLayout.Current.Y = 1000 + i;
                k2.CurrentLayout.Current.Width = 10000 + i;
            }

            ILayout secondLayout = null;

            int nbLayoutDestroyed = 0;
            int nbLayoutChanged = 0;

            keyboard.Layouts.LayoutDestroyed += delegate( object sender, LayoutEventArgs e )
            {
                ++nbLayoutDestroyed;
                Assert.That( nbLayoutChanged == 4, "Destroy fires after CurrentLayoutChangedEvent." );
            };

            keyboard.Layouts.CurrentChanged += delegate( object sender, KeyboardCurrentLayoutChangedEventArgs e )
            {
                Assert.That(
                    ((nbLayoutChanged & 1) == 0 && e.Previous == keyboard.Layouts.Default)
                    || ((nbLayoutChanged & 1) == 1 && e.Previous == secondLayout), "From default to second and back." );
                ++nbLayoutChanged;
                Assert.That( nbLayoutDestroyed == 0, "Destroy fires after CurrentLayoutChangedEvent." );
            };

            secondLayout = keyboard.Layouts.Create( "Second layout" );
            Assert.That( nbLayoutCreated == 1, "Event fired" );
            Assert.That( secondLayout == keyboard.Layouts["Second layout"] && secondLayout != keyboard.Layouts.Default, "Not the default yet." );

            for( int i = 0; i < 10; ++i )
            {
                Assert.That( z0.Keys[i].CurrentLayout.Current.X == 100 + i, "Via CurrentActualLayout" );
                Assert.That( z1.Keys[i].CurrentLayout.Current.Y == 1000 + i, "Via Current.CurrentLayout" );
                Assert.That( z2.Keys[i].CurrentLayout.Current.Width == 10000 + i, "Via CurrentLayout.Current" );
            }

            keyboard.Layouts.Current = secondLayout;
            Assert.That( nbLayoutChanged == 1, "Event fired" );

            for( int i = 0; i < 10; ++i )
            {
                Assert.That( z0.Keys[i].CurrentLayout.Current.X == 0, "Via CurrentActualLayout: 0 in 2nd layout" );
                Assert.That( z1.Keys[i].CurrentLayout.Current.Y == 0, "Via Current.CurrentLayout: 0 in 2nd layout" );
                Assert.That( z2.Keys[i].CurrentLayout.Current.Width == 0, "Via CurrentLayout.Current: 0 in 2nd layout" );
            }

            keyboard.Layouts.Current = keyboard.Layouts.Default;
            Assert.That( nbLayoutChanged == 2, "Event fired" );

            int nbLayoutResized = 0;
            keyboard.Layouts.LayoutSizeChanged += delegate( object sender, LayoutEventArgs e )
            {
                Assert.That( e.Layout.W == 98 || e.Layout.H == 89, "New Height and Width." );
                ++nbLayoutResized;
            };

            keyboard.Layouts.Current.H = 89;
            Assert.That( nbLayoutResized == 1, "Resize H event fired." );
            keyboard.Layouts.Current.W = 98;
            Assert.That( nbLayoutResized == 2, "Resize W event fired." );

            for( int i = 0; i < 10; ++i )
            {
                Assert.That( z0.Keys[i].CurrentLayout.Current.X == 100 + i, "Via Current.CurrentLayout: resizing layout has no effect." );
                Assert.That( z1.Keys[i].CurrentLayout.Current.Y == 1000 + i, "Via CurrentLayout.Current: resizing layout has no effect." );
                Assert.That( z2.Keys[i].CurrentLayout.Current.Width == 10000 + i, "Via CurrentActualLayout: resizing layout has no effect." );
            }

            keyboard.Layouts.Current = secondLayout;
            Assert.That( nbLayoutChanged == 3, "Event fired" );
        }       
    }
}

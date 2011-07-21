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
using CK.Context;
using CK.Plugin.Config;
using System.IO;
using CK.Core;

namespace Keyboard
{
    [TestFixture]
    public class Config
    {
        [TearDown]
        public void TearDown()
        {
            TestBase.CleanupTestDir();
        }

        [Test]
        public void ReloadPreviousCurrentKeyboard()
        {
            INamedVersionedUniqueId keyboardPluginId = KeyboardContext.PluginId;

            {
                int keyboardCreated = 0;
                IContext ctx = TestBase.CreateContext();

                Assert.That( ctx.ConfigManager.UserConfiguration != null, "Loads system and user configurations." );

                IKeyboardContext kbCtx = new KeyboardContext() { ConfigContainer = ctx.ConfigManager.Extended.Container };

                kbCtx.Keyboards.KeyboardCreated += ( sender, e ) => keyboardCreated++;

                IKeyboard kb1 = kbCtx.Keyboards.Create( "Keyboard1" );
                IKeyboard kb2 = kbCtx.Keyboards.Create( "Keyboard2" );
                IKeyboard kb3 = kbCtx.Keyboards.Create( "Keyboard3" );

                Assert.That( keyboardCreated == 3 );

                kbCtx.CurrentKeyboard = kb2;

                ctx.ConfigManager.Extended.Container[ctx, keyboardPluginId, "KeyboardContext"] = kbCtx;

                TestBase.Host.ContextPath = TestBase.GetTestFilePath( "Context" );
                ctx.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( "TestProfile", TestBase.GetTestFilePath( "UserConfiguration" ), ConfigSupportType.File, true );

                TestBase.Host.SaveContext();
                TestBase.Host.SaveUserConfig();
                TestBase.Host.SaveSystemConfig();
            }
            {
                IContext ctx = TestBase.CreateContext();
                Assert.That( ctx.ConfigManager.UserConfiguration != null, "Loads system and user configurations." );
                Assert.That( TestBase.Host.RestoreLastContext() );

                IConfigContainer config = ctx.ConfigManager.Extended.Container;

                IKeyboardContext kbCtx = (IKeyboardContext)config[ ctx, keyboardPluginId, "KeyboardContext" ];
                Assert.That( kbCtx, Is.Null, "Since keyboard plugin is not active." );

                config.Ensure( keyboardPluginId );

                kbCtx = (IKeyboardContext)config[ctx, keyboardPluginId, "KeyboardContext"];
                Assert.That( kbCtx, Is.Not.Null, "Now we find it." );
                
                Assert.That( kbCtx.Keyboards.Count == 3 );
                Assert.That( kbCtx.CurrentKeyboard.Name == "Keyboard2" );
            }
        }
    }
}

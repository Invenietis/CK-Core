//#region LGPL License
///*----------------------------------------------------------------------------
//* This file (Tests\CK.Core.Tests\AppSettingsTests.cs) is part of CiviKey. 
//*  
//* CiviKey is free software: you can redistribute it and/or modify 
//* it under the terms of the GNU Lesser General Public License as published 
//* by the Free Software Foundation, either version 3 of the License, or 
//* (at your option) any later version. 
//*  
//* CiviKey is distributed in the hope that it will be useful, 
//* but WITHOUT ANY WARRANTY; without even the implied warranty of
//* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
//* GNU Lesser General Public License for more details. 
//* You should have received a copy of the GNU Lesser General Public License 
//* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
//*  
//* Copyright © 2007-2015, 
//*     Invenietis <http://www.invenietis.com>,
//*     In’Tech INFO <http://www.intechinfo.fr>,
//* All rights reserved. 
//*-----------------------------------------------------------------------------*/
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using NUnit.Framework;

//namespace CK.Core.Tests
//{
//    [TestFixture]
//    public class AppSettingsTests
//    {

//        [SetUp]
//        public void ClearAppSettingsSection()
//        { 
//            RemoveConfigurationManagerSettings( "None" );
//            RemoveConfigurationManagerSettings( "Test" );
//        }

//        [Test]
//        public void initializing_with_a_function_getter()
//        {
//            AppSettings settings = new AppSettings();
//            settings.Initialize( s => s + "OK" );
//            Assert.That( settings["Test"], Is.EqualTo( "TestOK" ) );
//        }
        
//        [Test]
//        public void get_with_default_value()
//        {
//            AppSettings settings = new AppSettings();
//            settings.Initialize( s => s == "Test" ? (object)3712 : null );
//            Assert.That( settings.Get( "Test" ), Is.EqualTo( 3712 ) );
//            Assert.That( settings.Get<int>( "Test", -5 ), Is.EqualTo( 3712 ) );
//            Assert.That( settings.Get<int>( "None", -5 ), Is.EqualTo( -5 ) );

//            Assert.Throws<CKException>( () => Console.Write( settings.GetRequired( "None" ) ) );
//            Assert.Throws<CKException>( () => Console.Write( settings.GetRequired<int>( "None" ) ) );

//            Assert.That( settings.Get<float>( "Test", -8 ), Is.EqualTo( -8.0 ) );
//            Assert.Throws<CKException>( () => settings.GetRequired<float>( "None" ) );
            
//            Assert.That( settings["Test"], Is.EqualTo( "3712" ) );
//        }

//        [Test]
//        public void AppSettings_can_be_initialized_once_and_only_once()
//        {
//            AppSettings settings = new AppSettings();
//            settings.Initialize( s => s == "Test" );
//            Assert.Throws<CKException>( () => settings.Initialize( s => s == "Test" ) );
//        }

//        [Test]
//        public void AppSettings_can_be_overridden_and_restored()
//        {
//            AppSettings settings = new AppSettings();
//            settings.Initialize( s => s == "Test" ? "OK" : null );
//            Assert.That( settings["Test"], Is.EqualTo( "OK" ) );
//            Assert.That( settings["Other"], Is.Null );

//            settings.Override( ( previous, key ) => previous( key ) + "-Suffix" );
//            Assert.That( settings["Test"], Is.EqualTo( "OK-Suffix" ) );
//            Assert.That( settings["Other"], Is.EqualTo( "-Suffix" ) );

//            settings.Override( ( previous, key ) => "Prefix-" + previous( key ) );
//            Assert.That( settings["Test"], Is.EqualTo( "Prefix-OK-Suffix" ) );
//            Assert.That( settings["Other"], Is.EqualTo( "Prefix--Suffix" ) );

//            settings.RevertOverrides();
//            Assert.That( settings["Test"], Is.EqualTo( "OK" ) );
//            Assert.That( settings["Other"], Is.Null );
//        }

//        [Test]
//        public void using_unconfigured_AppSettings_binds_to_ConfigurationManager()
//        {
//            AppSettings settings = new AppSettings();
//            // Here, ConfigurationManager has been late bound.
//            Assert.That( settings[ "None" ], Is.Null );
//        }

//        [Test]
//        public void default_initialization_on_ConfigurationMananger_is_dynamic()
//        {
//            AppSettings settings = new AppSettings();
//            // Here, ConfigurationManager has been late bound.
//            Assert.That( settings[ "Test" ], Is.Null );
//            // Checks that this auto-configuration is "dynamic".
//            SetConfigurationManagerSettings( "Test", "Murfn" );
//            Assert.That( settings["Test"], Is.EqualTo( "Murfn" ) );
//            RemoveConfigurationManagerSettings( "Test" );
//            Assert.That( settings["Test"], Is.Null );
//        }

//        [Test]
//        public void Override_of_the_DefaultInitialization_is_possible()
//        {
//            AppSettings settings = new AppSettings();
//            SetConfigurationManagerSettings( "Test", "Murfn will be overridden." );
//            Assert.That( settings["Test"], Is.EqualTo( "Murfn will be overridden." ) );
//            settings.Override( ( legacy, key ) =>
//            {
//                if( key == "Test" )
//                {
//                    Assert.That( legacy( key ), Is.EqualTo( "Murfn will be overridden." ) );
//                    return "Overide Murfn...";
//                }
//                return legacy( key );
//            } );
//            Assert.That( settings["Test"], Is.EqualTo( "Overide Murfn..." ) );
//            settings.RevertOverrides();
//            Assert.That( settings["Test"], Is.EqualTo( "Murfn will be overridden." ) );
//            RemoveConfigurationManagerSettings( "Test" );
//            Assert.That( settings["Test"], Is.Null );
//            settings.Override( ( legacy, key ) =>
//            {
//                if( key == "Test" )
//                {
//                    Assert.That( legacy( key ), Is.Null );
//                    return "Overide Murfn... 2";
//                }
//                return legacy( key );
//            } );
//            Assert.That( settings["Test"], Is.EqualTo( "Overide Murfn... 2" ) );
//            settings.RevertOverrides();
//            Assert.That( settings["Test"], Is.Null );
//        }

//        void SetConfigurationManagerSettings( string key, string text )
//        {
//            Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
//            KeyValueConfigurationElement e = config.AppSettings.Settings[key];
//            if( e != null ) e.Value = text;
//            else config.AppSettings.Settings.Add( key, text );
//            config.Save( ConfigurationSaveMode.Modified );
//            ConfigurationManager.RefreshSection( "appSettings" );
//        }
        
//        void RemoveConfigurationManagerSettings( string key )
//        {
//            Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
//            config.AppSettings.Settings.Remove( key );
//            config.Save( ConfigurationSaveMode.Modified );
//            ConfigurationManager.RefreshSection( "appSettings" );
//        }
//    }
//}

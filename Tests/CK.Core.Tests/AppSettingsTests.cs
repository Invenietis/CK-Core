using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class AppSettingsTests
    {

        [SetUp]
        public void ClearAppSettingsSection()
        {
            RemoveConfigurationManagerSettings( "None" );
            RemoveConfigurationManagerSettings( "Test" );
        }

        [Test]
        public void StandardStringInitialization()
        {
            AppSettings settings = new AppSettings();
            settings.Initialize( s => s + "OK" );
            Assert.That( settings["Test"], Is.EqualTo( "TestOK" ) );
        }
        
        [Test]
        public void StandardObjectInitialization()
        {
            AppSettings settings = new AppSettings();
            settings.Initialize( s => s == "Test" ? (object)3712 : null );
            Assert.That( settings.GetObject( "Test" ), Is.EqualTo( 3712 ) );
            Assert.That( settings.GetObject<int>( "Test", -5 ), Is.EqualTo( 3712 ) );
            Assert.That( settings.GetObject<int>( "None", -5 ), Is.EqualTo( -5 ) );

            Assert.Throws<CKException>( () => Console.Write( settings.GetRequiredObject( "None" ) ) );
            Assert.Throws<CKException>( () => Console.Write( settings.GetRequiredObject<int>( "None" ) ) );

            Assert.That( settings.GetObject<float>( "Test", -8 ), Is.EqualTo( -8.0 ) );
            Assert.Throws<CKException>( () => settings.GetRequiredObject<float>( "None" ) );
            
            Assert.That( settings["Test"], Is.EqualTo( "3712" ) );
        }

        [Test]
        public void DoubleInitialization()
        {
            AppSettings settings = new AppSettings();
            settings.Initialize( s => s == "Test" );
            Assert.Throws<CKException>( () => settings.Initialize( s => s == "Test" ) );
        }

        [Test]
        public void OverrideAndRevert()
        {
            AppSettings settings = new AppSettings();
            settings.Initialize( s => s == "Test" ? "OK" : null );
            Assert.That( settings["Test"], Is.EqualTo( "OK" ) );
            Assert.That( settings["Other"], Is.Null );

            settings.Override( ( previous, key ) => previous( key ) + "-Suffix" );
            Assert.That( settings["Test"], Is.EqualTo( "OK-Suffix" ) );
            Assert.That( settings["Other"], Is.EqualTo( "-Suffix" ) );

            settings.Override( ( previous, key ) => "Prefix-" + previous( key ) );
            Assert.That( settings["Test"], Is.EqualTo( "Prefix-OK-Suffix" ) );
            Assert.That( settings["Other"], Is.EqualTo( "Prefix--Suffix" ) );

            settings.RevertOverrides();
            Assert.That( settings["Test"], Is.EqualTo( "OK" ) );
            Assert.That( settings["Other"], Is.Null );
        }

        [Test]
        public void DefaultInitializationOnConfigurationMananger()
        {
            AppSettings settings = new AppSettings();
            // Here, ConfigurationManager has been late bound.
            Assert.That( settings[ "None" ], Is.Null );
        }

        [Test]
        public void DefaultInitializationnOnConfigurationManangerIsDynamic()
        {
            AppSettings settings = new AppSettings();
            // Here, ConfigurationManager has been late bound.
            Assert.That( settings[ "Test" ], Is.Null );
            // Checks that this auto-configuration is "dynamic".
            SetConfigurationManagerSettings( "Test", "Murfn" );
            Assert.That( settings["Test"], Is.EqualTo( "Murfn" ) );
            RemoveConfigurationManagerSettings( "Test" );
            Assert.That( settings["Test"], Is.Null );
        }

        void SetConfigurationManagerSettings( string key, string text )
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            KeyValueConfigurationElement e = config.AppSettings.Settings[key];
            if( e != null ) e.Value = text;
            else config.AppSettings.Settings.Add( key, text );
            config.Save( ConfigurationSaveMode.Modified );
            ConfigurationManager.RefreshSection( "appSettings" );
        }
        
        void RemoveConfigurationManagerSettings( string key )
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            config.AppSettings.Settings.Remove( key );
            config.Save( ConfigurationSaveMode.Modified );
            ConfigurationManager.RefreshSection( "appSettings" );
        }
    }
}

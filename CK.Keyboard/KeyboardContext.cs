using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Storage;
using CK.Plugin.Config;
using System.Xml;
using CK.Core;
using System.Diagnostics;
using CK.Plugin;

namespace CK.Keyboard
{
    [Plugin( KeyboardContext.PluginIdString, Version = KeyboardContext.PluginIdVersion, PublicName = PluginPublicName,
        Categories = new string[] { "Accessibility", "Test" },
        IconUri = "Resources/test.png",
        RefUrl = "http://www.testUrl.com" )]
    public partial class KeyboardContext : IKeyboardContext, IPlugin, IStructuredSerializable, IStructuredSerializer<KeyboardCollection>
    {
        const string PluginIdString = "{2ED1562F-2416-45cb-9FC8-EEF941E3EDBC}";
        const string PluginIdVersion = "2.5.2";
        const string PluginPublicName = "CK.KeyboardContext";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        bool _isKeyboardContextDirty;
        KeyboardCollection _keyboards;

        public KeyboardContext()
            : this( null )
        {
        }

        public KeyboardContext( IServiceProvider sp )
        {
            _keyboards = new KeyboardCollection( this );

            _empty = new KeyboardMode( this );
            _modes = new Dictionary<string, KeyboardMode>( StringComparer.Ordinal );
            _modes.Add( String.Empty, _empty );
            
            if( sp != null ) sp.GetService<ISimpleServiceContainer>( true ).Add<IStructuredSerializer<KeyboardCollection>>( this );
        }

        public IPluginConfigAccessor Configuration { get; set; }

        [RequiredService]
        public IConfigContainer ConfigContainer { get; set; }

        [RequiredService]
        public ISimplePluginRunner PluginRunner { get; set; }

        public IKeyboardCollection Keyboards
        {
            get { return _keyboards; }
        }

        IKeyboard IKeyboardContext.CurrentKeyboard
        {
            get { return _keyboards.Current; }
            set { _keyboards.Current = (Keyboard)value; }
        }

        internal Keyboard CurrentKeyboard
        {
            get { return _keyboards.Current; }
            set { _keyboards.Current = value; }
        }

        public event EventHandler<CurrentKeyboardChangedEventArgs> CurrentKeyboardChanged
        {
            add { _keyboards.CurrentChanged += value; }
            remove { _keyboards.CurrentChanged -= value; }
        }

        public bool IsDirty
        {
            get { return _isKeyboardContextDirty; }
        }

        public void SetKeyboardContextDirty()
        {
            _isKeyboardContextDirty = true;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            Configuration.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );

            var ctx = Configuration.Context.GetOrSet<KeyboardCollection>( "KeyboardCollection", new KeyboardCollection( this ) );
            
            return true;
        }

        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
        }

        public void Start()
        {
            _keyboards.OnStart();
        }

        public void Teardown()
        {

        }

        public void Stop()
        {
            _keyboards.OnStop();
        }

        object IStructuredSerializer<KeyboardCollection>.ReadInlineContent( IStructuredReader sr, KeyboardCollection o )
        {
            _keyboards.Clear(); 
            _keyboards.ReadInlineContent( sr );
            return _keyboards;
        }

        void IStructuredSerializer<KeyboardCollection>.WriteInlineContent( IStructuredWriter sw, KeyboardCollection o )
        {
            _keyboards.WriteInlineContent( sw );
        }

        public void ReadInlineContent( IStructuredReader sr )
        {
            sr.ServiceContainer.Add<IStructuredSerializer<KeyboardCollection>>( this );

            sr.Xml.Read();
            sr.ReadInlineObjectStructuredElement( "Keyboards", _keyboards );
        }

        public void WriteInlineContent( IStructuredWriter sw )
        {
            sw.ServiceContainer.Add<IStructuredSerializer<KeyboardCollection>>( this );

            sw.WriteInlineObjectStructuredElement( "Keyboards", _keyboards );
        }
    }
}

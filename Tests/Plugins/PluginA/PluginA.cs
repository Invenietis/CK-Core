using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;
using CK.Context;
using System.Diagnostics;
using CK.Keyboard.Model;

namespace PluginA
{
    /// <summary>
    /// Used to check that Injection of IContext works fine
    /// </summary>
    [Plugin( PluginA.PluginIdString, Version = PluginA.PluginIdVersion, PublicName = PluginPublicName,
        Categories = new string[] { "Advanced", "Test" },
        IconUri = "Resources/test.png",
        RefUrl = "http://www.testUrl.com" )]
    public class PluginA : IPlugin
    {
        const string PluginIdString = "{87AA1820-6576-4090-AC63-2A165A485AB0}";
        const string PluginIdVersion = "1.1.0";
        const string PluginPublicName = "PluginA";
        public static readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginIdString, PluginIdVersion, PluginPublicName );

        [RequiredService( Required = true )]
        public IContext Context { get; set; }

        [DynamicService(Requires=RunningRequirement.Optional)]
        public IKeyboardContext KeyboardContext { get; set; }

        public void Start()
        {
            Debug.Assert( Context != null );
        }

        public void Stop()
        {
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
        }
    }
}

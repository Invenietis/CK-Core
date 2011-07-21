using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Core;
using System.Collections;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{
    public class RunningConfiguration
    {
        PluginRunner _runner;
        ISolvedPluginConfiguration _pluginConfig;
        RunnerRequirementsSnapshot _requirements;

        internal RunningConfiguration( PluginRunner r )
        {
            _runner = r;
        }

        internal void Initialize()
        {
            _pluginConfig = new SolvedPluginConfigurationSnapshot( _runner.ConfigManager.SolvedPluginConfiguration );
            _requirements = new RunnerRequirementsSnapshot( _runner.RunnerRequirements );
        }

        public event EventHandler IsDirtyChanged
        {
            add { _runner.IsDirtyChanged += value; }
            remove { _runner.IsDirtyChanged -= value; }
        }

        public ISolvedPluginConfiguration PluginConfiguration
        {
            get { return _pluginConfig; }
        }

        public RunnerRequirementsSnapshot Requirements
        {
            get { return _requirements; }
        }

        public bool IsDirty { get { return _runner.IsDirty; } }

        internal void Apply( SolvedPluginConfigurationSnapshot snapshotConfig, RunnerRequirementsSnapshot snapShotRunner )
        {
            _pluginConfig = snapshotConfig;
            _requirements = snapShotRunner;
        }

     }
}

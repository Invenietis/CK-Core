#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\RunningConfiguration.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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

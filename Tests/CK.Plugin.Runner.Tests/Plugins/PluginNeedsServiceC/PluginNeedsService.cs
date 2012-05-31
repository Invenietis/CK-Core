#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\PluginNeedsServiceC\PluginNeedsService.cs) is part of CiviKey. 
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
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    //Used by RefPluginStatusSwitching
    //Used by RefPluginLiveAction
    /// <summary>
    /// Plugin that require (MustExistAndRun) the IServiceB interface directly as a DynamicService.
    /// </summary>
    [Plugin( "{4E69383E-044D-4786-9077-5F8E5B259793}",
        PublicName = "PluginNeedsService_MEAR", Version = "1.1.0" )]
    public class PluginNeedsService_MEAR : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IServiceC Service { get; set; }

        public bool Started;

        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        #endregion
    }

    /// <summary>
    /// Plugin that require (MustExistTryStart) the IServiceB interface directly as a DynamicService.
    /// </summary>
    [Plugin( "{58C00B79-D882-4C11-BD90-1F25AD664C67}",
        PublicName = "PluginNeedsService_METS", Version = "1.1.0" )]
    public class PluginNeedsService_METS : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IServiceC Service { get; set; }

        public bool Started;

        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        #endregion
    }

    /// <summary>
    /// Plugin that require (MustExist) the IServiceB interface directly as a DynamicService.
    /// </summary>
    [Plugin( "{317B5D34-BA84-4A15-92F4-4E791E737EF0}",
        PublicName = "PluginNeedsService_ME", Version = "1.1.0" )]
    public class PluginNeedsService_ME : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IServiceC Service { get; set; }

        public bool Started;

        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        #endregion
    }

    /// <summary>
    /// Plugin that require (OptionalTryStart) the IServiceB interface directly as a DynamicService.
    /// </summary>
    [Plugin( "{ABD53A18-4549-49B8-82C0-9977200F47E9}",
        PublicName = "PluginNeedsService_OTS", Version = "1.1.0" )]
    public class PluginNeedsService_OTS : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IServiceC Service { get; set; }

        public bool Started;

        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        #endregion
    }

    /// <summary>
    /// Plugin that require (Optional) the IServiceB interface directly as a DynamicService.
    /// </summary>
    [Plugin( "{C78FCB4F-6925-4587-AC98-DA0AE1A977D1}",
        PublicName = "PluginNeedsService_O", Version = "1.1.0" )]
    public class PluginNeedsService_O : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.Optional )]
        public IServiceC Service { get; set; }

        public bool Started;

        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        #region IPlugin Membres

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Teardown()
        {
            // Nothing to do.
        }

        #endregion
    }
}

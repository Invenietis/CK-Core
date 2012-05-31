#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\Plugins\PluginNeedsServiceC\PluginNeedsIService.cs) is part of CiviKey. 
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

    /// <summary>
    /// Plugin that require (MustExistAndRun) the IServiceB interface as a ICKService{T}.
    /// </summary>
    [Plugin( "{457E357D-102D-447D-89B8-DA9C849910C8}",
        PublicName = "PluginNeedsIService_MEAR", Version = "1.1.0" )]
    public class PluginNeedsIService_MEAR : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IServiceC> Service { get; set; }

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
    /// Plugin that require (MustExistAndRun) the IServiceB interface as a ICKService{T}.
    /// </summary>
    [Plugin( "{9BBCFE92-7465-4B3B-88D0-3CEF1E2E5580}",
        PublicName = "PluginNeedsIService_METS", Version = "1.1.0" )]
    public class PluginNeedsIService_METS : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistTryStart )]
        public IService<IServiceC> Service { get; set; }

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
    /// Plugin that require (MustExist) the IServiceB interface as a ICKService{T}.
    /// </summary>
    [Plugin( "{973B4050-280F-43B0-A9E3-0C4DC9BC2C5F}",
        PublicName = "PluginNeedsIService_ME", Version = "1.1.0" )]
    public class PluginNeedsIService_ME : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IService<IServiceC> Service { get; set; }

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
    /// Plugin that require (OptionalTryStart) the IServiceB interface as a ICKService{T}.
    /// </summary>
    [Plugin( "{CDCE6413-038D-4020-A3E0-51FA755C5E72}",
        PublicName = "PluginNeedsIService_OTS", Version = "1.1.0" )]
    public class PluginNeedsIService_OTS : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.OptionalTryStart )]
        public IService<IServiceC> Service { get; set; }

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
    /// Plugin that require (Optional) the IServiceB interface as a ICKService{T}.
    /// </summary>
    [Plugin( "{FF896081-A15D-4A5C-8030-13544EF09673}",
        PublicName = "PluginNeedsIService_O", Version = "1.1.0" )]
    public class PluginNeedsIService_O : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.Optional )]
        public IService<IServiceC> Service { get; set; }

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

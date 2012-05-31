#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\Plugins\RefInternalNonDynamicService\RefInternalNonDynamicService.cs) is part of CiviKey. 
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

using CK.Plugin;

namespace RefInternalNonDynamicService
{
    public interface INotDynamicService
    {
        void ThisServiceDoesNotExtendIDynamicInterface();
    }

    [Plugin( PluginIdString, Version = PluginIdVersion, PublicName = PluginPublicName )]
    public class PluginSuccess : IPlugin
    {
        const string PluginIdString = "{3C49D3E4-1DD7-4017-B5A2-7ABBCE6C135B}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "PluginSuccess";

        /// <summary>
        /// This is perfectly valid. It is at runtime that this plugin will fail to start
        /// if there is no such service available in the service container.
        /// </summary>
        [RequiredService]
        public INotDynamicService ValidRef { get; set; }

        public void Start()
        {
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

    [Plugin( PluginIdString, Version = PluginIdVersion, PublicName = PluginPublicName )]
    public class PluginSuccessAlso : IPlugin
    {
        const string PluginIdString = "{80AB63AC-8422-499C-A017-F73B0CA47288}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "PluginSuccessAlso";

        /// <summary>
        /// We allow the use of DynamicService to reference a non-dynamic interface
        /// as long as the reference is optional.
        /// </summary>
        [DynamicService( Requires = RunningRequirement.Optional )]
        public INotDynamicService NotBuggyRefBecauseOtional { get; set; }

        public void Start()
        {
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

    [Plugin( PluginIdString, Version = PluginIdVersion, PublicName = PluginPublicName )]
    public class PluginFailed : IPlugin
    {
        const string PluginIdString = "{EC92F3A1-5CD3-423F-AE8D-EA519DEB1D3D}";
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "PluginFailed";

        /// <summary>
        /// Discover of the plugin fails since the reference must exist.
        /// </summary>
        [DynamicService( Requires = RunningRequirement.MustExist )]
        public INotDynamicService BuggyRefBecauseMustExist { get; set; }

        public void Start()
        {
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

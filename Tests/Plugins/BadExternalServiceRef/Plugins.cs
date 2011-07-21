using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Diagnostics;

namespace BadExternalServiceRef
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

        [DynamicService( Requires = RunningRequirement.MustExist )]
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

}

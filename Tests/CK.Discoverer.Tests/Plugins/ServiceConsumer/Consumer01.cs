using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    //Used by RefPluginStatusSwitching


    [Plugin( "{2294F5BD-C511-456F-8E6B-A39A84FBAE51}" )]
    public class Consumer01 : IPlugin
    {
        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Teardown()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion

        public IServiceProducer Producer { get; set; }
        
        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IServiceProducer02 Producer02 { get; set; }
    }
}

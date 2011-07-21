using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    //Used by RefPluginStatusSwitching

    [Plugin( "{5D6E000A-BEFB-4C57-AA47-AB3AF9973D77}" )]
    public class Consumer02 : IPlugin
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

        public IService<IServiceProducer> Producer { get; set; }
        
        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IService<IServiceProducer02> Producer02 { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    [Plugin( "{372E3E79-8F3B-494C-8C21-E5653931EDBD}", 
                Categories = new string[] { "Test" }, 
                PublicName="VPWS-old", 
                Version="1.0.0" )]
    public class Plugin05 : IPlugin, IServiceF
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

        public bool CanStart( out string lastError )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IServiceF Members

        public int Return1()
        {
            return 1;
        }

        #endregion
    }
}

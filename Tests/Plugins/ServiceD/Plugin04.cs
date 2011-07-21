using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    [Plugin( "{E3359BBE-7B52-41CC-B8F4-CF594CA22E8B}", 
        Categories = new string[] { "Test" }, 
        PublicName = "Plugin04", Version = "2.5.0" )]
    public class Plugin04 : IPlugin, IServiceD, IServiceE
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

        #region IServiceD Members

        public int Return0()
        {
            return 0;
        }

        #endregion

        #region IServiceE Members

        public int Return1()
        {
            return 1;
        }

        #endregion
    }
}

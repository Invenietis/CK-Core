using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    [Plugin( "{F8459BEF-9918-4188-BC96-B85762DDDA63}" )]
    public class ServiceProducer : IPlugin, IServiceProducer
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

        public event EventHandler<ProducedEventArgs>  Produced;

        public void ProduceNow()
        {
            Produced( this, new ProducedEventArgs() );
        }

    }
}

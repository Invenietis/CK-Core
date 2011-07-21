using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using NUnit.Framework;

namespace Injection.CircleRef
{
    public interface Service02 : IDynamicService
    {
        bool IsRunning { get; }
    }

    [Plugin( "{CD20AB53-6D77-41B8-BC8A-5D95519B1094}" )]
    public class Plugin02 : IPlugin, Service02
    {
        bool _running;

        [DynamicService( Requires=RunningRequirement.MustExistAndRun )]
        public IService<Service03> ServiceWrapped { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            return _running = true;
        }

        public void Start()
        {
            Assert.That( ServiceWrapped != null );
            Assert.That( ServiceWrapped.Service.IsRunning );
        }

        public void Teardown()
        {
            
        }

        public void Stop()
        {
            
        }

        #region Service01 Members

        public bool IsRunning
        {
            get { return _running; }
        }

        #endregion
    }
}

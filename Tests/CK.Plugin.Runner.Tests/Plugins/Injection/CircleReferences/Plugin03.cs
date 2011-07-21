using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using NUnit.Framework;

namespace Injection.CircleRef
{
    public interface Service03 : IDynamicService
    {
        bool IsRunning { get; }
    }

    [Plugin( "{0AF439FE-1562-4BE4-8AAC-D009D1E75BD0}" )]
    public class Plugin03 : IPlugin, Service03
    {
        bool _running;

        [DynamicService( Requires=RunningRequirement.MustExistAndRun )]
        public IService<Service01> ServiceWrapped { get; set; }

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

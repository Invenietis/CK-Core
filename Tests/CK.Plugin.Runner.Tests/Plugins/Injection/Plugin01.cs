using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using NUnit.Framework;

namespace Injection
{
    [Plugin( "{7E0A35E0-0A49-461A-BDC7-7C0083CC5DC9}" )]
    public class Plugin01 : IPlugin
    {
        bool _serviceHasBeenStopped;
        public IPluginConfigAccessor Configuration { get; set; }

        [DynamicService( Requires=RunningRequirement.MustExistTryStart )]
        public IService<IService02> ServiceWrapped { get; set; }

        public bool Setup( IPluginSetupInfo info )
        {
            Assert.That( Configuration != null );
            Assert.That( ServiceWrapped == null );
            return true;
        }

        public void Start()
        {
            Assert.That( ServiceWrapped != null );
            Assert.That( ServiceWrapped.Status == RunningStatus.Started );

            IObjectPluginConfig config = Configuration[ServiceWrapped.Service.SomeObject];
            Assert.That( (string)config["testKey"] == "testValue" );

            string key = "newKey";
            object value = "newValue";

            ServiceWrapped.Service.UpdateEditedConfig( key, value );

            Assert.That( config[key] == value );

            ServiceWrapped.ServiceStatusChanged += ( o, e ) =>
            {
                _serviceHasBeenStopped = true;
            };
        }

        public void Teardown()
        {
            Assert.That( _serviceHasBeenStopped );
        }

        public void Stop()
        {
            
        }
    }
}

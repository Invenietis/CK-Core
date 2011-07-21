using CK.Plugin;
using NUnit.Framework;

namespace Injection.CircleRef
{
    public interface Service01 : IDynamicService
    {
        bool IsRunning { get; }
    }

    [Plugin( "{6DCB0BB5-5843-4F48-9FBD-5A0FAD2C8157}" )]
    public class Plugin01 : IPlugin, Service01
    {
        bool _running;

        [DynamicService( Requires=RunningRequirement.MustExistAndRun )]
        public IService<Service02> ServiceWrapped { get; set; }

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

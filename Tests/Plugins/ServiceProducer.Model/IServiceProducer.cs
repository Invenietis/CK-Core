using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    public class ProducedEventArgs : EventArgs
    {
        public readonly DateTime Time;

        public ProducedEventArgs()
        {
            Time = DateTime.UtcNow;
        }
    }

    public interface IServiceProducer : IDynamicService
    {
        event EventHandler<ProducedEventArgs> Produced;

        void ProduceNow();
    }

}

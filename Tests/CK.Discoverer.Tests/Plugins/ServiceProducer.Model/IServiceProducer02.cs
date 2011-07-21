using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;

namespace CK.Tests.Plugin
{
    public interface IServiceProducer02 : IDynamicService
    {
        event EventHandler<ProducedEventArgs> Produced;

        void ProduceNow();
    }
}

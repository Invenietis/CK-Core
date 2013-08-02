using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring
{
    public interface IGrandOutputSink
    {
        void Handle( GrandOutputEventInfo logEvent );
    }

}

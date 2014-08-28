using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Monitoring.Server
{
    public class CriticalErrorEventArgs : EventArgs
    {
        public readonly string Error;

        public CriticalErrorEventArgs( string error )
        {
            Error = error;
        }
    }

}

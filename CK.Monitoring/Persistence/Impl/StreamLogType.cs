using System;

namespace CK.Monitoring.Impl
{
    [Flags]
    enum StreamLogType
    {
        EndOfStream = 0,

        TypeMask                    = 3,

        TypeLog                     = 1,
        TypeOpenGroup               = 2,
        TypeOpenGroupWithException  = 3,
        TypeGroupClosed             = 4
    }
}

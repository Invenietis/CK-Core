using System;

namespace CK.Monitoring.Impl
{
    [Flags]
    enum StreamLogType
    {
        EndOfStream = 0,

        TypeMask                    = 3,

        TypeLine                    = 1,
        TypeOpenGroup               = 2,
        TypeGroupClosed             = 3,

        HasTags = 4,
        HasException = 8,
        HasFileName = 16,
        IsTextTheExceptionMessage = 32,
        HasConclusions = 64,

        IsMultiCast = 128,

        HasUniquifier = 256
    }
}

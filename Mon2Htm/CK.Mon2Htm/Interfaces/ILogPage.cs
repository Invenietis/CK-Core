using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public interface ILogPage
    {
        IReadOnlyList<ILogEntry> Entries { get; }

        IReadOnlyList<ILogEntry> OpenGroupsAtStart { get; }

        IReadOnlyList<ILogEntry> OpenGroupsAtEnd { get; }

        int PageNumber { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public interface IStructuredLogPage
    {
        IReadOnlyList<IPagedLogEntry> Entries { get; }

        IReadOnlyList<ILogEntry> OpenGroupsAtStart { get; }

        IReadOnlyList<ILogEntry> OpenGroupsAtEnd { get; }

        int PageNumber { get; }
    }
}

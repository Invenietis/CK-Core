using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public interface IPagedLogEntry : ILogEntry
    {
        /// <summary>
        /// Children contained in a group. Can be null (not an OpenGroup) or empty.
        /// </summary>
        IReadOnlyList<IPagedLogEntry> Children { get; }

        /// <summary>
        /// Page at which a group started. 0 when the group started in the same page.
        /// </summary>
        int GroupStartsOnPage { get; }

        /// <summary>
        /// Page at which a group ended. 0 when the group ended in the same page.
        /// </summary>
        int GroupEndsOnPage { get; }
    }
}

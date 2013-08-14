using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Monitoring
{
    public class GrandOutputEventInfo
    {
        readonly GrandOutputSource _source;
        readonly ILogEntry _entry;
        readonly int _depth;

        /// <summary>
        /// Initializes a new <see cref="GrandOutputEventInfo"/>.
        /// </summary>
        /// <param name="source">Source information (monitor and channel name).</param>
        /// <param name="e">Log entry.</param>
        /// <param name="depth">See <see cref="SourceRelativeDepth"/>.</param>
        public GrandOutputEventInfo( GrandOutputSource source, ILogEntry e, int depth )
        {
            _source = source;
            _entry = e;
            _depth = depth;
        }

        /// <summary>
        /// Gets the source of the event: the source is defined by a monitor identifier and the channel name.
        /// </summary>
        public GrandOutputSource Source { get { return _source; } }

        /// Gets a unified, immutable, view of the log event as a <see cref="ILogEntry"/>.
        /// </summary>
        public ILogEntry LogEntry { get { return _entry; } }

        /// <summary>
        /// Gets the depth of this entry relatively to the source: a negative depth indicates an entry that is emmitted in a group "above", a group that has not
        /// been previously seen by this <see cref="Source"/>.
        /// </summary>
        public int SourceRelativeDepth { get { return _depth; } }

        /// <summary>
        /// Gets the depth of this entry from the root of the activities.
        /// </summary>
        public int EntryDepth { get { return _depth + _source.InitialDepth; } }

    }
}

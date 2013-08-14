using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CK.RouteConfig;

namespace CK.Monitoring.Impl
{
    internal class ConfiguredSinkParallel : ConfiguredSink
    {
        readonly ConfiguredSink[] _children;

        public ConfiguredSinkParallel( ActionParallelConfiguration c, ConfiguredSink[] children )
            : base( c )
        {
            _children = children;
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/> by calling each
        /// child's handle in parallel.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        public override void Handle( GrandOutputEventInfo logEvent )
        {
            IEnumerable<Task> tasks = _children.Select( c => new Task( () => c.Handle( logEvent ) ) );
            Parallel.ForEach( tasks, t => t.RunSynchronously() );
        }

    }
}

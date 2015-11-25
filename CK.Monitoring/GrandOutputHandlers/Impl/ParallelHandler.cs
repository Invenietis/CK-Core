using System.Threading.Tasks;
using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    internal class ParallelHandler : HandlerBase
    {
        readonly HandlerBase[] _children;

        public ParallelHandler( ActionParallelConfiguration c, HandlerBase[] children )
            : base( c )
        {
            _children = children;
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/> by calling each
        /// child's handler in parallel.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        /// <param name="parrallelCall">True if this is called in parallel.</param>
        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            Parallel.For( 0,  _children.Length, i => _children[i].Handle( logEvent, true ) );
        }

    }
}

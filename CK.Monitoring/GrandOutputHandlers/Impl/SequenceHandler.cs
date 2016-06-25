using CK.RouteConfig;

namespace CK.Monitoring.GrandOutputHandlers
{
    internal class SequenceHandler : HandlerBase
    {
        readonly HandlerBase[] _children;

        public SequenceHandler( ActionSequenceConfiguration c, HandlerBase[] children )
            : base( c )
        {
            _children = children;
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/> by calling each
        /// child's handle in sequence.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        /// <param name="parrallelCall">True if this is called in parallel.</param>
        public override void Handle( GrandOutputEventInfo logEvent, bool parrallelCall )
        {
            foreach( var c in _children ) c.Handle( logEvent, parrallelCall );
        }

    }
}

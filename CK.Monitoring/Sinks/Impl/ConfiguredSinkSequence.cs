using CK.RouteConfig;

namespace CK.Monitoring.Impl
{
    internal class ConfiguredSinkSequence : ConfiguredSink
    {
        readonly ConfiguredSink[] _children;

        public ConfiguredSinkSequence( ActionSequenceConfiguration c, ConfiguredSink[] children )
            : base( c )
        {
            _children = children;
        }

        /// <summary>
        /// Handles a <see cref="GrandOutputEventInfo"/> by calling each
        /// child's handle in sequence.
        /// </summary>
        /// <param name="logEvent">Event to handle.</param>
        public override void Handle( GrandOutputEventInfo logEvent )
        {
            foreach( var c in _children ) c.Handle( logEvent );
        }

    }
}

using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Captures centralized information among the different <see cref="HandlerBase">Handlers</see> of a channel.
    /// </summary>
    public sealed class ChannelOption
    {
        LogFilter _currentFilter;

        internal ChannelOption( LogFilter mainRouteFilter )
        {
            _currentFilter = mainRouteFilter;
        }

        /// <summary>
        /// Enables any handler to publish the minimal filter level it requires (if any).
        /// </summary>
        /// <param name="filter">Filter required by a <see cref="HandlerBase"/>.</param>
        public void SetMinimalFilter( LogFilter filter )
        {
            _currentFilter = _currentFilter.Combine( filter );
        }

        /// <summary>
        /// Gets the minimal <see cref="LogFilter"/>.
        /// Since a handler can publish its minimal filter requirement, we can optimize the filtering levels on 
        /// monitors bound to a channel.
        /// </summary>
        public LogFilter CurrentMinimalFilter { get { return _currentFilter; } }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Captures centralized information among the different <see cref="HandlerBase">Handlers</see> of a channel.
    /// </summary>
    public sealed class ChannelOption
    {
        LogLevelFilter _currentFilter;

        /// <summary>
        /// Enables a <see cref="GrandOutpuHandler"/> to publish the filter level it requires (if any).
        /// </summary>
        /// <param name="filter">Filter required by a <see cref="GrandOutpuHandler"/>.</param>
        public void SetMinimalFilter( LogLevelFilter filter )
        {
            if( filter != LogLevelFilter.None ) 
            {
                if( _currentFilter == LogLevelFilter.None || filter < _currentFilter ) _currentFilter = filter;
            }
        }

        /// <summary>
        /// Gets the minimal <see cref="LogLevelFilter"/>.
        /// Since a <see cref="GrandOutpuHandler"/> can publish its level, we can optimize the filtering level on 
        /// monitors bound to a channel.
        /// </summary>
        public LogLevelFilter CurrentMinimalFilter { get { return _currentFilter; } }

    }
}

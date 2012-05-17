using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;

namespace CK.Core
{
    /// <summary>
    /// Abstract base class for <see cref="IMuxActivityLoggerClient"/> that routes
    /// multiplexed log data back to simple <see cref="IActivityLoggerClient"/> specific 
    /// to each <see cref="IActivityLogger"/> sender.
    /// </summary>
    public abstract class MuxActivityLoggerClientDemux : IMuxActivityLoggerClient
    {
        IActivityLogger _lastSender;
        IActivityLoggerClient _lastClient;
        ListDictionary _clients;

        IActivityLoggerClient FindOrCreate( IActivityLogger sender )
        {
            if( _lastSender == sender ) return _lastClient;
            if( _lastSender == null )
            {
                _lastClient = CreateClient( sender );
            }
            else
            {
                if( _clients == null )
                {
                    _clients = new ListDictionary();
                    _clients.Add( _lastSender, _lastClient );
                    _lastClient = null;
                }
                else _lastClient = (IActivityLoggerClient)_clients[sender];
                if( _lastClient == null )
                {
                    _lastClient = CreateClient( sender );
                    _clients.Add( sender, _lastClient );
                }
            }
            _lastSender = sender;
            return _lastClient;
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="current">Current level filter.</param>
        /// <param name="newValue">The new level filter.</param>
        void IMuxActivityLoggerClient.OnFilterChanged( IActivityLogger sender, LogLevelFilter current, LogLevelFilter newValue )
        {
            FindOrCreate( sender ).OnFilterChanged( current, newValue );
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having 
        /// called <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="level">Log level.</param>
        /// <param name="text">Text to log (never null).</param>
        void IMuxActivityLoggerClient.OnUnfilteredLog( IActivityLogger sender, LogLevel level, string text )
        {
            FindOrCreate( sender ).OnUnfilteredLog( level, text );
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The newly opened <see cref="IActivityLogGroup"/>.</param>
        void IMuxActivityLoggerClient.OnOpenGroup( IActivityLogger sender, IActivityLogGroup group )
        {
            FindOrCreate( sender ).OnOpenGroup( group );
        }

        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The group that will be closed.</param>
        /// <param name="conclusion">The conclusion to associate to the closing group.</param>
        /// <returns>The new conclusion that should be associated to the group. Returning null has no effect on the current conclusion.</returns>
        string IMuxActivityLoggerClient.OnGroupClosing( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            return FindOrCreate( sender ).OnGroupClosing( group, conclusion );
        }


        /// <summary>
        /// Routes the call to the associated <see cref="IActivityLoggerClient"/> (after having called 
        /// <see cref="CreateClient"/> if necessary).
        /// </summary>
        /// <param name="sender">The sender logger.</param>
        /// <param name="group">The group that will be closed.</param>
        /// <param name="conclusion">The conclusion associated to the closed group.</param>
        void IMuxActivityLoggerClient.OnGroupClosed( IActivityLogger sender, IActivityLogGroup group, string conclusion )
        {
            FindOrCreate( sender ).OnGroupClosed( group, conclusion );
        }

        /// <summary>
        /// Must be overriden to create a new <see cref="IActivityLoggerClient"/> for the given <see cref="IActivityLogger"/>.
        /// </summary>
        /// <param name="logger">The new sender for which a <see cref="IActivityLoggerClient"/> must be created.</param>
        /// <returns>A new concrete <see cref="IActivityLoggerClient"/> bound to the given logger.</returns>
        protected abstract IActivityLoggerClient CreateClient( IActivityLogger logger );
    }
}

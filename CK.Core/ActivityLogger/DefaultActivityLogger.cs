using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Specialized <see cref="ActivityLogger"/> that contains a non removable <see cref="ActivityLoggerTap"/>.
    /// </summary>
    public class DefaultActivityLogger : ActivityLogger
    {
        ActivityLoggerTap _tap;

        class CheckedOutput : ActivityLoggerOutput
        {
            public CheckedOutput( DefaultActivityLogger logger )
                : base( logger )
            {
            }

            new DefaultActivityLogger Logger { get { return (DefaultActivityLogger)base.Logger; } }

            protected override void OnAfterRemoved( IActivityLoggerClient client )
            {
                if( client == Logger._tap && !RegisteredMuxClients.Contains( client ) ) throw new InvalidOperationException();
                base.OnAfterRemoved( client );
            }

            protected override void OnAfterRemoved( IMuxActivityLoggerClient client )
            {
                if( client == Logger._tap && !RegisteredClients.Contains( client ) ) throw new InvalidOperationException();
                base.OnAfterRemoved( client );
            }

        }

        /// <summary>
        /// Initialize a new <see cref="DefaultActivityLogger"/>.
        /// </summary>
        public DefaultActivityLogger()
            : base( null )
        {
            SetOutput( new CheckedOutput( this ) );
            _tap = new ActivityLoggerTap();
            Output.RegisterMuxClient( _tap );
        }

        /// <summary>
        /// Gets the <see cref="ActivityLoggerTap"/> that manages <see cref="IActivityLoggerSink"/>
        /// for this <see cref="DefaultActivityLogger"/>.
        /// </summary>
        public ActivityLoggerTap Tap 
        { 
            get { return _tap; } 
        }

        /// <summary>
        /// Adds an <see cref="IActivityLoggerSink"/> to the <see cref="RegisteredSinks"/>.
        /// Duplicate <see cref="IActivityLoggerSink"/> are silently ignored.
        /// </summary>
        /// <param name="l">An activity logger sink implementation.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public DefaultActivityLogger Register( IActivityLoggerSink l )
        {
            _tap.Register( l );
            return this;
        }

        /// <summary>
        /// Unregisters the given <see cref="IActivityLoggerSink"/> from the collection of loggers.
        /// Silently ignored unregistered logger.
        /// </summary>
        /// <param name="l">An activity logger sink implementation.</param>
        /// <returns>This logger to enable fluent syntax.</returns>
        public virtual DefaultActivityLogger Unregister( IActivityLoggerSink l )
        {
            _tap.Unregister( l );
            return this;
        }

        /// <summary>
        /// Gets the list of registered <see cref="IActivityLoggerSink"/>.
        /// </summary>
        public IReadOnlyList<IActivityLoggerSink> RegisteredSinks
        {
            get { return _tap.RegisteredSinks; }
        }

    }
}

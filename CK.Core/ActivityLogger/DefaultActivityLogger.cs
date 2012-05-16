using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Gives access to concrete implementation of <see cref="IDefaultActivityLogger"/> thanks to <see cref="Create"/> factory method.
    /// </summary>
    public class DefaultActivityLogger : ActivityLogger, IDefaultActivityLogger
    {
        /// <summary>
        /// Factory method for <see cref="IDefaultActivityLogger"/> implementation.
        /// </summary>
        /// <returns>A new <see cref="IDefaultActivityLogger"/> implementation.</returns>
        static public IDefaultActivityLogger Create()
        {
            return new DefaultActivityLogger();
        }

        class EmptyDefault : ActivityLogger.EmptyLogger, IDefaultActivityLogger
        {
            public ActivityLoggerTap Tap
            {
                get { return ActivityLoggerTap.Empty; }
            }

            public IDefaultActivityLogger Register( IActivityLoggerSink sink )
            {
                return this;
            }

            public IDefaultActivityLogger Unregister( IActivityLoggerSink sink )
            {
                return this;
            }

            public IReadOnlyList<IActivityLoggerSink> RegisteredSinks
            {
                get { return ReadOnlyListEmpty<IActivityLoggerSink>.Empty; }
            }
        }

        /// <summary>
        /// Empty <see cref="IDefaultActivityLogger"/> (null object design pattern).
        /// </summary>
        static public new readonly IDefaultActivityLogger Empty = new EmptyDefault();

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

        DefaultActivityLogger()
            : base( null )
        {
            SetOutput( new CheckedOutput( this ) );
            _tap = new ActivityLoggerTap();
            Output.RegisterMuxClient( _tap );
        }

        public ActivityLoggerTap Tap 
        { 
            get { return _tap; } 
        }

        public IDefaultActivityLogger Register( IActivityLoggerSink sink )
        {
            _tap.Register( sink );
            return this;
        }

        public IDefaultActivityLogger Unregister( IActivityLoggerSink sink )
        {
            _tap.Unregister( sink );
            return this;
        }

        public IReadOnlyList<IActivityLoggerSink> RegisteredSinks
        {
            get { return _tap.RegisteredSinks; }
        }

    }
}

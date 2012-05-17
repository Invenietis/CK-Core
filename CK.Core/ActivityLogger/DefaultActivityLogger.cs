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

            public ActivityLoggerErrorCounter ErrorCounter
            {
                get { return ActivityLoggerErrorCounter.Empty; }
            }

            public ActivityLoggerPathCatcher PathCatcher
            {
                get { return ActivityLoggerPathCatcher.Empty; }
            }

        }

        /// <summary>
        /// Empty <see cref="IDefaultActivityLogger"/> (null object design pattern).
        /// </summary>
        static public new readonly IDefaultActivityLogger Empty = new EmptyDefault();

        ActivityLoggerTap _tap;
        ActivityLoggerErrorCounter _errorCounter;
        ActivityLoggerPathCatcher _pathCatcher;

        DefaultActivityLogger()
        {
            _tap = new ActivityLoggerTap();
            _errorCounter = new ActivityLoggerErrorCounter();
            _pathCatcher = new ActivityLoggerPathCatcher();
            Output.RegisterMuxClient( _tap );
            Output.RegisterMuxClient( _errorCounter );
            Output.RegisterMuxClient( _pathCatcher );
            Output.NonRemoveableClients.AddRangeArray( _tap, _errorCounter, _pathCatcher );
        }

        ActivityLoggerTap IDefaultActivityLogger.Tap 
        { 
            get { return _tap; } 
        }

        ActivityLoggerErrorCounter IDefaultActivityLogger.ErrorCounter
        {
            get { return _errorCounter; }
        }

        ActivityLoggerPathCatcher IDefaultActivityLogger.PathCatcher
        {
            get { return _pathCatcher; }
        }

        IDefaultActivityLogger IDefaultActivityLogger.Register( IActivityLoggerSink sink )
        {
            _tap.Register( sink );
            return this;
        }

        IDefaultActivityLogger IDefaultActivityLogger.Unregister( IActivityLoggerSink sink )
        {
            _tap.Unregister( sink );
            return this;
        }

        IReadOnlyList<IActivityLoggerSink> IDefaultActivityLogger.RegisteredSinks
        {
            get { return _tap.RegisteredSinks; }
        }

    }
}

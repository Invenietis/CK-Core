using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.Core.Impl;
using CK.Monitoring.Impl;

namespace CK.Monitoring
{
    internal class StandardChannel : IChannel
    {
        readonly IGrandOutputSink _common;
        readonly IReadOnlyList<ConfiguredSink> _sinks;
        readonly CountdownEvent _locker;
        readonly string _configurationName;
        int _inputCount;

        internal StandardChannel( IGrandOutputSink common, CountdownEvent locker, IReadOnlyList<ConfiguredSink> sinks, string configurationName )
        {
            _common = common;
            _locker = locker;
            _sinks = sinks;
            _configurationName = configurationName;
        }

        public GrandOutputSource CreateInput( IActivityMonitorImpl monitor, string channelName )
        {
            Interlocked.Increment( ref _inputCount );
            return new GrandOutputSource( monitor, channelName );
        }

        public void ReleaseInput( GrandOutputSource source )
        {
            Interlocked.Decrement( ref _inputCount );
        }

        public void Handle( GrandOutputEventInfo logEvent )
        {
            _common.Handle( logEvent );
            DoHandle( logEvent );
        }

        public LogLevelFilter MinimalFilter 
        {
            get { return LogLevelFilter.None; } 
        }

        protected virtual void DoHandle( GrandOutputEventInfo logEvent )
        {
            try
            {
                // Here, an exception may be thrown if the countdown is already set to 0.
                // This may occur if and only if a GrandOutputClient has obtained the Channel
                // 
                _locker.AddCount();
                ThreadPool.QueueUserWorkItem( o =>
                    {
                        try
                        {
                            foreach( var s in _sinks ) s.Handle( logEvent );
                        }
                        catch( Exception ex )
                        {
                            ActivityMonitor.LoggingError.Add( ex, "While logging event." );
                        }
                        finally
                        {
                            _locker.Signal();
                        }
                    }
                );
            }
            catch( Exception ex )
            {
                ActivityMonitor.LoggingError.Add( ex, "While handling log event." );
            }
        }
    }
}

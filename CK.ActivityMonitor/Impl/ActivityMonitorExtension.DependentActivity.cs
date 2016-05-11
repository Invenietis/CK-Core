using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityMonitor"/> and other types from the Activity monitor framework.
    /// </summary>
    public static partial class ActivityMonitorExtension
    {

        /// <summary>
        /// Offers dependent token creation and launching.
        /// </summary>
        public struct DependentSender
        {
            readonly IActivityMonitor _monitor;
            readonly string _fileName;
            readonly int _lineNumber;

            internal DependentSender( IActivityMonitor m, string f, int n )
            {
                _monitor = m;
                _fileName = f;
                _lineNumber = n;
            }

            /// <summary>
            /// Creates a token for a dependent activity that will use the current monitor's topic.
            /// By default, a line with <see cref="ActivityMonitor.Tags.CreateDependentActivity"/> is logged that describes the 
            /// creation of the token.
            /// If <paramref name="delayedLaunch"/> is true, the actual launch of the dependent activity must be signaled thanks to <see cref="Launch(ActivityMonitor.DependentToken)"/>
            /// (otherwise there will be no way to bind the two activities). 
            /// </summary>
            /// <param name="delayedLaunch">True to use <see cref="Launch(ActivityMonitor.DependentToken)"/> later to indicate the actual launch of the dependent activity.</param>
            /// <returns>A dependent token.</returns>
            public ActivityMonitor.DependentToken CreateToken( bool delayedLaunch = false )
            {
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithMonitorTopic( _monitor, delayedLaunch, out msg );
                if( delayedLaunch ) t.DelayedLaunchMessage = msg;
                else _monitor.UnfilteredLog( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, msg, t.CreationDate, null, _fileName, _lineNumber );
                return t;
            }

            /// <summary>
            /// Creates a token for a dependent activity that will be bound to a specified topic (or that will not change the dependent monitor's topic
            /// if null is specified).
            /// </summary>
            /// <param name="dependentTopic">Topic for the dependent activity. Use null to not change the dependent monitor's topic.</param>
            /// <param name="delayedLaunch">True to use <see cref="Launch(ActivityMonitor.DependentToken)"/> later to indicate the actual launch of the dependent activity.</param>
            /// <returns>A dependent token.</returns>
            public ActivityMonitor.DependentToken CreateTokenWithTopic( string dependentTopic, bool delayedLaunch = false )
            {
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithDependentTopic( _monitor, delayedLaunch, dependentTopic, out msg );
                if( delayedLaunch ) t.DelayedLaunchMessage = msg;
                else _monitor.UnfilteredLog( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, msg, t.CreationDate, null, _fileName, _lineNumber );
                return t;
            }

            /// <summary>
            /// Signals the launch of one or more dependent activities by emitting a log line that describes the token.
            /// The token must have been created by <see cref="CreateToken"/> or <see cref="CreateTokenWithTopic"/> with a true delayedLaunch parameter
            /// otherwise an <see cref="InvalidOperationException"/> is thrown.
            /// </summary>
            /// <param name="token">Dependent token.</param>
            public void Launch( ActivityMonitor.DependentToken token )
            {
                if( token.DelayedLaunchMessage == null ) throw new InvalidOperationException( Impl.ActivityMonitorResources.ActivityMonitorDependentTokenMustBeDelayedLaunch );
                _monitor.UnfilteredLog( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, token.DelayedLaunchMessage, _monitor.NextLogTime(), null, _fileName, _lineNumber );
            }

            /// <summary>
            /// Launches one or more dependent activities (thanks to a delegate) that will use the current monitor's topic.
            /// This creates a new <see cref="ActivityMonitor.DependentToken"/> and opens a group that wraps the execution of the <paramref name="dependentLauncher"/>.
            /// </summary>
            /// <param name="dependentLauncher">Must create and launch dependent activities that should use the created token.</param>
            /// <returns>A dependent token.</returns>
            public void Launch( Action<ActivityMonitor.DependentToken> dependentLauncher )
            {
                if( dependentLauncher == null ) throw new ArgumentNullException( nameof( dependentLauncher ) );
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithMonitorTopic( _monitor, true, out msg );
                using( _monitor.UnfilteredOpenGroup( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, null, msg, t.CreationDate, null, _fileName, _lineNumber ) )
                {
                    dependentLauncher( t );
                    _monitor.CloseGroup( _monitor.NextLogTime(), "Success." );
                }
            }

            /// <summary>
            /// Launches one or more dependent activities (thanks to a delegate) that will be bound to a specified topic (or that will not change 
            /// the dependent monitor's topic if null is specified).
            /// This creates a new <see cref="ActivityMonitor.DependentToken"/> and opens a group that wraps the execution of the <paramref name="dependentLauncher"/>.
            /// </summary>
            /// <param name="dependentLauncher">Must create and launch dependent activities that should use the created token.</param>
            /// <param name="dependentTopic">Topic for the dependent activity. When null, the dependent monitor's topic is not changed.</param>
            public void LaunchWithTopic( Action<ActivityMonitor.DependentToken> dependentLauncher, string dependentTopic )
            {
                if( dependentLauncher == null ) throw new ArgumentNullException( nameof(dependentLauncher) );
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithDependentTopic( _monitor, true, dependentTopic, out msg );
                using( _monitor.UnfilteredOpenGroup( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, null, msg, t.CreationDate, null, _fileName, _lineNumber ) )
                {
                    dependentLauncher( t );
                    _monitor.CloseGroup( _monitor.NextLogTime(), "Success." );
                }
            }
        }
        
        /// <summary>
        /// Enables dependent activities token creation and activities launch.
        /// Use <see cref="StartDependentActivity">IActivityMonitor.StartDependentActivity</see> to declare the start of a 
        /// dependent activity on the target monitor.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/>.</param>
        /// <param name="fileName">Source file name of the emitter (automatically injected by C# compiler but can be explicitly set).</param>
        /// <param name="lineNumber">Line number in the source file (automatically injected by C# compiler but can be explicitly set).</param>
        /// <returns>Sender object.</returns>
        static public DependentSender DependentActivity( this IActivityMonitor @this, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            return new DependentSender( @this, fileName, lineNumber );
        }
        
        /// <summary>
        /// Starts a dependent activity. This sets the <see cref="ActivityMonitor.DependentToken.Topic"/> if it is not null and opens a group
        /// tagged with <see cref="ActivityMonitor.Tags.StartDependentActivity"/> with a message that can be parsed back thanks to <see cref="ActivityMonitor.DependentToken.TryParseStartMessage"/>.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/>.</param>
        /// <param name="token">Token that describes the origin of the activity.</param>
        /// <param name="fileName">Source file name of the emitter (automatically injected by C# compiler but can be explicitly set).</param>
        /// <param name="lineNumber">Line number in the source file (automatically injected by C# compiler but can be explicitly set).</param>
        /// <returns>A disposable object. It must be disposed at the end of the activity.</returns>
        static public IDisposable StartDependentActivity( this IActivityMonitor @this, ActivityMonitor.DependentToken token, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            if( token == null ) throw new ArgumentNullException( "token" );
            return ActivityMonitor.DependentToken.Start( token, @this, fileName, lineNumber );
        }

    }
}

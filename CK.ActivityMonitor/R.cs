using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core
{
    internal class R
    {
        internal static readonly string ActivityMonitorBoundClientMultipleRegister = "A '{0}' can be registered in only one IActivityMonitor.Output at the same time. Unregister it before Registering it in another monitor.";
        internal static readonly string ActivityMonitorConcurrentThreadAccess = "Concurrent accesses from 2 threads to the same ActivityMonitor has been detected. Only one thread at a time can interact with an ActivityMonitor.";
        internal static readonly string ActivityMonitorDependentTokenMustBeDelayedLaunch = "Dependent token must have been created with CreateToken( true ) to enable Lauch( token ) to be called later (or CreateTokenWithTopic).";
        internal static readonly string ActivityMonitorErrorWhileGetConclusionText = "Unexpected error while getting conclusion text: '{0}'.";
        internal static readonly string ActivityMonitorInvalidLogLevel = "The level must be a valid level (Trace, Info, Warn, Error or Fatal).";
        internal static readonly string ActivityMonitorReentrancyCallOnly = "Only reentrant calls to this method are supported.";
        internal static readonly string ActivityMonitorReentrancyError = "A reentrant call in an ActivityMonitor has been detected. A monitor usage must not trigger another operation on the same monitor.";
        internal static readonly string ActivityMonitorReentrancyReleaseError = "Internal error: Error during release reentrancy operation. Thread id={0} entered whereas release is called from thread '{1}', id={2}.";
        internal static readonly string ActivityMonitorTagMustBeRegistered = "The Tag (CKTrait) must be registered in ActivityMonitor.Tags.";
        internal static readonly string ClosedByBridgeRemoved = "Prematurely closed by Bridge removed.";
        internal static readonly string PossibleWrongOverloadUseWithException = "Possible use of the wrong overload: Use the form that takes a first parameter of type Exception and then the string text instead of this ( string format, object arg0, ... ) method to log the exception, or calls this overload explicitely with the Exception.Message string.";
        internal static readonly string InvalidRootLogPath = "Invalid Root log path.";
        internal static readonly string SystemActivityMonitorRootLogPathSetOnlyOnce = "Root log path must be set once and only once.";
        internal static readonly string RootLogPathMustBeSet = "Root log path must be set.";
    }
}

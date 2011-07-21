using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Helper class that helps detecting missing calls to <see cref="IDisposable.Dispose"/>.
    /// </summary>
    /// <example>
    /// Sample code for a simple class (if unmanaged resources are involved or may be involved by the disposable object,
    /// the standard Dispose(bool disposing) pattern must be implemented.
    /// <code>
    /// class DisposableClassDebug : IDisposable
    /// {
    /// #if DEBUG
    ///     MissingDisposeCallSentinel _sentinel = new MissingDisposeCallSentinel();
    ///     ~DisposableClassDebug()
    ///     {
    ///         MissingDisposeCallSentinel.RegisterMissing( _sentinel );
    ///     }
    /// #endif
    ///
    ///     public void Dispose()
    ///     {
    /// #if DEBUG
    ///         _sentinel = null;
    ///         GC.SuppressFinalize( this );
    /// #endif
    ///     }
    /// }
    /// </code> 
    /// </example>
    public class MissingDisposeCallSentinel
    {
        /// <summary>
        /// Creation time of the <see cref="IDisposable"/> object.
        /// </summary>
        public readonly DateTime Time;

        /// <summary>
        /// Thread identifier that created the <see cref="IDisposable"/> object.
        /// </summary>
        public readonly int ThreadId;
        
        /// <summary>
        /// Stack trace of the <see cref="IDisposable"/> object creation.
        /// </summary>
        public readonly StackTrace StackTrace;

        MissingDisposeCallSentinel _nextFail;
        static object _lock = new Object();
        static MissingDisposeCallSentinel _lastFail;
        static int _count;

        /// <summary>
        /// Initializes a new <see cref="MissingDisposeCallSentinel"/>. 
        /// Should be called during a field initializer in the <see cref="IDisposable"/> object.
        /// </summary>
        [MethodImplAttribute( MethodImplOptions.NoInlining )]
        public MissingDisposeCallSentinel()
        { 
            Time = DateTime.UtcNow;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            StackTrace = new StackTrace( 1, true );
        }

        /// <summary>
        /// Overriden to return <see cref="Time"/>, <see cref="ThreadId"/> and <see cref="StackTrace"/>.
        /// </summary>
        /// <returns>Creation time information.</returns>
        public override string ToString()
        {
            return
                "Time: " + Time + Environment.NewLine +
                "Thread: " + ThreadId + Environment.NewLine +
                "Stack: " + StackTrace;
        }

        /// <summary>
        /// Clears all registered <see cref="MissingDisposeCallSentinel"/>
        /// This method is thread safe.
        /// </summary>
        public static void Clear()
        {
            lock( _lock ) 
            { 
                _lastFail = null; 
                _count = 0; 
            }
        }

        /// <summary>
        /// Gets all the <see cref="MissingDisposeCallSentinel"/> that been registered via <see cref="RegisterMissing"/>.
        /// This method is thread safe.
        /// </summary>
        public static IEnumerable<MissingDisposeCallSentinel> Missing
        {
            get
            {
                MissingDisposeCallSentinel last;
                lock( _lock ) { last = _lastFail; }
                while( last != null )
                {
                    yield return last;
                    last = last._nextFail;
                }
            }
        }

        /// <summary>
        /// Gets a string with missing dispose information. Null if <see cref="HasMissingDisposeCall"/> is false.
        /// This method is thread safe.
        /// </summary>
        /// <returns>Number of missing dispose and related information to each of them.</returns>
        public static string DumpMissing()
        {
            lock( _lock )
            {
                if( _count == 0 ) return null;
                StringBuilder b = new StringBuilder();
                b.AppendFormat( "Missing {0} Dispose:", _count );
                MissingDisposeCallSentinel m = _lastFail; 
                while( m != null )
                {
                    b.AppendLine();
                    b.Append( m.ToString() );
                    m = m._nextFail;
                }
                return b.ToString();
            }
        }

        /// <summary>
        /// True if any missing call to dispose have been detected.
        /// This method is thread safe.
        /// </summary>
        public static bool HasMissingDisposeCall
        {
            get { return _lastFail != null; }
        }

        /// <summary>
        /// Registers a sentinel. Should be called from the <see cref="IDisposable"/> object finalizer.
        /// This method is thread safe.
        /// </summary>
        /// <param name="s">A <see cref="MissingDisposeCallSentinel"/>. Can be null (to avoid the test in the caller).</param>
        public static void RegisterMissing( MissingDisposeCallSentinel s )
        {
            if( s != null )
            {
                lock( _lock )
                {
                    s._nextFail = _lastFail;
                    _lastFail = s;
                    ++_count; 
                }
            }
        }

        /// <summary>
        /// DEBUG must be active (at the caller level). Typical use: <code>DebugCheckMissing( s => Debug.Fail( s ) );</code>.
        /// (<see cref="Debug.Fail(string)"/> can not be used as a delegate because of its own <see cref="ConditionalAttribute"/>.)
        /// Triggers <see cref="GC.Collect(int)"/> to detect missing dispose calls.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void DebugCheckMissing( Action<string> onMissing )
        {
            GC.Collect( 1, GCCollectionMode.Forced );
            GC.WaitForPendingFinalizers();
            string missing = DumpMissing();
            if( missing != null )
            {
                if( onMissing != null ) onMissing( missing );
                else Debug.Fail( missing );
            }
        }

    }

}

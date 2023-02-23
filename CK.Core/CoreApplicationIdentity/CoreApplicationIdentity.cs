using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Exposes the identity of the current application (technically the Application Domain) as an immutable
    /// singleton <see cref="Instance"/>.
    /// <see cref="Configure(Action{Builder},bool)"/> or <see cref="TryConfigure(Action{Builder})"/> methods can be called
    /// until the instance is used. <see cref="OnInitialized(Action)"/> enables deferring actions to wait for the application
    /// identity to be ready.
    /// <para>
    /// The "party" is identified by the <see cref="DomainName"/>, <see cref="EnvironmentName"/> and <see cref="PartyName"/>.
    /// This "party" is the logical process: logs issued by a process can always be grouped by this "party identifier".
    /// </para>
    /// But "process uniqueness" is a complex matter. A process can only be truly unique (at any point in time) by using
    /// synchronization primitives like <see cref="System.Threading.Mutex"/> or specific coordination infrastructure
    /// (election algorithms) in distributed systems.
    /// <para>
    /// At least, the "running instance" itself is necessarily unique because of the <see cref="InstanceId"/>.
    /// </para>
    /// <para>
    /// Other "identity" can be captured by the <see cref="ContextDescriptor"/>: this can use process arguments, working directory
    /// or any other contextual information that helps identify a process. Whether this uniquely identifies the process
    /// is not (and cannot) be handled by this model.
    /// </para>
    /// </summary>
    public sealed partial class CoreApplicationIdentity
    {
        /// <summary>
        /// Gets the name of the domain to which this application belongs.
        /// It cannot empty and defaults to "Undefined". This reserved name
        /// must prevent any logs to be sent to any collector that is not on the machine
        /// that runs this application (this is typically used on developer's machine).
        /// <para>
        /// It must be a case sensitive identifier that should use PascalCase convention if possible:
        /// it must only contain 'A'-'Z', 'a'-'z', '0'-'9' and '_' characters and must not
        /// start with a digit nor a '_'.
        /// </para>
        /// </summary>
        public string DomainName { get; }

        /// <summary>
        /// Gets the name of the environment. Defaults to the empty string.
        /// <para>
        /// When not empty, it must be a case sensitive identifier that should use PascalCase convention if possible:
        /// it must only contain 'A'-'Z', 'a'-'z', '0'-'9' and '_' characters and must not
        /// start with a digit nor a '_'.
        /// </para>
        /// </summary>
        public string EnvironmentName { get; }

        /// <summary>
        /// Gets this party name. Cannot be empty.
        /// <para>
        /// It must be a case sensitive identifier that should use PascalCase convention:
        /// it must only contain 'A'-'Z', 'a'-'z', '0'-'9' and '_' characters and must not
        /// start with a digit nor a '_'.
        /// </para>
        /// <para>
        /// Defaults to a string derived from the <see cref="Environment.ProcessPath"/>.
        /// If the ProcessPath is null, the "Undefined" string is used.
        /// </para>
        /// </summary>
        public string PartyName { get; }

        /// <summary>
        /// Gets a string that identifies the context into which this
        /// application is running. Defaults to the empty string.
        /// <para>
        /// There is no constraint on this string (but shorter is better) except that
        /// the characters 0 to 8 (NUl, SOH, STX, ETX, EOT, ENQ, ACK, BEL, BSP) are
        /// mapped to their respective angle bracket enclosed string representation
        /// (0x0 is mapped to &lt;NUL&gt;, 0x1 to &lt;SOH&gt;, etc.).
        /// </para>
        /// </summary>
        public string ContextDescriptor { get; }

        /// <summary>
        /// Gets a Base64Url encoded opaque string that is the SHA1
        /// of the <see cref="DomainName"/>/<see cref="EnvironmentName"/>/<see cref="PartyName"/>/<see cref="ContextDescriptor"/>.
        /// This identifies this application and its running context (but not this running instance).
        /// </summary>
        public string ContextualId { get; }

        /// <summary>
        /// Gets this <see cref="PartyName"/>-C<see cref="ContextualId"/>.
        /// Since PartyName cannot contain '-', the "-C" acts as an easy separator that can be used.
        /// </summary>
        public string PartyContextualName { get; }

        /// <summary>
        /// Gets this <see cref="PartyName"/>-I<see cref="InstanceId"/>.
        /// Since PartyName cannot contain '-', the "-I" acts as an easy separator that can be used.
        /// </summary>
        public string PartyInstanceName { get; }

        /// <summary>
        /// Gets a Base64Url encoded opaque random string that identifies this running instance.
        /// This is currently a 21 characters (15 bytes of entropy).
        /// <para>
        /// This is a static property: it's available as soon as the process starts.
        /// </para>
        /// </summary>
        public static string InstanceId { get; } = Util.GetRandomBase64UrlString( 21 );

        CoreApplicationIdentity( Builder b )
        {
            DomainName = b.DomainName;
            EnvironmentName = b.EnvironmentName;
            PartyName = b.PartyName ?? "Undefined";
            ContextDescriptor = b.ContextDescriptor ?? "";
            ContextualId = Base64UrlHelper.ToBase64UrlString( SHA1.HashData( Encoding.UTF8.GetBytes( $"{DomainName}/{EnvironmentName}/{PartyName}/{ContextDescriptor}" ) ) );
            PartyContextualName = PartyName + "-C" + ContextualId;
            PartyInstanceName = PartyName + "-I" + InstanceId;
        }

        static Builder? _builder;
        static CoreApplicationIdentity? _instance;

#if DEBUG
        // Method that exists in DEBUG only. Used by tests: the
        // CancellationTokenSource can be reset.
        static CancellationTokenSource _token;

        static void Reset()
        {
            _instance = null;
            _builder = new Builder();
            _token = new CancellationTokenSource();
            IsInitialized = false;
        }
#else
        // Real (release) CancellationTokenSource cannot be reset.
        // Tests are disabled.
        static readonly CancellationTokenSource _token;
#endif

        static CoreApplicationIdentity()
        {
            _builder = new Builder();
            _token = new CancellationTokenSource();
        }

        /// <summary>
        /// Configure the application identity if it's not yet initialized or throws an <see cref="InvalidOperationException"/> otherwise.
        /// </summary>
        /// <param name="configurator">The configuration action.</param>
        /// <param name="initialize">Whether <see cref="Initialize()"/> should be called to lock the identity.</param>
        public static void Configure( Action<Builder> configurator, bool initialize = false )
        {
            lock( _token )
            {
                if( _builder == null ) Throw.InvalidOperationException( "CoreApplicationIdentity is already initialized." );
                else configurator( _builder );
                if( initialize ) Initialize();
            }
        }

        /// <summary>
        /// Tries to configure the application identity if it's not yet initialized.
        /// </summary>
        /// <param name="configurator">The configuration action.</param>
        /// <returns>True if the <paramref name="configurator"/> has been called, false if the <see cref="Instance"/> is already available.</returns>
        public static bool TryConfigure( Action<Builder> configurator )
        {
            if( _builder != null )
            {
                lock( _token )
                {
                    if( _builder != null )
                    {
                        configurator( _builder );
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether the <see cref="Instance"/> has been initialized or
        /// can still be configured.
        /// <para>This is set to true after the callbacks of <see cref="OnInitialized(Action)"/> have been called.</para>
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Registers a callback that will be called when the <see cref="Instance"/> will be available
        /// or immediately if the instance has already been configured.
        /// <para>
        /// Current implementation calls the stored delegates in LIFO order. If this becomes an issue, this will
        /// be changed.
        /// </para>
        /// </summary>
        /// <param name="action">Any action that requires the application's identity to be available.</param>
        public static void OnInitialized( Action action )
        {
            _token.Token.UnsafeRegister( _ => action(), null );
        }

        /// <summary>
        /// Gets the available identity.
        /// The first call to this property triggers the initialization of the identity
        /// and the calls to registered <see cref="OnInitialized(Action)"/> callbacks.
        /// </summary>
        public static CoreApplicationIdentity Instance => Initialize();

        /// <summary>
        /// Ensures that the available identity is initialized.
        /// This locks the <see cref="Instance"/> and calls the registered <see cref="OnInitialized(Action)"/>
        /// callbacks.
        /// </summary>
        public static CoreApplicationIdentity Initialize()
        {
            if( _instance == null )
            {
                bool callInit = false;
                // Simple double check locking.
                lock( _token )
                {
                    if( _instance == null )
                    {
                        Debug.Assert( _builder != null );
                        _instance = _builder.Build();
                        _builder = null;
                        callInit = true;
                    }
                }
                // Calls the callbacks outside the lock.
                if( callInit )
                {
                    _token.Cancel( throwOnFirstException: true );
                    IsInitialized = true;
                }
            }
            return _instance;
        }
    }
}

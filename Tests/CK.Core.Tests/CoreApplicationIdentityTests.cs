using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace CK.Core.Tests
{

    [TestFixture]
    public class CoreApplicationIdentityTests
    {
        [SetUp]
        public void ResetStaticIdentity()
        {
            var m = typeof( CoreApplicationIdentity ).GetMethod( "Reset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static );
            Assume.That( m != null, "This test can only run in DEBUG mode." );
            m.Invoke( null, null );
        }

        [Test]
        public void InstanceId_is_21_chars_length()
        {
            CoreApplicationIdentity.InstanceId.Length.Should().Be( 21 );
        }

        [Test]
        public void IsValidPartyName_check()
        {
            CoreApplicationIdentity.IsValidPartyName( "" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( "$" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( " " ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( "_" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( "_A" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( "A_" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( "-A" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( "A-" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidPartyName( new string( 'P', CoreApplicationIdentity.PartyNameMaxLength + 1 ) ).Should().BeFalse();

            CoreApplicationIdentity.IsValidPartyName( "A" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "A_B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "A__B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "A__B_C" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "A-B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "A--B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "A--B-C" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A_B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A__B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A__B_C" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A-B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A--B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$A--B-C" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidPartyName( "$" + new string( 'P', CoreApplicationIdentity.PartyNameMaxLength ) ).Should().BeTrue();
        }

        [Test]
        public void IsValidDomainName_check()
        {
            CoreApplicationIdentity.IsValidDomainName( "" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( "/" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( "/A" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( "A/" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( "A//B" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( "A/B/C/" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( "A/B//C" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidDomainName( new string( 'D', CoreApplicationIdentity.DomainNameMaxLength + 1 ) ).Should().BeFalse();

            CoreApplicationIdentity.IsValidDomainName( "A" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidDomainName( "A/B" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidDomainName( "A/B/C" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidDomainName( new string( 'D', CoreApplicationIdentity.DomainNameMaxLength ) ).Should().BeTrue();
        }

        [Test]
        public void IsValidEnvironmentName_check()
        {
            CoreApplicationIdentity.IsValidEnvironmentName( "" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "#" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "A" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "/" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "#~" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "#/" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "##" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "#A#" ).Should().BeFalse();
            CoreApplicationIdentity.IsValidEnvironmentName( "#" + new string( 'E', CoreApplicationIdentity.EnvironmentNameMaxLength ) ).Should().BeFalse();

            CoreApplicationIdentity.IsValidEnvironmentName( "#A" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidEnvironmentName( "#AB" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidEnvironmentName( "#_Prod-Lovely_" ).Should().BeTrue();
            CoreApplicationIdentity.IsValidEnvironmentName( "#" + new string( 'E', CoreApplicationIdentity.EnvironmentNameMaxLength - 1 ) ).Should().BeTrue();
        }

        [Test]
        public void PartyName_from_ProcessPath_ultimately_resolves_to_Unknown()
        {
            var valid = CoreApplicationIdentity.Builder.PartyNameFromProcessPath( Environment.ProcessPath );
            CoreApplicationIdentity.IsValidPartyName( valid ).Should().BeTrue();

            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "" ).Should().Be( "Unknown" );
        }

        [Test]
        public void default_CoreApplicationIdentity_is_valid()
        {
            Debug.Assert( CoreApplicationIdentity.DefaultDomainName == "Undefined" );
            Debug.Assert( CoreApplicationIdentity.DefaultEnvironmentName == "#Dev" );
            Debug.Assert( CoreApplicationIdentity.DefaultPartyName == "Unknown" );
            var d = CoreApplicationIdentity.Instance;
            d.DomainName.Should().Be( "Undefined" );
            d.EnvironmentName.Should().Be( "#Dev" );
            d.PartyName.Should().NotBeNullOrWhiteSpace();
            d.PartyName.Should().NotContainAny( "\\", "//", ":" );
            d.PartyName.Should().NotStartWith( "_" ).And.NotStartWith( "-" );
            d.ContextDescriptor.Should().Be( "" );
            d.ContextualId.Should().NotBeNullOrWhiteSpace();
            d.PartyContextualName.Should().Be( d.PartyName + ".C" + d.ContextualId );
            d.PartyInstanceName.Should().Be( d.PartyName + ".I" + CoreApplicationIdentity.InstanceId );
        }

        [Test]
        public void OnInitialized_is_always_called_even_after_initialization()
        {
            // This test shows that CancellationTokenSource reverts the call order.
            // Here we don't really care but this could be annoying in general
            // since this doesn't follow the standard event pattern...
            //
            // This behavior is by design and intended (comment from the CancellationTokenSource.ExecuteCallbackHandlers source code):
            //
            // "We call the delegates in LIFO order on each partition so that callbacks fire 'deepest first'.
            //  This is intended to help with nesting scenarios so that child enlisters cancel before their parents."
            //
            int callOrder = 0;
            int call1 = 0;
            CoreApplicationIdentity.OnInitialized( () => call1 = ++callOrder );
            int call2 = 0;
            CoreApplicationIdentity.OnInitialized( () => call2 = ++callOrder );
            int call3 = 0;
            CoreApplicationIdentity.OnInitialized( () => call3 = ++callOrder );
            int call4 = 0;
            CoreApplicationIdentity.OnInitialized( () => call4 = ++callOrder );

            CoreApplicationIdentity.Initialize();

            int call5 = 0;
            CoreApplicationIdentity.OnInitialized( () => call5 = ++callOrder );

            call1.Should().Be( 4 );
            call2.Should().Be( 3 );
            call3.Should().Be( 2 );
            call4.Should().Be( 1 );
            call5.Should().Be( 5 );
        }

        [Test]
        public void once_Initialized_Configure_fails()
        {
            CoreApplicationIdentity.IsInitialized.Should().BeFalse();
            CoreApplicationIdentity.TryConfigure( b => b.DomainName = "D" ).Should().BeTrue();
            CoreApplicationIdentity.Configure( b => b.PartyName = "Pop", true );

            CoreApplicationIdentity.IsInitialized.Should().BeTrue();

            CoreApplicationIdentity.TryConfigure( b => b.DomainName = "nop" ).Should().BeFalse();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.PartyName = "Pop" ) ).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void max_lengths_are_checked_by_builder()
        {
            CoreApplicationIdentity.IsInitialized.Should().BeFalse();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.DomainName = null! ) ).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.DomainName = "" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.DomainName = "." ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.DomainName = "/A" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.DomainName = new string( 'D', CoreApplicationIdentity.DomainNameMaxLength + 1 ) ) ).Should().Throw<ArgumentException>();

            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = null! ) ).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = "_" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = "A_" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = "" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = "." ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = "A/B" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.EnvironmentName = new string( 'E', CoreApplicationIdentity.EnvironmentNameMaxLength + 1 ) ) ).Should().Throw<ArgumentException>();

            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.PartyName = "" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.PartyName = "." ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.PartyName = "A/B" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.PartyName = new string( 'P', CoreApplicationIdentity.PartyNameMaxLength + 1 ) ) ).Should().Throw<ArgumentException>();

            FluentActions.Invoking( () => CoreApplicationIdentity.Configure( b => b.ContextDescriptor = new string( 'C', CoreApplicationIdentity.ContextDescriptorMaxLength + 1 ) ) ).Should().Throw<ArgumentException>();

            CoreApplicationIdentity.IsInitialized.Should().BeFalse();
        }

        [Test]
        public void PartyNameFromProcessPath_tests()
        {
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( null ).Should().Be( CoreApplicationIdentity.DefaultPartyName );
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "" ).Should().Be( CoreApplicationIdentity.DefaultPartyName );
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "---" ).Should().Be( CoreApplicationIdentity.DefaultPartyName );
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "_" ).Should().Be( CoreApplicationIdentity.DefaultPartyName );
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "0123456789" ).Should().Be( CoreApplicationIdentity.DefaultPartyName );
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "_0_" ).Should().Be( CoreApplicationIdentity.DefaultPartyName );
            CoreApplicationIdentity.Builder.PartyNameFromProcessPath( "Party012345678901234567890123456789" ).Should().Be( "y012345678901234567890123456789" );
        }

        // With party.
        [TestCase( "A/$p", "A", "$p", null )]
        [TestCase( "$p/A", "A", "$p", null )]
        [TestCase( "$p/A/B", "A/B", "$p", null )]
        [TestCase( "A/B/$p", "A/B", "$p", null )]
        [TestCase( "A/$p/B", "A/B", "$p", null )]
        [TestCase( "A/B/C/$p", "A/B/C", "$p", null )]
        [TestCase( "A/B/$p/C", "A/B/C", "$p", null )]
        [TestCase( "A/$p/B/C", "A/B/C", "$p", null )]
        [TestCase( "$p/A/B/C", "A/B/C", "$p", null )]
        // With party and environment.
        [TestCase( "A/$p/#e", "A", "$p", "#e" )]
        [TestCase( "$p/A/#e", "A", "$p", "#e" )]
        [TestCase( "$p/A/B/#e", "A/B", "$p", "#e" )]
        [TestCase( "A/B/$p/#e", "A/B", "$p", "#e" )]
        [TestCase( "A/$p/B/#e", "A/B", "$p", "#e" )]
        [TestCase( "A/B/C/$p/#e", "A/B/C", "$p", "#e" )]
        [TestCase( "A/B/$p/C/#e", "A/B/C", "$p", "#e" )]
        [TestCase( "A/$p/B/C/#e", "A/B/C", "$p", "#e" )]
        [TestCase( "$p/A/B/C/#e", "A/B/C", "$p", "#e" )]
        [TestCase( "A/#e/$p", "A", "$p", "#e" )]
        [TestCase( "$p/#e/A", "A", "$p", "#e" )]
        [TestCase( "$p/A/#e/B", "A/B", "$p", "#e" )]
        [TestCase( "A/B/#e/$p", "A/B", "$p", "#e" )]
        [TestCase( "A/$p/#e/B", "A/B", "$p", "#e" )]
        [TestCase( "#e/A/B/C/$p", "A/B/C", "$p", "#e" )]
        [TestCase( "A/#e/B/$p/C", "A/B/C", "$p", "#e" )]
        [TestCase( "A/$p/B/#e/C", "A/B/C", "$p", "#e" )]
        [TestCase( "#e/$p/A/B/C", "A/B/C", "$p", "#e" )]
        // Domain only.
        [TestCase( "A", "A", null, null )]
        [TestCase( "A/B", "A/B", null, null )]
        // With environment.
        [TestCase( "A/B/#e", "A/B", null, "#e" )]
        [TestCase( "#e/A/B", "A/B", null, "#e" )]
        [TestCase( "A/#e/B", "A/B", null, "#e" )]
        public void TryParseFullName_successful_tests( string? f, string domainName, string? partyName, string? environmentName )
        {
            CoreApplicationIdentity.TryParseFullName( f, out var d, out var p, out var e ).Should().BeTrue();
            d.Should().Be( domainName );
            p.Should().Be( partyName );
            e.Should().Be( environmentName );
        }

        [TestCase( null )]
        [TestCase( "" )]
        [TestCase( "$p" )]
        [TestCase( "#e" )]
        [TestCase( "#e/$p" )]
        [TestCase( "A$p/B/#e" )]
        [TestCase( "A/$p/B#e" )]
        public void TryParseFullName_failed_tests( string f )
        {
            CoreApplicationIdentity.TryParseFullName( f, out _, out _, out _ ).Should().BeFalse();
        }

        [Test]
        public void TryParseFullName_failed_too_long_tests()
        {
            var domainMax = new string( 'd', CoreApplicationIdentity.DomainNameMaxLength );
            var partyMax = new string( 'p', CoreApplicationIdentity.PartyNameMaxLength );
            var environmentMax = '#' + new string( 'e', CoreApplicationIdentity.EnvironmentNameMaxLength - 1 );
            CoreApplicationIdentity.TryParseFullName( $"{domainMax}/${partyMax}/{environmentMax}", out var d, out var p, out var e ).Should().BeTrue();
            d.Should().Be( domainMax );
            p.Should().Be( '$' + partyMax );
            e.Should().Be( environmentMax );

            CoreApplicationIdentity.TryParseFullName( $"X{domainMax}/${partyMax}/{environmentMax}", out _, out _, out _ ).Should().BeFalse();
            CoreApplicationIdentity.TryParseFullName( $"{domainMax}D/$p/#e", out _, out _, out _ ).Should().BeFalse();
            CoreApplicationIdentity.TryParseFullName( $"D/${partyMax}X/#e", out _, out _, out _ ).Should().BeFalse();
            CoreApplicationIdentity.TryParseFullName( $"D/$p/{environmentMax}X", out _, out _, out _ ).Should().BeFalse();

        }
    }

}

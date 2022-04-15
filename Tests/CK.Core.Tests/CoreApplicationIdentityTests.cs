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
        public void default_CoreApplicationIdentity_is_valid()
        {
            var d = CoreApplicationIdentity.Instance;
            d.DomainName.Should().Be( "Undefined" );
            d.EnvironmentName.Should().Be( "" );
            d.PartyName.Should().NotBeNullOrWhiteSpace();
            d.PartyName.Should().NotContainAny( "\\", "//", ":" );
            d.PartyName.Should().NotStartWith( "_" );
            d.ContextDescriptor.Should().Be( "" );
            d.ContextualId.Should().NotBeNullOrWhiteSpace();
            d.PartyContextualName.Should().Be( d.PartyName + "-C" + d.ContextualId );
            d.PartyInstanceName.Should().Be( d.PartyName + "-I" + CoreApplicationIdentity.InstanceId );
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

    }
}

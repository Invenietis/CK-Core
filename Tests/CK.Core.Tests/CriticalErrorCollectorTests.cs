using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{

    public class CriticalErrorCollectorTests
    {

        [Test]
        public void simple_add_exception_to_CriticalErrorCollector()
        {
            CriticalErrorCollector c = new CriticalErrorCollector();
            c.Invoking( sut => sut.Add( null, "" ) ).Should().Throw<ArgumentNullException>();
            c.Add( new Exception( "A" ), null );
            c.Add( new Exception( "B" ), "Comment" );

            var errors = c.ToArray();
            errors[0].ToString().Should().Be( " - A" );
            errors[1].ToString().Should().Be( "Comment - B" );
        }

        [Test]
        public void catching_one_event()
        {
            CriticalErrorCollector c = new CriticalErrorCollector();
            CriticalErrorCollector.ErrorEventArgs catched = null;
            EventHandler<CriticalErrorCollector.ErrorEventArgs> h = ( sender, e ) => catched = e;
            c.OnErrorFromBackgroundThreads += h;
            try
            {
                c.Add( new Exception( "The-Test-Exception-Message" ), "Produced by SimpleTest" );
                c.WaitOnErrorFromBackgroundThreadsPending();
                catched.Should().NotBeNull();
                catched.Errors.Should().HaveCount(1);
                catched.Errors[0].Exception.Message.Should().Contain( "The-Test-Exception-Message" );
                catched.Errors[0].Comment.Should().Contain( "Produced by SimpleTest" );
            }
            finally
            {
                c.OnErrorFromBackgroundThreads -= h;
            }
        }

        [Test]
        public void buggy_handlers_are_removed_and_their_errors_are_caught()
        {
            CriticalErrorCollector c = new CriticalErrorCollector();
            int buggyEventHandlerCount = 0;

            var goodCollector = new List<string>();
            Action<CriticalErrorCollector.ErrorEventArgs> addMsg = errorEvent =>
            {
                lock( goodCollector )
                {
                    foreach( var e in errorEvent.Errors )
                    goodCollector.Add( e.Exception.Message );
                }
            };

            var hGood = new EventHandler<CriticalErrorCollector.ErrorEventArgs>( ( sender, e ) => addMsg( e ) );
            var hBad = new EventHandler<CriticalErrorCollector.ErrorEventArgs>( ( sender, e ) => { ++buggyEventHandlerCount; throw new Exception( "From buggy handler." ); } );
            c.OnErrorFromBackgroundThreads += hGood;
            c.OnErrorFromBackgroundThreads += hBad;
            try
            {
                c.Add( new Exception( "The-Test-Exception-Message" ), "First call" );
                c.WaitOnErrorFromBackgroundThreadsPending();
                buggyEventHandlerCount.Should().Be( 1 );
                goodCollector.Count.Should().Be( 2, "We also received the error of the buggy handler :-)." );
                if( goodCollector.Count != 2 )
                {
                    // Forces the display of the messages.
                    string.Join( Environment.NewLine + "-" + Environment.NewLine, goodCollector )
                        .Should().Be( "Only 2 messages should have been received." );
                }

                c.Add( new Exception( "The-Test-Exception-Message" ), "Second call" );
                c.WaitOnErrorFromBackgroundThreadsPending();
                goodCollector.Count.Should().Be( 3 );
                buggyEventHandlerCount.Should().Be( 1 );
            }
            finally
            {
                c.OnErrorFromBackgroundThreads -= hGood;
                c.OnErrorFromBackgroundThreads -= hBad;
            }
        }
    }
}

using NUnit.Framework;
using System;
using Shouldly;

namespace CK.Core.Tests;

[TestFixture]
public class ShouldlySpecificTests
{
    [Test]
    public void Shouldly_non_generic_ShouldThrow_doesnt_honor_LSP()
    {
        Action bug = () => throw new ArgumentNullException();

        // This is our ShouldThrow (based on Delegate).
        bug.ShouldThrow( typeof(Exception) );

        // Same test using Shouldly's non generic ShouldThrow.
        Action withShouldly = () => Shouldly.ShouldThrowExtensions.ShouldThrow( bug, typeof( Exception ) );
        // It throws an assertion exception.
        withShouldly.ShouldThrow<ShouldAssertException>();
    }

    [Test]
    public void ShouldThrow_on_Delegate_correctly_handles_multicast_Delegate()
    {
        Action bug = () => throw new ArgumentNullException();
        Action noBug = () => {};

        Action? combined = noBug;
        combined += bug;

        combined.ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void ShouldThrow_on_Delegate_checks_that_Delegate_has_no_parameter()
    {
        Action<int> expectParameter = SomeFunc;

        Action sut = () => expectParameter.ShouldThrow<ArgumentNullException>();

        sut.ShouldThrow<ArgumentException>().Message.ShouldBe( """
            ShouldThrow can only be called on a delegate without parameters.
            Found method ShouldlySpecificTests.SomeFunc( Int32 a )
            """ );
    }

    static void SomeFunc( int a ) => throw new ArgumentNullException();
}

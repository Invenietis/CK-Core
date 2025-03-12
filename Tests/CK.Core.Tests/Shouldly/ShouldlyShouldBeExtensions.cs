using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CK.Core.Tests;

/// <summary>
/// Extends Shouldly with useful helpers.
/// </summary>
/// <remarks>
/// This helper is here (and tested here) in CK.Core.
/// CK.Testing refrences Shouldly and exposes this helper.
/// </remarks>
[ShouldlyMethods]
public static class ShouldlyShouldBeExtensions
{
    /// <summary>
    /// Fix https://github.com/shouldly/shouldly/issues/934.
    /// </summary>
    /// <remarks>
    /// This Delegate based method is chosen over the original Should's extension method
    /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
    /// </remarks>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="actual">The action code that should throw.</param>
    /// <param name="customMessage">Optional message.</param>
    /// <returns>The exception instance.</returns>
    public static TException ShouldThrow<TException>( this Delegate actual, string? customMessage = null )
        where TException : Exception
    {
        return (TException)ThrowInternal( actual, customMessage, typeof( TException ), exactType: false );
    }

    /// <summary>
    /// Fix https://github.com/shouldly/shouldly/issues/934.
    /// <para>
    /// Note that this change the semantics and this is intended: Shouldy's
    /// non generic ShouldThrow doesn't honor the Lyskov Substitution Principle (as opposed to the generic one),
    /// the <paramref name="exceptionType"/> must be the exact type of the exception (like our
    /// <see cref="ShouldThrowExactly(Delegate, Type, string?)"/> does).
    /// </para>
    /// </summary>
    /// <remarks>
    /// This Delegate based method is chosen over the original Should's extension method
    /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
    /// </remarks>
    /// <param name="actual">The action code that should throw.</param>
    /// <param name="exceptionType">The expected exception type.</param>
    /// <param name="customMessage">Optional message.</param>
    /// <returns>The exception instance.</returns>
    public static Exception ShouldThrow( this Delegate actual, Type exceptionType, string? customMessage = null )
    {
        return ThrowInternal( actual, customMessage, exceptionType, exactType: false );
    }

    /// <summary>
    /// Fix https://github.com/shouldly/shouldly/issues/934.
    /// </summary>
    /// <remarks>
    /// This Delegate based method is chosen over the original Should's extension method
    /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
    /// </remarks>
    /// <param name="actual">The action code that should throw.</param>
    /// <param name="exceptionType">The exact expected exception type.</param>
    /// <param name="customMessage">Optional message.</param>
    /// <returns>The exception instance.</returns>
    public static TException ShouldThrowExactly<TException>( this Delegate actual, string? customMessage = null )
        where TException : Exception
    {
        return (TException)ThrowInternal( actual, customMessage, typeof( TException ), exactType: true );
    }

    /// <summary>
    /// Fix https://github.com/shouldly/shouldly/issues/934.
    /// </summary>
    /// <remarks>
    /// This Delegate based method is chosen over the original Should's extension method
    /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
    /// </remarks>
    /// <param name="actual">The action code that should throw.</param>
    /// <param name="exceptionType">The exact expected exception type.</param>
    /// <param name="customMessage">Optional message.</param>
    /// <returns>The exception instance.</returns>
    public static Exception ShouldThrowExactly( this Delegate actual, Type exceptionType, string? customMessage = null )
    {
        return ThrowInternal( actual, customMessage, exceptionType, exactType: true );
    }

    static Exception ThrowInternal( Delegate actual,
                                    string? customMessage,
                                    Type expectedExceptionType,
                                    bool exactType,
                                    [CallerMemberName]string? shouldlyMethod = null )
    {
        // Handle composite delegates: consider the invocation list if any.
        var multi = actual.GetInvocationList();
        if( multi.Length == 0 )
        {
            foreach( var d in multi )
            {
                CheckNoParameters( d );
            }
        }
        else
        {
            CheckNoParameters( actual );
        }

        static void CheckNoParameters( Delegate d )
        {
            var parameters = d.Method.GetParameters();
            if( parameters.Length > 0 )
            {
                var parametersDesc = parameters.Select( p => $"{p.ParameterType.Name} {p.Name}" );
                throw new ArgumentException( $"""
                        ShouldThrow can only be called on a delegate without parameters.
                        Found method {d.Method.DeclaringType?.Name}.{d.Method.Name}( {string.Join( ", ", parametersDesc )} )
                        """ );
            }
        }
        try
        {
            if( multi.Length == 0 )
            {
                Execute( actual );
            }
            else
            {
                foreach( var d in multi )
                {
                    Execute( d );
                }
            }

            static void Execute( Delegate d ) => d.Method.Invoke( d.Target, BindingFlags.DoNotWrapExceptions, null, null, null );
        }
        catch( Exception ex )
        {
            if( ex.GetType() == expectedExceptionType
                || (!exactType && expectedExceptionType.IsAssignableFrom( ex.GetType() )) )
            {
                return ex;
            }
            throw new ShouldAssertException( new ShouldlyThrowMessage( expectedExceptionType, ex.GetType(), customMessage, shouldlyMethod! ).ToString(), ex );
        }
        throw new ShouldAssertException( new ShouldlyThrowMessage( expectedExceptionType, customMessage: customMessage, shouldlyMethod! ).ToString() );
    }

    public static void ShouldBe( this NormalizedPath actual, NormalizedPath expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this short actual, short expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this ushort actual, ushort expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this sbyte actual, sbyte expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this byte actual, byte expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this short? actual, short? expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this ushort? actual, ushort? expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this sbyte? actual, sbyte? expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

    public static void ShouldBe( this byte? actual, byte? expected, string? customMessage = null )
    {
        actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
    }

}

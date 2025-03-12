using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace CK.Core.Tests;


#region IAddService

public interface IAddServiceBase
{
}

public interface IAddService : IAddServiceBase
{
    int Add( int a, int b );
}

public interface IAddServiceDerived : IAddService
{
}

public class AddServiceImpl : IAddService
{
    public int Add( int a, int b )
    {
        return a + b;
    }
}

#endregion

#region ISubstractService

public interface ISubstractService
{
    int Substract( int a, int b );
}
public class SubstractServiceImpl : ISubstractService
{
    public int Substract( int a, int b )
    {
        return a - b;
    }
}

#endregion

#region IMultService

public interface IMultService
{
    int Mult( int a, int b );
}

public class MultServiceImpl : IMultService
{
    public int Mult( int a, int b )
    {
        return a * b;
    }
}

#endregion

public class ProvidedClass
{
    public int Age { get; set; }

    public ProvidedClass( int age )
    {
        Age = age;
    }
}

public class DisposableThatReenterClearWhenDisposed : IDisposable
{
    public ISimpleServiceContainer? ServiceContainer { get; set; }

    public void Dispose()
    {
        ServiceContainer?.Clear();
    }
}

public class EmptyClass
{
}


[ExcludeFromCodeCoverage]
public class ServiceContainerTests
{
    [Test]
    public void registering_a_simple_class()
    {
        ISimpleServiceContainer container = new SimpleServiceContainer();
        ProvidedClass providedClass = new ProvidedClass( 5 );
        container.Add( providedClass );

        // NotNullWhen works.
        if( container.TryGetService<ProvidedClass>( out var retrievedObject, false ) )
        {
            retrievedObject.Should().NotBeNull();
            retrievedObject.Age.Should().Be( 5 );
        }
        else Assert.Fail();
    }


    [Test]
    public void registering_an_implementation()
    {
        int removedServicesCount = 0;

        IServiceContainerConformanceTest( new SimpleServiceContainer() );

        ISimpleServiceContainer baseProvider = new SimpleServiceContainer();
        IMultService multService = new MultServiceImpl();
        multService.Mult( 3, 7 ).Should().Be( 21 );
        baseProvider.Add( typeof( IMultService ), multService, o => removedServicesCount++ );
        ISimpleServiceContainer container = new SimpleServiceContainer( baseProvider );

        IServiceContainerConformanceTest( container, baseProvider, baseProvider.GetService<IMultService>( false ) );

        removedServicesCount.Should().Be( 1 );
    }

    [Test]
    public void removing_a_registered_service()
    {
        int removedServicesCount = 0;

        SimpleServiceContainer container = new SimpleServiceContainer();
        container.Add( typeof( IAddService ), new AddServiceImpl(), o => removedServicesCount++ );

        IAddService service = container.GetService<IAddService>( false );
        service.Should().BeOfType<AddServiceImpl>();

        removedServicesCount.Should().Be( 0 );
        container.Remove( typeof( IAddService ) );
        removedServicesCount.Should().Be( 1 );

        container.GetService( typeof( IAddService ) ).Should().BeNull();
        container.GetService<IAddService>( false ).Should().BeNull();
        container.Invoking( sut => sut.GetService<IAddService>( true ) ).Should().Throw<Exception>();
    }

    [Test]
    public void clearing_a_container_disposes_all_its_registered_IDisposable_objects_and_remove_reentrancy_is_handled()
    {
        SimpleServiceContainer container = new SimpleServiceContainer();
        DisposableThatReenterClearWhenDisposed disposableClass = new DisposableThatReenterClearWhenDisposed();
        disposableClass.ServiceContainer = container;
        container.Add( disposableClass, Util.ActionDispose );

        DisposableThatReenterClearWhenDisposed service = container.GetService<DisposableThatReenterClearWhenDisposed>( false );
        service.Should().NotBeNull();
        service.GetType().Should().Be( typeof( DisposableThatReenterClearWhenDisposed ) );

        container.Clear();

        container.GetService( typeof( DisposableThatReenterClearWhenDisposed ) ).Should().BeNull();
        container.GetService<DisposableThatReenterClearWhenDisposed>( false ).Should().BeNull();
        container.Invoking( sut => sut.GetService<DisposableThatReenterClearWhenDisposed>( true ) ).Should().Throw<Exception>();
    }

    [Test]
    public void using_onRemove_action_reentrancy_of_remove_is_handled()
    {

        SimpleServiceContainer container = new SimpleServiceContainer();
        DisposableThatReenterClearWhenDisposed disposableClass = new DisposableThatReenterClearWhenDisposed();
        EmptyClass stupidObject = new EmptyClass();

        disposableClass.ServiceContainer = container;
        container.Add( disposableClass, Util.ActionDispose );
        container.Add( stupidObject, e => container.Clear() );

        DisposableThatReenterClearWhenDisposed service = container.GetService<DisposableThatReenterClearWhenDisposed>( false );
        service.Should().BeOfType<DisposableThatReenterClearWhenDisposed>();

        container.Remove<EmptyClass>();

        container.GetService( typeof( DisposableThatReenterClearWhenDisposed ) ).Should().BeNull();
        container.GetService<DisposableThatReenterClearWhenDisposed>( false ).Should().BeNull();
        container.Invoking( sut => sut.GetService<DisposableThatReenterClearWhenDisposed>( true ) ).Should().Throw<Exception>();
    }

    [Test]
    public void SimpleServiceContainer_exposes_its_own_IServiceProvier_and_ISimpleServiceContainer_implementation()
    {
        SimpleServiceContainer container = new SimpleServiceContainer();
        container.GetService<IServiceProvider>( false ).Should().BeSameAs( container );
        container.GetService<ISimpleServiceContainer>( false ).Should().BeSameAs( container );

        container.Invoking( sut => sut.Add<ISimpleServiceContainer>( container ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<ISimpleServiceContainer>( new SimpleServiceContainer() ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<ISimpleServiceContainer>( JustAFunc<ISimpleServiceContainer> ) ).Should().Throw<Exception>();

        container.Invoking( sut => sut.Add<IServiceProvider>( container ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<IServiceProvider>( new SimpleServiceContainer() ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<IServiceProvider>( JustAFunc<IServiceProvider> ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.AddDisabled( typeof( IServiceProvider ) ) ).Should().Throw<Exception>();

    }

    [Test]
    public void when_registering_types_they_must_match()
    {
        SimpleServiceContainer container = new SimpleServiceContainer();

        container.Invoking( sut => sut.Add( typeof( int ), new ProvidedClass( 5 ), null ) ).Should().Throw<Exception>();

        container.Add( typeof( ProvidedClass ), () => { return new EmptyClass(); }, null );
        container.Invoking( sut => sut.GetService<ProvidedClass>( false ) ).Should().Throw<Exception>();
    }

    [ExcludeFromCodeCoverage]
    static object JustAFunc() { return null!; }

    [ExcludeFromCodeCoverage]
    static T JustAFunc<T>() where T : class { return null!; }

    [Test]
    public void checking_null_arguments()
    {
        ISimpleServiceContainer container = new SimpleServiceContainer();

        //SimpleServiceContainer.Add( Type serviceType, object serviceInstance, Action<Object> onRemove = null )
        container.Invoking( sut => sut.Add( null!, new ProvidedClass( 5 ) ) ).Should().Throw<ArgumentNullException>();
        container.Invoking( sut => sut.Add( typeof( ProvidedClass ), (object)null! ) ).Should().Throw<ArgumentNullException>();

        //SimpleServiceContainer.Add( Type serviceType, Func<Object> serviceInstance, Action<Object> onRemove = null )
        container.Invoking( sut => sut.Add( null!, JustAFunc ) ).Should().Throw<ArgumentNullException>();
        container.Invoking( sut => sut.Add( typeof( ProvidedClass ), (Func<Object>)null! ) ).Should().Throw<ArgumentNullException>();

        container.Invoking( sut => sut.AddDisabled( null! ) ).Should().Throw<ArgumentNullException>();
        container.Invoking( sut => sut.GetService( null! ) ).Should().Throw<ArgumentNullException>();
        container.Invoking( sut => sut.Add<ProvidedClass>( JustAFunc<ProvidedClass>, null! ) ).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void container_can_be_chained_an_loops_are_detected()
    {
        SimpleServiceContainer firstContainer = new SimpleServiceContainer();
        SimpleServiceContainer secondContainer = new SimpleServiceContainer();
        SimpleServiceContainer thirdContainer = new SimpleServiceContainer();

        firstContainer.Invoking( sut => sut.BaseProvider = firstContainer ).Should().Throw<Exception>();

        //firstContainer( secondContainer )
        firstContainer.BaseProvider = secondContainer;
        secondContainer.Invoking( sut => sut.BaseProvider = firstContainer ).Should().Throw<Exception>();

        //firstContainer( secondContainer( thirdContainer ) )
        secondContainer.BaseProvider = thirdContainer;
        thirdContainer.Invoking( sut => sut.BaseProvider = firstContainer ).Should().Throw<Exception>();
        thirdContainer.Invoking( sut => sut.BaseProvider = secondContainer ).Should().Throw<Exception>();

        //firstContainer( thirdContainer ) and secondContainer( thirdContainer ) 
        firstContainer.BaseProvider = thirdContainer;
        thirdContainer.Invoking( sut => sut.BaseProvider = secondContainer ).Should().Throw<Exception>();
        thirdContainer.Invoking( sut => sut.BaseProvider = firstContainer ).Should().Throw<Exception>();

    }

    /// <summary>
    /// Tests the fact that the ISimpleServiceContainer set as parameter is conform to the way the interface should be used.
    /// </summary>
    /// <param name="container">the ISimpleServiceContainer implementation to test</param>
    public void IServiceContainerConformanceTest( ISimpleServiceContainer container )
    {
        IServiceContainerConformanceTest<object>( container, null!, null! );
    }

    /// <summary>
    /// Tests the fact that the ISimpleServiceContainer set as parameter is conform to the way the interface should be used.
    /// </summary>
    /// <typeparam name="T">the service implemented by the servicecontainer's baseprovider </typeparam>
    /// <param name="container">the ISimpleServiceContainer implementation to test</param>
    /// <param name="baseProviderServiceToTest"></param>
    public void IServiceContainerConformanceTest<T>( ISimpleServiceContainer container, ISimpleServiceContainer baseProvider, T baseProviderServiceToTest )
    {
        Func<IAddService> creatorFunc = () => new AddServiceImpl();

        IServiceContainerCoAndContravariance( container, creatorFunc );

        IServiceContainerConformanceAddRemove( container, creatorFunc );

        IServiceContainerConformanceAddFailsWhenExisting( container, creatorFunc );

        IServiceContainerConformanceRemoveRecursive( container );

        container.Add<IAddService>( creatorFunc );
        container.Add<ISubstractService>( new SubstractServiceImpl() );

        IAddService service = container.GetService<IAddService>( false );
        service.Should().NotBeNull();
        service.GetType().Should().Be( typeof( AddServiceImpl ) );
        service.Add( 1, 1 ).Should().Be( 2 );

        ISubstractService substractService = container.GetService<ISubstractService>( false );
        substractService.Should().NotBeNull();
        substractService.GetType().Should().Be( typeof( SubstractServiceImpl ) );
        substractService.Substract( 1, 1 ).Should().Be( 0 );

        //clear test
        container.Clear();

        container.GetService<IAddService>( false ).Should().BeNull();
        container.GetService<ISubstractService>( false ).Should().BeNull();

        //base provider test
        if( baseProvider != null && baseProviderServiceToTest != null )
        {
            T baseService = container.GetService<T>( false );
            baseService.Should().NotBeNull( "The baseProvider contains the specified service." );

            container.Remove( typeof( T ) );
            baseService = container.GetService<T>( false );
            baseService.Should().NotBeNull( "Trying to remove a base service from a child provider does nothing." );

            container.AddDisabled( typeof( T ) );
            container.GetService<T>( false ).Should().BeNull( "Access to this service is disabled" );

            baseProvider.Remove( typeof( T ) );
            container.GetService<T>( false ).Should().BeNull( "Access to this service is disabled & The service doesn't exist anymore on the baseProvider" );

            container.Remove( typeof( T ) );
            container.GetService<T>( false ).Should().BeNull( "The service doesn't exist anymore on the baseProvider" );

            baseProvider.Add( baseProviderServiceToTest );
            container.GetService<T>( false ).Should().NotBeNull( "Back to the beginning's state, the service is retrieved from the base provider." );
        }
    }

    private static void IServiceContainerConformanceRemoveRecursive( ISimpleServiceContainer container )
    {
        bool removedCall = false;
        container.Add<IAddService>( new AddServiceImpl(), s => { removedCall = true; container.Remove( typeof( IAddService ) ); } );
        container.GetService<IAddService>( false ).Should().NotBeNull();
        container.Remove( typeof( IAddService ) );
        removedCall.Should().BeTrue( "OnRemove has been called and can safely remove the service again without stack overflow exception." );
    }

    private static void IServiceContainerConformanceAddFailsWhenExisting( ISimpleServiceContainer container, Func<IAddService> creatorFunc )
    {
        container.Add<IAddService>( new AddServiceImpl() );
        container.Invoking( sut => sut.Add( creatorFunc ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<IAddService>( creatorFunc, s => { } ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add( typeof( IAddService ), new AddServiceImpl() ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add( typeof( IAddService ), new AddServiceImpl(), s => { } ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<IAddService>( new AddServiceImpl() ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.Add<IAddService>( new AddServiceImpl(), s => { } ) ).Should().Throw<Exception>();
        container.Invoking( sut => sut.AddDisabled( typeof( IAddService ) ) ).Should().Throw<Exception>();
        container.Remove( typeof( IAddService ) );
    }

    private static void IServiceContainerConformanceAddRemove( ISimpleServiceContainer container, Func<IAddService> creatorFunc )
    {
        container.GetService<IAddService>( false ).Should().BeNull( "Starting with no IAddService." );

        container.Add( creatorFunc );
        container.Invoking( sut => sut.Add( creatorFunc ) ).Should().Throw<Exception>( "Adding an already existing service throws an exception." );

        container.GetService<IAddService>( false ).Should().NotBeNull( "Deferred creation occured." );
        container.Remove( typeof( IAddService ) );
        container.GetService<IAddService>( false ).Should().BeNull( "Remove works." );

        // Removing an unexisting service is okay.
        container.Remove( typeof( IAddService ) );

        bool removed = false;
        container.Add<IAddService>( creatorFunc, s => removed = true );
        container.Remove( typeof( IAddService ) );
        removed.Should().BeFalse( "Since the service has never been required, it has not been created, hence, OnRemove action has not been called." );

        container.Add<IAddService>( creatorFunc, s => removed = true );
        container.GetService<IAddService>( false ).Should().NotBeNull( "Service has been created." );
        container.Remove( typeof( IAddService ) );
        removed.Should().BeTrue( "This time, OnRemove action has been called." );

        removed = false;
        container.Add<IAddService>( new AddServiceImpl(), s => removed = true );
        container.Remove( typeof( IAddService ) );
        removed.Should().BeTrue( "Since the service instance has been added explicitely, OnRemove action has been called." );
    }

    private static void IServiceContainerCoAndContravariance( ISimpleServiceContainer container, Func<IAddService> creatorFunc )
    {
        {
            _onRemoveServiceCalled = false;
            container.Add<IAddService>( new AddServiceImpl(), OnRemoveService );
            container.Remove( typeof( IAddService ) );
            _onRemoveServiceCalled.Should().BeTrue( "OnRemoveService has been called." );

            _onRemoveServiceCalled = false;
            container.Add<IAddService>( new AddServiceImpl(), OnRemoveBaseServiceType );
            container.Remove( typeof( IAddService ) );
            _onRemoveServiceCalled.Should().BeTrue( "OnRemoveBaseServiceType has been called." );

            _onRemoveServiceCalled = false;
            container.Add<IAddService>( new AddServiceImpl(), OnRemoveServiceObject );
            container.Remove( typeof( IAddService ) );
            _onRemoveServiceCalled.Should().BeTrue( "OnRemoveServiceObject has been called." );

            //container.Add<IAddService>( new AddServiceImpl(), OnRemoveDerivedServiceType );
            //container.Remove( typeof( IAddService ) );

            //container.Add<IAddService>( new AddServiceImpl(), OnRemoveUnrelatedType );
            //container.Remove( typeof( IAddService ) );
        }
        {
            _onRemoveServiceCalled = false;
            container.Add( creatorFunc, OnRemoveService );
            container.Remove( typeof( IAddService ) );
            _onRemoveServiceCalled.Should().BeFalse( "Service has never been created." );

            container.Add<IAddService>( creatorFunc, OnRemoveBaseServiceType );
            container.Remove( typeof( IAddService ) );
            _onRemoveServiceCalled.Should().BeFalse( "Service has never been created." );

            container.Add<IAddService>( creatorFunc, OnRemoveServiceObject );
            container.Remove( typeof( IAddService ) );
            _onRemoveServiceCalled.Should().BeFalse( "Service has never been created." );
        }
    }

    static bool _onRemoveServiceCalled;
    static void OnRemoveService( IAddService s )
    {
        _onRemoveServiceCalled = true;
    }

    static void OnRemoveServiceObject( object o )
    {
        _onRemoveServiceCalled = true;
    }

    static void OnRemoveBaseServiceType( IAddServiceBase baseType )
    {
        _onRemoveServiceCalled = true;
    }

}

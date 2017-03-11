using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CK.Core.Tests
{

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
        public ISimpleServiceContainer ServiceContainer { get; set; }

        public void Dispose()
        {
            ServiceContainer.Clear();
        }
    }

    public class EmptyClass
    {
    }


    [ExcludeFromCodeCoverage]
    public class ServiceContainerTests
    {
        [Fact]
        public void registering_a_simple_class()
        {
            ISimpleServiceContainer container = new SimpleServiceContainer();
            ProvidedClass providedClass = new ProvidedClass(5);
            container.Add(providedClass);

            ProvidedClass retrievedObject = container.GetService<ProvidedClass>();
            retrievedObject.Should().NotBeNull();
            retrievedObject.Age.Should().Be(5);
        }


        [Fact]
        public void registering_an_implementation()
        {
            int removedServicesCount = 0;

            IServiceContainerConformanceTest(new SimpleServiceContainer());

            ISimpleServiceContainer baseProvider = new SimpleServiceContainer();
            IMultService multService = new MultServiceImpl();
            multService.Mult(3, 7).Should().Be(21);
            baseProvider.Add(typeof(IMultService), multService, o => removedServicesCount++);
            ISimpleServiceContainer container = new SimpleServiceContainer(baseProvider);

            IServiceContainerConformanceTest(container, baseProvider, baseProvider.GetService<IMultService>());

            removedServicesCount.Should().Be(1);
        }

        [Fact]
        public void removing_a_registered_service()
        {
            int removedServicesCount = 0;

            SimpleServiceContainer container = new SimpleServiceContainer();
            container.Add(typeof(IAddService), new AddServiceImpl(), o => removedServicesCount++);

            IAddService service = container.GetService<IAddService>();
            service.Should().BeOfType<AddServiceImpl>();

            removedServicesCount.Should().Be(0);
            container.Remove(typeof(IAddService));
            removedServicesCount.Should().Be(1);

            container.GetService(typeof(IAddService)).Should().BeNull();
            container.GetService<IAddService>().Should().BeNull();
            Should.Throw<CKException>(() => container.GetService<IAddService>(true));
        }

        [Fact]
        public void clearing_a_container_disposes_all_its_registered_IDisposable_objects_and_remove_reentrancy_is_handled()
        {
            SimpleServiceContainer container = new SimpleServiceContainer();
            DisposableThatReenterClearWhenDisposed disposableClass = new DisposableThatReenterClearWhenDisposed();
            disposableClass.ServiceContainer = container;
            container.Add(disposableClass, Util.ActionDispose);

            DisposableThatReenterClearWhenDisposed service = container.GetService<DisposableThatReenterClearWhenDisposed>();
            service.Should().NotBeNull();
            service.GetType().Should().Be(typeof(DisposableThatReenterClearWhenDisposed));

            container.Clear();

            container.GetService(typeof(DisposableThatReenterClearWhenDisposed)).Should().BeNull();
            container.GetService<DisposableThatReenterClearWhenDisposed>().Should().BeNull();
            Should.Throw<CKException>(() => container.GetService<DisposableThatReenterClearWhenDisposed>(true));
        }

        [Fact]
        public void using_onRemove_action_reentrancy_of_remove_is_handled()
        {

            SimpleServiceContainer container = new SimpleServiceContainer();
            DisposableThatReenterClearWhenDisposed disposableClass = new DisposableThatReenterClearWhenDisposed();
            EmptyClass stupidObject = new EmptyClass();

            disposableClass.ServiceContainer = container;
            container.Add(disposableClass, Util.ActionDispose);
            container.Add(stupidObject, e => container.Clear());

            DisposableThatReenterClearWhenDisposed service = container.GetService<DisposableThatReenterClearWhenDisposed>();
            service.Should().BeOfType<DisposableThatReenterClearWhenDisposed>();

            container.Remove<EmptyClass>();

            container.GetService(typeof(DisposableThatReenterClearWhenDisposed)).Should().BeNull();
            container.GetService<DisposableThatReenterClearWhenDisposed>().Should().BeNull();
            Should.Throw<CKException>(() => container.GetService<DisposableThatReenterClearWhenDisposed>(true));
        }

        [Fact]
        public void SimpleServiceContainer_exposes_its_own_IServiceProvier_and_ISimpleServiceContainer_implementation()
        {
            SimpleServiceContainer container = new SimpleServiceContainer();
            container.GetService<IServiceProvider>().Should().BeSameAs(container);
            container.GetService<ISimpleServiceContainer>().Should().BeSameAs(container);

            Should.Throw<CKException>(() => container.Add<ISimpleServiceContainer>(container));
            Should.Throw<CKException>(() => container.Add<ISimpleServiceContainer>(new SimpleServiceContainer()));
            Should.Throw<CKException>(() => container.Add<ISimpleServiceContainer>(JustAFunc<ISimpleServiceContainer>));

            Should.Throw<CKException>(() => container.Add<IServiceProvider>(container));
            Should.Throw<CKException>(() => container.Add<IServiceProvider>(new SimpleServiceContainer()));
            Should.Throw<CKException>(() => container.Add<IServiceProvider>(JustAFunc<IServiceProvider>));
            Should.Throw<CKException>(() => container.AddDisabled(typeof(IServiceProvider)));

        }

        [Fact]
        public void when_registering_types_they_must_match()
        {
            SimpleServiceContainer container = new SimpleServiceContainer();

            Should.Throw<CKException>(() => container.Add(typeof(int), new ProvidedClass(5), null));

            container.Add(typeof(ProvidedClass), () => { return new EmptyClass(); }, null);
            Should.Throw<CKException>(() => container.GetService<ProvidedClass>());
        }

        [ExcludeFromCodeCoverage]
        static object JustAFunc() { return null; }

        [ExcludeFromCodeCoverage]
        static T JustAFunc<T>() where T : class { return null; }

        [Fact]
        public void checking_null_arguments()
        {
            ISimpleServiceContainer container = new SimpleServiceContainer();

            //SimpleServiceContainer.Add( Type serviceType, object serviceInstance, Action<Object> onRemove = null )
            Should.Throw<ArgumentNullException>(() => container.Add(null, new ProvidedClass(5)));
            Should.Throw<ArgumentNullException>(() => container.Add(typeof(ProvidedClass), (object)null));

            //SimpleServiceContainer.Add( Type serviceType, Func<Object> serviceInstance, Action<Object> onRemove = null )
            Should.Throw<ArgumentNullException>(() => container.Add(null, JustAFunc));
            Should.Throw<ArgumentNullException>(() => container.Add(typeof(ProvidedClass), (Func<Object>)null));

            Should.Throw<ArgumentNullException>(() => container.AddDisabled(null));
            Should.Throw<ArgumentNullException>(() => container.GetService(null));
            Should.Throw<ArgumentNullException>(() => container.Add<ProvidedClass>(JustAFunc<ProvidedClass>, null));
        }

        [Fact]
        public void container_can_be_chained_an_loops_are_detected()
        {
            SimpleServiceContainer firstContainer = new SimpleServiceContainer();
            SimpleServiceContainer secondContainer = new SimpleServiceContainer();
            SimpleServiceContainer thirdContainer = new SimpleServiceContainer();

            Should.Throw<CKException>(() => firstContainer.BaseProvider = firstContainer);

            //firstContainer( secondContainer )
            firstContainer.BaseProvider = secondContainer;
            Should.Throw<CKException>(() => secondContainer.BaseProvider = firstContainer);

            //firstContainer( secondContainer( thirdContainer ) )
            secondContainer.BaseProvider = thirdContainer;
            Should.Throw<CKException>(() => thirdContainer.BaseProvider = firstContainer);
            Should.Throw<CKException>(() => thirdContainer.BaseProvider = secondContainer);

            //firstContainer( thirdContainer ) and secondContainer( thirdContainer ) 
            firstContainer.BaseProvider = thirdContainer;
            Should.Throw<CKException>(() => thirdContainer.BaseProvider = secondContainer);
            Should.Throw<CKException>(() => thirdContainer.BaseProvider = firstContainer);

        }

        /// <summary>
        /// Tests the fact that the ISimpleServiceContainer set as parameter is conform to the way the interface should be used.
        /// </summary>
        /// <param name="container">the ISimpleServiceContainer implementation to test</param>
        public void IServiceContainerConformanceTest(ISimpleServiceContainer container)
        {
            IServiceContainerConformanceTest<object>(container, null, null);
        }

        /// <summary>
        /// Tests the fact that the ISimpleServiceContainer set as parameter is conform to the way the interface should be used.
        /// </summary>
        /// <typeparam name="T">the service implemented by the servicecontainer's baseprovider </typeparam>
        /// <param name="container">the ISimpleServiceContainer implementation to test</param>
        /// <param name="baseProviderServiceToTest"></param>
        public void IServiceContainerConformanceTest<T>(ISimpleServiceContainer container, ISimpleServiceContainer baseProvider, T baseProviderServiceToTest)
        {
            Func<IAddService> creatorFunc = () => new AddServiceImpl();

            IServiceContainerCoAndContravariance(container, creatorFunc);

            IServiceContainerConformanceAddRemove(container, creatorFunc);

            IServiceContainerConformanceAddFailsWhenExisting(container, creatorFunc);

            IServiceContainerConformanceRemoveRecursive(container);

            container.Add<IAddService>(creatorFunc);
            container.Add<ISubstractService>(new SubstractServiceImpl());

            IAddService service = container.GetService<IAddService>();
            service.Should().NotBeNull();
            service.GetType().Should().Be(typeof(AddServiceImpl));
            service.Add(1, 1).Should().Be(2);

            ISubstractService substractService = container.GetService<ISubstractService>();
            substractService.Should().NotBeNull();
            substractService.GetType().Should().Be(typeof(SubstractServiceImpl));
            substractService.Substract(1, 1).Should().Be(0);

            //clear test
            container.Clear();

            container.GetService<IAddService>().Should().BeNull();
            container.GetService<ISubstractService>().Should().BeNull();

            //base provider test
            if (baseProvider != null && baseProviderServiceToTest != null)
            {
                T baseService = container.GetService<T>();
                baseService.Should().NotBeNull("The baseProvider contains the specified service.");

                container.Remove(typeof(T));
                baseService = container.GetService<T>();
                baseService.Should().NotBeNull("Trying to remove a base service from a child provider does nothing.");

                container.AddDisabled(typeof(T));
                container.GetService<T>().Should().BeNull("Access to this service is disabled");

                baseProvider.Remove(typeof(T));
                container.GetService<T>().Should().BeNull("Access to this service is disabled & The service doesn't exist anymore on the baseProvider");

                container.Remove(typeof(T));
                container.GetService<T>().Should().BeNull("The service doesn't exist anymore on the baseProvider");

                baseProvider.Add(baseProviderServiceToTest);
                container.GetService<T>().Should().NotBeNull("Back to the beginning's state, the service is retrieved from the base provider.");
            }
        }

        private static void IServiceContainerConformanceRemoveRecursive(ISimpleServiceContainer container)
        {
            bool removedCall = false;
            container.Add<IAddService>(new AddServiceImpl(), s => { removedCall = true; container.Remove(typeof(IAddService)); });
            container.GetService<IAddService>().Should().NotBeNull();
            container.Remove(typeof(IAddService));
            removedCall.Should().BeTrue("OnRemove has been called and can safely remove the service again without stack overflow exception.");
        }

        private static void IServiceContainerConformanceAddFailsWhenExisting(ISimpleServiceContainer container, Func<IAddService> creatorFunc)
        {
            container.Add<IAddService>(new AddServiceImpl());
            Should.Throw<CKException>(() => container.Add(creatorFunc));
            Should.Throw<CKException>(() => container.Add<IAddService>(creatorFunc, s => { }));
            Should.Throw<CKException>(() => container.Add(typeof(IAddService), new AddServiceImpl()));
            Should.Throw<CKException>(() => container.Add(typeof(IAddService), new AddServiceImpl(), s => { }));
            Should.Throw<CKException>(() => container.Add<IAddService>(new AddServiceImpl()));
            Should.Throw<CKException>(() => container.Add<IAddService>(new AddServiceImpl(), s => { }));
            Should.Throw<CKException>(() => container.AddDisabled(typeof(IAddService)));
            container.Remove(typeof(IAddService));
        }

        private static void IServiceContainerConformanceAddRemove(ISimpleServiceContainer container, Func<IAddService> creatorFunc)
        {
            container.GetService<IAddService>().Should().BeNull("Starting with no IAddService.");

            container.Add<IAddService>(creatorFunc);
            Should.Throw<CKException>(() => container.Add<IAddService>(creatorFunc), "Adding an already existing service throws an exception.");

            container.GetService<IAddService>().Should().NotBeNull("Deferred creation occured.");
            container.Remove(typeof(IAddService));
            container.GetService<IAddService>().Should().BeNull("Remove works.");

            // Removing an unexisting service is okay.
            container.Remove(typeof(IAddService));

            bool removed = false;
            container.Add<IAddService>(creatorFunc, s => removed = true);
            container.Remove(typeof(IAddService));
            removed.Should().BeFalse("Since the service has never been required, it has not been created, hence, OnRemove action has not been called.");

            container.Add<IAddService>(creatorFunc, s => removed = true);
            container.GetService<IAddService>().Should().NotBeNull("Service has been created.");
            container.Remove(typeof(IAddService));
            removed.Should().BeTrue("This time, OnRemove action has been called.");

            removed = false;
            container.Add<IAddService>(new AddServiceImpl(), s => removed = true);
            container.Remove(typeof(IAddService));
            removed.Should().BeTrue("Since the service instance has been added explicitely, OnRemove action has been called.");
        }

        private static void IServiceContainerCoAndContravariance(ISimpleServiceContainer container, Func<IAddService> creatorFunc)
        {
            {
                _onRemoveServiceCalled = false;
                container.Add<IAddService>(new AddServiceImpl(), OnRemoveService);
                container.Remove(typeof(IAddService));
                _onRemoveServiceCalled.Should().BeTrue("OnRemoveService has been called.");

                _onRemoveServiceCalled = false;
                container.Add<IAddService>(new AddServiceImpl(), OnRemoveBaseServiceType);
                container.Remove(typeof(IAddService));
                _onRemoveServiceCalled.Should().BeTrue("OnRemoveBaseServiceType has been called.");

                _onRemoveServiceCalled = false;
                container.Add<IAddService>(new AddServiceImpl(), OnRemoveServiceObject);
                container.Remove(typeof(IAddService));
                _onRemoveServiceCalled.Should().BeTrue("OnRemoveServiceObject has been called.");

                //container.Add<IAddService>( new AddServiceImpl(), OnRemoveDerivedServiceType );
                //container.Remove( typeof( IAddService ) );

                //container.Add<IAddService>( new AddServiceImpl(), OnRemoveUnrelatedType );
                //container.Remove( typeof( IAddService ) );
            }
            {
                _onRemoveServiceCalled = false;
                container.Add(creatorFunc, OnRemoveService);
                container.Remove(typeof(IAddService));
                _onRemoveServiceCalled.Should().BeFalse("Service has never been created.");

                container.Add<IAddService>(creatorFunc, OnRemoveBaseServiceType);
                container.Remove(typeof(IAddService));
                _onRemoveServiceCalled.Should().BeFalse("Service has never been created.");

                container.Add<IAddService>(creatorFunc, OnRemoveServiceObject);
                container.Remove(typeof(IAddService));
                _onRemoveServiceCalled.Should().BeFalse("Service has never been created.");
            }
        }

        static bool _onRemoveServiceCalled;
        static void OnRemoveService(IAddService s)
        {
            _onRemoveServiceCalled = true;
        }

        static void OnRemoveServiceObject(object o)
        {
            _onRemoveServiceCalled = true;
        }

        static void OnRemoveBaseServiceType(IAddServiceBase baseType)
        {
            _onRemoveServiceCalled = true;
        }

    }
}

#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\ServiceContainerTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;
using Core;
using System.ComponentModel.Design;

namespace Core
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

    public class DisposableClass : IDisposable
    {
        public ISimpleServiceContainer ServiceContainer { get; set; }
        public void Dispose()
        {
            ServiceContainer.Clear();
        }
    }

    public class MockClass
    {

    }

    [TestFixture]
    public class ServiceContainerTests
    {
        [Test]
        public void SimpleServiceContainerClassTest()
        {           
            ISimpleServiceContainer container = new SimpleServiceContainer();
            ProvidedClass providedClass = new ProvidedClass( 5 );
            container.Add( providedClass );

            ProvidedClass retrievedObject = container.GetService<ProvidedClass>();
            Assert.That( retrievedObject, Is.Not.Null );
            Assert.That( retrievedObject.Age, Is.EqualTo( 5 ) );
        }

 

        [Test]
        public void SimpleServiceContainerTest()
        {
            int removedServicesCount = 0;

            IServiceContainerConformanceTest( new SimpleServiceContainer() );

            ISimpleServiceContainer baseProvider = new SimpleServiceContainer();
            IMultService multService = new MultServiceImpl();
            baseProvider.Add( typeof( IMultService ), multService, o => removedServicesCount++ );
            ISimpleServiceContainer container = new SimpleServiceContainer( baseProvider );

            IServiceContainerConformanceTest( container, baseProvider, baseProvider.GetService<IMultService>() );

            Assert.That( removedServicesCount, Is.EqualTo( 1 ) );
        }

        [Test]
        public void SimpleServiceContainerFallbackTest()
        {
            int removedServicesCount = 0;

            SimpleServiceContainer container = new SimpleServiceContainer();
            container.Add( typeof( IAddService ), new AddServiceImpl(),  o => removedServicesCount++  );

            IAddService service = container.GetService<IAddService>();
            Assert.That( service != null );
            Assert.That( service.GetType(), Is.EqualTo( typeof( AddServiceImpl ) ) );
            Assert.That( service.Add( 1, 1 ), Is.EqualTo( 2 ) );

            Assert.That( removedServicesCount, Is.EqualTo( 0 ) );
            container.Remove( typeof( IAddService ) );
            Assert.That( removedServicesCount, Is.EqualTo( 1 ) );

            Assert.That( container.GetService( typeof(IAddService) ), Is.Null );
            Assert.That( container.GetService<IAddService>(), Is.Null );
            Assert.Throws<CKException>( () => container.GetService<IAddService>( true ) );
        }

        [Test]
        public void SimpleServiceContainerRemoveFallbackTest()
        {

            SimpleServiceContainer container = new SimpleServiceContainer();
            DisposableClass disposableClass = new DisposableClass();
            disposableClass.ServiceContainer = container;
            container.Add<DisposableClass>( disposableClass, Util.ActionDispose );

            DisposableClass service = container.GetService<DisposableClass>();
            Assert.That( service != null );
            Assert.That( service.GetType(), Is.EqualTo( typeof( DisposableClass ) ) );

            container.Clear();

            Assert.That( container.GetService( typeof( DisposableClass ) ), Is.Null );
            Assert.That( container.GetService<DisposableClass>(), Is.Null );
            Assert.Throws<CKException>( () => container.GetService<DisposableClass>( true ) );
        }

        [Test]
        public void SimpleServiceContainerClearFallbackTest()
        {

            SimpleServiceContainer container = new SimpleServiceContainer();
            DisposableClass disposableClass = new DisposableClass();
            MockClass mockClass = new MockClass();

            disposableClass.ServiceContainer = container;
            container.Add<DisposableClass>( disposableClass, Util.ActionDispose );
            container.Add<MockClass>( mockClass, e => container.Clear() );

            DisposableClass service = container.GetService<DisposableClass>();
            Assert.That( service != null );
            Assert.That( service.GetType(), Is.EqualTo( typeof( DisposableClass ) ) );

            container.Clear();

            Assert.That( container.GetService( typeof( DisposableClass ) ), Is.Null );
            Assert.That( container.GetService<DisposableClass>(), Is.Null );
            Assert.Throws<CKException>( () => container.GetService<DisposableClass>( true ) );
        }

        [Test]
        public void SimpleServiceContainerGetDirectServiceTest()
        {
            SimpleServiceContainer container = new SimpleServiceContainer();
            Assert.That( container.GetService<IServiceProvider>() == container );
            Assert.That( container.GetService<ISimpleServiceContainer>() == container );

            Assert.Throws<CKException>( () => container.Add<ISimpleServiceContainer>( container ) );
            Assert.Throws<CKException>( () => container.Add<ISimpleServiceContainer>( new SimpleServiceContainer() ) );
            Assert.Throws<CKException>( () => container.AddDisabled( typeof( ISimpleServiceContainer ) ) );

            Assert.Throws<CKException>( () => container.Add<IServiceProvider>( container ) );
            Assert.Throws<CKException>( () => container.Add<IServiceProvider>( new SimpleServiceContainer() ) );
            Assert.Throws<CKException>( () => container.AddDisabled( typeof( IServiceProvider ) ) );

        }

        /// <summary>
        /// Tests the fact that the ISimpleServiceContainer set as parameter is conform to the way the interface should be used.
        /// </summary>
        /// <param name="container">the ISimpleServiceContainer implementation to test</param>
        public void IServiceContainerConformanceTest( ISimpleServiceContainer container )
        {
            IServiceContainerConformanceTest<object>( container, null, null );
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

            IServiceContainerConformanceRemoveRecusive( container );

            container.Add<IAddService>( creatorFunc );
            container.Add<ISubstractService>( new SubstractServiceImpl() );

            IAddService service = container.GetService<IAddService>();
            Assert.That( service != null );
            Assert.That( service.GetType(), Is.EqualTo( typeof( AddServiceImpl ) ) );
            Assert.That( service.Add( 1, 1 ), Is.EqualTo( 2 ) );
            
            ISubstractService substractService = container.GetService<ISubstractService>();
            Assert.That( substractService != null );
            Assert.That( substractService.GetType(), Is.EqualTo( typeof( SubstractServiceImpl ) ) );
            Assert.That( substractService.Substract( 1, 1 ), Is.EqualTo( 0 ) );
            
            //clear test
            container.Clear();

            Assert.That( container.GetService<IAddService>(), Is.Null );
            Assert.That( container.GetService<ISubstractService>(), Is.Null );            

            //base provider test
            if( baseProvider != null && baseProviderServiceToTest != null)
            {             
                T baseService = container.GetService<T>();
                Assert.That( baseService != null,"The baseProvider contains the specified service.");
                
                container.Remove( typeof( T ) );
                baseService = container.GetService<T>();
                Assert.That( baseService != null, "Trying to remove a base service from a child provider does nothing." );

                container.AddDisabled( typeof(T) );
                Assert.That( container.GetService<T>(), Is.Null, "Access to this service is disabled" );
                
                baseProvider.Remove( typeof( T ) );
                Assert.That( container.GetService<T>(), Is.Null, "Access to this service is disabled & The service doesn't exist anymore on the baseProvider" );

                container.Remove( typeof( T ) );
                Assert.That( container.GetService<T>(), Is.Null, "The service doesn't exist anymore on the baseProvider");
                
                baseProvider.Add( baseProviderServiceToTest, null );
                Assert.That( container.GetService<T>(), Is.Not.Null,"Back to the beginning's state, the service is retrieved from the base provider." );
            }
        }

        private static void IServiceContainerConformanceRemoveRecusive( ISimpleServiceContainer container )
        {
            bool removedCall = false;
            container.Add<IAddService>( new AddServiceImpl(), s => { removedCall = true; container.Remove( typeof( IAddService ) ); } );
            Assert.That( container.GetService<IAddService>(), Is.Not.Null );
            container.Remove( typeof( IAddService ) );
            Assert.That( removedCall, "OnRemove has been called and can safely remove the service again without stack overflow exception." );
        }

        private static void IServiceContainerConformanceAddFailsWhenExisting( ISimpleServiceContainer container, Func<IAddService> creatorFunc )
        {
            container.Add<IAddService>( new AddServiceImpl() );
            Assert.Throws<CKException>( () => container.Add( creatorFunc ) );
            Assert.Throws<CKException>( () => container.Add<IAddService>( creatorFunc, s => { } ) );
            Assert.Throws<CKException>( () => container.Add( typeof( IAddService ), new AddServiceImpl() ) );
            Assert.Throws<CKException>( () => container.Add( typeof( IAddService ), new AddServiceImpl(), s => { } ) );
            Assert.Throws<CKException>( () => container.Add<IAddService>( new AddServiceImpl() ) );
            Assert.Throws<CKException>( () => container.Add<IAddService>( new AddServiceImpl(), s => { } ) );
            Assert.Throws<CKException>( () => container.AddDisabled( typeof( IAddService ) ) );
            container.Remove( typeof( IAddService ) );
        }

        private static void IServiceContainerConformanceAddRemove( ISimpleServiceContainer container, Func<IAddService> creatorFunc )
        {
            Assert.That( container.GetService<IAddService>() == null, "Starting with no IAddService." );

            container.Add<IAddService>( creatorFunc );
            Assert.Throws<CKException>( () => container.Add<IAddService>( creatorFunc ), "Adding an already existing service throws an exception." );

            Assert.That( container.GetService<IAddService>() != null, "Deferred creation occured." );
            container.Remove( typeof( IAddService ) );
            Assert.That( container.GetService<IAddService>() == null, "Remove works." );

            // Removing an unexisting service is okay.
            container.Remove( typeof( IAddService ) );

            bool removed = false;
            container.Add<IAddService>( creatorFunc, s => removed = true );
            container.Remove( typeof( IAddService ) );
            Assert.That( removed == false, "Since the service has never been required, it has not been created, hence, OnRemove action has not been called." );

            container.Add<IAddService>( creatorFunc, s => removed = true );
            Assert.That( container.GetService<IAddService>() != null, "Service has been created." );
            container.Remove( typeof( IAddService ) );
            Assert.That( removed, "This time, OnRemove action has been called." );

            removed = false;
            container.Add<IAddService>( new AddServiceImpl(), s => removed = true );
            container.Remove( typeof( IAddService ) );
            Assert.That( removed, "Since the service instance has been added explicitely, OnRemove action has been called." );
        }

        private static void IServiceContainerCoAndContravariance( ISimpleServiceContainer container, Func<IAddService> creatorFunc )
        {
            {
                _onRemoveServiceCalled = false;
                container.Add<IAddService>( new AddServiceImpl(), OnRemoveService );
                container.Remove( typeof( IAddService ) );
                Assert.That( _onRemoveServiceCalled, "OnRemoveService has been called." );

                _onRemoveServiceCalled = false;
                container.Add<IAddService>( new AddServiceImpl(), OnRemoveBaseServiceType );
                container.Remove( typeof( IAddService ) );
                Assert.That( _onRemoveServiceCalled, "OnRemoveBaseServiceType has been called." );

                _onRemoveServiceCalled = false;
                container.Add<IAddService>( new AddServiceImpl(), OnRemoveServiceObject );
                container.Remove( typeof( IAddService ) );
                Assert.That( _onRemoveServiceCalled, "OnRemoveServiceObject has been called." );

                //container.Add<IAddService>( new AddServiceImpl(), OnRemoveDerivedServiceType );
                //container.Remove( typeof( IAddService ) );

                //container.Add<IAddService>( new AddServiceImpl(), OnRemoveUnrelatedType );
                //container.Remove( typeof( IAddService ) );
            }
            {
                _onRemoveServiceCalled = false;
                container.Add( creatorFunc, OnRemoveService );
                container.Remove( typeof( IAddService ) );
                Assert.That( !_onRemoveServiceCalled, "Service has never been created." );

                container.Add<IAddService>( creatorFunc, OnRemoveBaseServiceType );
                container.Remove( typeof( IAddService ) );
                Assert.That( !_onRemoveServiceCalled, "Service has never been created." );

                container.Add<IAddService>( creatorFunc, OnRemoveServiceObject );
                container.Remove( typeof( IAddService ) );
                Assert.That( !_onRemoveServiceCalled, "Service has never been created." );
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

        static void OnRemoveDerivedServiceType( IAddServiceDerived derivedType )
        {
        }

        static void OnRemoveUnrelatedType( string unrelatedType )
        {
        }

    
    }
}
